using FluentValidation;
using Nexus.CustomerOrder.Api.Features.Accounts;
using Nexus.CustomerOrder.Application.Features.Accounts;
using Nexus.CustomerOrder.Application.Features.Accounts.Extensions;
using Nexus.CustomerOrder.Application.Features.Accounts.Validation;

// Access configuration via builder.Configuration instead of sp.Configuration
var builder = WebApplication.CreateBuilder(args);

// Get the "tables" connection string and also expose it as "Storage:ConnectionString"
// so the TableClient<TConfig, TEntity> (via TableStorageConfiguration) can use it.
//var tablesCs = builder.Configuration.GetConnectionString("tables")
//             ?? throw new InvalidOperationException("Missing connection string 'tables'.");
//builder.Configuration["Storage:ConnectionString"] = tablesCs;

// Register TableServiceClient for the ping endpoint (optional utility)
//builder.Services.AddSingleton(new TableServiceClient(tablesCs));

// Register MediatR � scan Application assembly for handlers
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateAccountHandler).Assembly));

// Register the Create Account feature (adds ITableClient<AccountsTableStorageConfiguration, AccountTableEntity>)
builder.Services.AddAccountsInfrastructure();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateAccountDtoValidator>();

var app = builder.Build();

app.MapAccounts();

app.Run();

public partial class Program
{ }