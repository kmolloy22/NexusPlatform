---
description: 'Nexus Platform development guidelines for .NET 9 microservices'
applyTo: '**/*.cs, **/*.csproj'
---

# Nexus Platform Development Guidelines

## Project Context
.NET 9 microservices using Clean Architecture, CQRS with MediatR, Azure Table Storage, deployed on Azure Container Apps.

## Core Technology Standards

- Target Framework: net9.0 with nullable reference types enabled
- Use C# 12 features: primary constructors, collection expressions
- Azure.Data.Tables for all storage operations
- MediatR for CQRS pattern
- xUnit + FluentAssertions + NSubstitute for testing
- OpenTelemetry for observability

## Architecture Rules (Always Enforce)

### Layer Dependencies
- ✅ API → Application → Domain (one-way flow)
- ✅ Infrastructure implements interfaces from Application/Domain
- ❌ Never reference Infrastructure from Domain
- ❌ Never put business logic in API layer
- ❌ No circular dependencies between projects

### CQRS Pattern
- ✅ Use MediatR for all business operations
- ✅ Commands for state changes, Queries for data retrieval
- ✅ Handlers contain business logic, not endpoints
- ✅ One handler per command/query

### Repository Pattern
- ✅ Use `ITableClient<TConfig, TEntity>` for Azure Table Storage
- ✅ Define repository interfaces in Application layer
- ✅ Implement repositories in Infrastructure layer
- ✅ Inject repositories into handlers via constructor

## Async & Cancellation (Critical)

- All I/O operations must be async
- Every async method signature: `async Task<T> MethodAsync(..., CancellationToken cancellationToken = default)`
- Always propagate CancellationToken through the call chain
- Never use `Task.Result` or `.Wait()` - always await

## Naming Conventions (Strict)

### Namespaces
`Nexus.{ServiceName}.{Layer}.Features.{FeatureName}`

Example: `Nexus.CustomerOrder.Application.Features.Accounts`

### CQRS Components
- Commands: `{Verb}{Entity}Command` → `CreateAccountCommand`, `UpdateAccountCommand`
- Queries: `Get{Entity}{Context}Query` → `GetAccountByIdQuery`, `GetAccountsQuery`
- Handlers: `{Verb}{Entity}Handler` → `CreateAccountHandler`
- DTOs: `{Entity}{Purpose}Dto` → `CreateAccountDto`, `AccountResponseDto`

### API Components
- Endpoints: `Map{Verb}{Entity}Endpoint` → `MapCreateAccountEndpoint`
- Groups: `{Entity}Endpoints` → static class with all endpoint methods

### Data Access
- Interfaces: `I{Entity}Repository` → `IAccountRepository`
- Implementations: `{Entity}Repository` → `AccountRepository`
- Table Entities: `{Entity}TableEntity` → `AccountTableEntity`
- Configs: `{Entity}TableStorageConfiguration` → `AccountsTableStorageConfiguration`

## Code Generation Workflow

When creating a new feature, generate in this order:

1. **Domain Entity** (if needed) - Domain/{Feature}/{Entity}.cs
2. **Command/Query** - Application/Features/{Feature}/{Action}{Entity}Command.cs
3. **Handler** - Application/Features/{Feature}/{Action}{Entity}Handler.cs
4. **Repository Interface** - Application/Features/{Feature}/I{Entity}Repository.cs
5. **Repository Implementation** - Application/Features/{Feature}/Infrastructure/StorageAccount/{Entity}Repository.cs
6. **Table Entity** - Application/Features/{Feature}/Infrastructure/StorageAccount/{Entity}TableEntity.cs
7. **API Endpoint** - Api/Features/{Feature}/{Action}{Entity}Endpoint.cs
8. **DI Registration** - Update Program.cs or extension methods

Always use primary constructors for dependency injection.

## Required Code Patterns

### MediatR Handler (Primary Constructor)
```csharp
public class CreateAccountHandler(IAccountRepository repository) 
    : IRequestHandler<CreateAccountCommand, Guid>
{
    public async Task<Guid> Handle(CreateAccountCommand request, 
        CancellationToken cancellationToken)
    {
        // Business logic here
    }
}
```

### Minimal API Endpoint
```csharp
app.MapPost("/accounts", async (
    [FromBody] CreateAccountDto dto,
    [FromServices] IMediator mediator,
    CancellationToken cancellationToken) =>
{
    var command = new CreateAccountCommand(dto.Name, dto.Email);
    var id = await mediator.Send(command, cancellationToken);
    return Results.Created($"/accounts/{id}", new { id });
})
.WithName("CreateAccount")
.WithOpenApi();
```

### Repository with Primary Constructor
```csharp
internal class AccountRepository(
    ITableClient<AccountsTableStorageConfiguration, AccountTableEntity> tableClient) 
    : IAccountRepository
{
    // Implementation using tableClient
}
```

### Table Entity Structure
```csharp
public sealed class AccountTableEntity : ITableEntity
{
    public const string DefaultPartitionKey = "accounts";
    
    public string PartitionKey { get; set; } = DefaultPartitionKey;
    public string RowKey { get; set; } = default!;
    
    // Properties here
    // Flatten complex objects: Address_Street, Address_City
    
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
```

## Error Handling

- Use `Maybe<T>` from Nexus.Shared.Core for nullable returns
- Throw specific exceptions: `ArgumentNullException`, `InvalidOperationException`, `KeyNotFoundException`
- Never throw generic `Exception`
- Return proper HTTP status codes: 200 OK, 201 Created, 400 BadRequest, 404 NotFound
- Error messages should be helpful but not expose internals

## Testing Requirements

When generating or modifying code, ALWAYS offer to create unit tests.

### Test Standards
- Framework: xUnit with `[Fact]` and `[Theory]`
- Assertions: FluentAssertions (`.Should()` syntax)
- Mocking: NSubstitute
- Naming: `MethodName_WhenCondition_ThenExpectedBehavior`
- Structure: Arrange, Act, Assert (blank lines between)
- Coverage: All public methods, edge cases, null checks

### Test Example Pattern
```csharp
[Fact]
public async Task Handle_WhenValidCommand_ThenCreatesAccount()
{
    // Arrange
    var repository = Substitute.For<IAccountRepository>();
    var handler = new CreateAccountHandler(repository);
    var command = new CreateAccountCommand("Test", "test@example.com");

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeEmpty();
    await repository.Received(1).CreateAsync(Arg.Any<Account>(), Arg.Any<CancellationToken>());
}
```

## Code Quality Checklist

Before completing any code generation:
- [ ] XML documentation on public APIs
- [ ] All async methods have CancellationToken parameter
- [ ] Using nullable reference types correctly (no null warnings)
- [ ] Primary constructors for dependency injection
- [ ] No business logic in API endpoints
- [ ] Repository interfaces in Application, implementations in Infrastructure
- [ ] Proper error handling with specific exceptions
- [ ] Tests offered for new/modified code

## Azure & Deployment

- Configure resources via .NET Aspire AppHost
- Include health checks for Container Apps
- Use OpenTelemetry for distributed tracing
- Follow 12-factor app principles
- Environment-specific settings in appsettings.{Environment}.json

## What NOT to Do

- ❌ Don't use `Task.Result` or `.Wait()` - causes deadlocks
- ❌ Don't ignore CancellationToken - breaks graceful shutdown
- ❌ Don't put queries in command handlers or vice versa
- ❌ Don't create anemic domain models (logic belongs in domain)
- ❌ Don't use reflection for configuration - use strongly-typed classes
- ❌ Don't log sensitive data (passwords, tokens, PII)

---

For detailed examples, examine existing features in the codebase. This file contains rules, not comprehensive templates.