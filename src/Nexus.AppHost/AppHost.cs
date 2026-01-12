using Azure.Provisioning.AppContainers;
using Azure.Provisioning.Storage;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage")
                .ConfigureInfrastructure(infra =>
                {
                    var resources = infra.GetProvisionableResources();
                    var storageAccount = resources.OfType<StorageAccount>()
                                                  .Single();
                    storageAccount.AllowBlobPublicAccess = true;
                })
                .RunAsEmulator(storage =>
                {
                    storage.WithBlobPort(11000);
                    storage.WithTablePort(11002);
                    storage.WithImageTag("3.34.0");
                    storage.WithDataVolume();
                    storage.WithLifetime(ContainerLifetime.Persistent);
                });

var blobs = storage.AddBlobs("Blobs");
var tables = storage.AddTables("Tables");

var healthPort = 8081;

// API Project
var api = builder.AddProject<Nexus_CustomerOrder_Api>("nexus-customer-order-api")
                .WithReference(blobs)
                .WithReference(tables)
                .WaitFor(blobs)
                .WaitFor(tables)
                .WithExternalHttpEndpoints()
                .WithEnvironment("HTTP_PORTS", $"8080;{healthPort.ToString()}");

// Blazor Web Project - automatically gets API URL via service discovery
var web = builder.AddProject<Nexus_Web>("nexus-customer-order-web")
                .WithReference(api)  // ← API URL auto-injected!
                .WithExternalHttpEndpoints();

// Only add Azure deployment config in publish mode
if (builder.ExecutionContext.IsPublishMode)
{
    api.PublishAsAzureContainerApp((infra, containerApp) =>
    {
        var container = containerApp.Template.Containers.Single().Value;

        container?.Probes.Add(new ContainerAppProbe
        {
            ProbeType = ContainerAppProbeType.Liveness,
            HttpGet = new ContainerAppHttpRequestInfo
            {
                Path = "/health/alive",
                Port = healthPort,
                Scheme = ContainerAppHttpScheme.Http
            },
            PeriodSeconds = 10
        });

        container?.Probes.Add(new ContainerAppProbe
        {
            ProbeType = ContainerAppProbeType.Readiness,
            HttpGet = new ContainerAppHttpRequestInfo
            {
                Path = "/health/ready",
                Port = healthPort,
                Scheme = ContainerAppHttpScheme.Http
            },
            PeriodSeconds = 10
        });

        containerApp.Template.Scale.MinReplicas = 0;
        containerApp.Template.Scale.MaxReplicas = 10;
    })
                .WithEnvironment("HTTP_PORTS", $"8080;{healthPort.ToString()}");

    // Blazor Web Project
    web = builder.AddProject<Nexus_Web>("nexus-customer-order-web")
                    .WithReference(api)  // Service discovery - automatically configures ApiBaseUrl
                    .WithExternalHttpEndpoints()
                    .PublishAsAzureContainerApp((infra, containerApp) =>
                    {
                        containerApp.Template.Scale.MinReplicas = 0;
                        containerApp.Template.Scale.MaxReplicas = 5;
                    });
}

builder.Build().Run();