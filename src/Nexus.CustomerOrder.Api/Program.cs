using FluentValidation;
using Nexus.CustomerOrder.Api.Features.Accounts;
using Nexus.CustomerOrder.Application.Features.Accounts;
using Nexus.CustomerOrder.Application.Features.Accounts.Extensions;
using Nexus.CustomerOrder.Application.Features.Catalog.Extensions;
using Nexus.CustomerOrder.Application.Features.Accounts.Validation;
using Nexus.CustomerOrder.Application.Features.Catalog;
using Nexus.Infrastructure.StorageAccount;
using Nexus.CustomerOrder.Application.Features.Catalog.Validation;
using Nexus.CustomerOrder.Api.Features.Catalog;

// Access configuration via builder.Configuration instead of sp.Configuration
var builder = WebApplication.CreateBuilder(args);

// Register MediatR – scan Application assembly for handlers
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateAccountHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(CreateProductHandler).Assembly);
});

builder.Services.AddPartitioningStrategy(builder.Configuration);

// Register the Create Account feature (adds ITableClient<AccountsTableStorageConfiguration, AccountTableEntity>)
builder.Services.AddAccountsInfrastructure();
builder.Services.AddCatalogInfrastructure();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateAccountDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();

var app = builder.Build();

app.MapAccounts();
app.MapProducts();

app.Run();

public partial class Program
{ }