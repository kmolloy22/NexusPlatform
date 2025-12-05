# GitHub Copilot Instructions for Nexus Platform

## Project Overview
Nexus Platform is a .NET 9 microservices application using Clean Architecture, deployed on Azure Container Apps with Azure Table Storage.

## Architecture Patterns
- **Clean Architecture**: Domain → Application → Infrastructure → API layers
- **CQRS**: Commands and Queries with MediatR handlers
- **Repository Pattern**: Generic `ITableClient<TConfig, TEntity>` for Azure Table Storage
- **Feature Organization**: Group related functionality together

## Code Generation Guidelines

### Naming Conventions
- Namespace pattern: `Nexus.{ServiceName}.{Layer}`
- Commands: `{Action}{Entity}Command` (e.g., `CreateAccountCommand`)
- Queries: `{Action}{Entity}Query` (e.g., `GetAccountsQuery`)
- Handlers: `{Action}{Entity}Handler` (e.g., `CreateAccountHandler`)
- Endpoints: `{Action}{Entity}Endpoint` (e.g., `CreateAccountEndpoint`)

### MediatR Handlers
```csharp
// Pattern for command handlers
public class CreateAccountHandler : IRequestHandler<CreateAccountCommand, Guid>
{
    private readonly IAccountRepository _repository;
    
    public CreateAccountHandler(IAccountRepository repository) => _repository = repository;
    
    public async Task<Guid> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

### Minimal API Endpoints
```csharp
// Pattern for endpoint registration
public static void Map{Feature}Endpoint(this IEndpointRouteBuilder app)
{
    app.MapPost("/", async ([FromBody] CreateDto dto, [FromServices] IMediator mediator) =>
    {
        var command = new CreateCommand(/* map dto properties */);
        var result = await mediator.Send(command);
        return Results.Created($"/{result}", new { id = result });
    })
    .WithName("Create{Entity}")
    .WithSummary("Creates a new {entity}")
    .WithDescription("Detailed description of the endpoint");
}
```

### Repository Implementation
```csharp
// Pattern for repository classes
internal class AccountRepository : IAccountRepository
{
    private readonly ITableClient<AccountsTableStorageConfiguration, AccountTableEntity> _tableClient;
    
    public AccountRepository(ITableClient<AccountsTableStorageConfiguration, AccountTableEntity> tableClient)
    {
        _tableClient = tableClient;
    }
    
    public async Task<EntityType> MethodAsync(Parameters, CancellationToken cancellationToken = default)
    {
        // Implementation using _tableClient
    }
}
```

### Azure Table Storage Entities
```csharp
// Pattern for table entities
public sealed class EntityTableEntity : ITableEntity
{
    public const string DefaultPartitionKey = "entityname";
    
    public string PartitionKey { get; set; } = DefaultPartitionKey;
    public string RowKey { get; set; } = default!;
    
    // Entity properties
    // For complex objects, use flattened properties (Address_Street1, Address_City, etc.)
    
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
```

## Technology Stack Requirements
- **.NET 9** target framework
- **Nullable reference types** enabled
- **Azure.Data.Tables** for storage
- **MediatR** for CQRS
- **xUnit + FluentAssertions** for testing
- **OpenTelemetry** for observability

## Project Structure Context
```
src/
├── Nexus.AppHost/                          # .NET Aspire orchestration
├── Nexus.ServiceDefaults/                  # Common service configurations
├── Nexus.CustomerOrder.Api/               # API endpoints (Minimal APIs)
├── Nexus.CustomerOrder.Application/       # Business logic (MediatR handlers)
├── Nexus.CustomerOrder.Domain/            # Domain entities and logic
├── Nexus.Infrastructure.StorageAccount/   # Azure Storage abstractions
├── Nexus.Shared.Core/                     # Common utilities (Maybe<T>)
└── Nexus.Shared.Kernel/                   # Serialization and extensions

tests/
├── Nexus.CustomerOrder.Api.Tests.Units/
└── Nexus.Infrastructure.StorageAccount.Tests.Units/
```

## Testing Patterns
- Use **AAA pattern** (Arrange, Act, Assert)
- **Integration tests**: Use `WebApplicationFactory` and real storage
- **Unit tests**: Mock dependencies with NSubstitute
- **Test naming**: `MethodName_WhenCondition_ThenExpectedResult`

## Error Handling
- Use `Maybe<T>` for nullable returns
- Return proper HTTP status codes (201 for Created, 404 for NotFound, etc.)
- Include meaningful error messages without exposing sensitive data

## Azure Integration
- Configure resources via `.NET Aspire` in `AppHost.cs`
- Use **health checks** for liveness and readiness probes
- Follow **Azure Container Apps** deployment patterns
- Implement **OpenTelemetry** tracing and metrics

## Code Quality Rules
- Follow `.editorconfig` formatting rules
- Include **XML documentation** for public APIs
- Use **async/await** for I/O operations
- Apply **SOLID principles** and Clean Architecture
- Maintain **immutability** where appropriate

## Common Anti-Patterns to Avoid
- Don't put business logic in API controllers
- Don't reference infrastructure from domain layer
- Don't use generic Exception types
- Don't ignore cancellation tokens
- Don't create circular dependencies between layers

## Reference Files for Patterns
- Domain Entity: `src/Nexus.CustomerOrder.Domain/Features/Accounts/Account.cs`
- MediatR Handler: `src/Nexus.CustomerOrder.Application/Features/Accounts/CreateAccountHandler.cs`
- Repository: `src/Nexus.CustomerOrder.Application/Features/Accounts/Infrastructure/StorageAccount/AccountRepository.cs`
- API Endpoint: `src/Nexus.CustomerOrder.Api/Features/Accounts/AccountCreate/CreateAccountEndpoint.cs`
- Table Entity: `src/Nexus.CustomerOrder.Application/Features/Accounts/Infrastructure/StorageAccount/AccountTableEntity.cs`

For comprehensive guidelines and templates, see: `docs/PROMPT_GUIDE.md`