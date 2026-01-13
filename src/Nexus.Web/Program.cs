using MudBlazor.Services;
using Nexus.Web.Components;
using Nexus.Web.Services;
using Refit;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults first
builder.AddServiceDefaults();

// Add Blazor services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add MudBlazor with default configuration
builder.Services.AddMudServices();

// Configure API Client with Refit + Service Discovery
// When running via AppHost, "http://nexus-customer-order-api" resolves automatically
// When running standalone, falls back to ApiBaseUrl from appsettings.json
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? "http://nexus-customer-order-api"; // Service discovery name from AppHost

builder.Services.AddRefitClient<IAccountsApiClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddStandardResilienceHandler(); // Retry, timeout, circuit breaker
                                     //.AddServiceDiscovery(); // ← Enables service discovery!

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();