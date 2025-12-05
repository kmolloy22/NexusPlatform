# Nexus Platform Architecture

## Overview

The Nexus Platform is a cloud-native microservices application built on .NET 9 using Clean Architecture principles. The platform is designed for scalability, maintainability, and cloud-first deployment on Azure.

## Architectural Patterns

### Clean Architecture

The solution follows Robert C. Martin's Clean Architecture pattern with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                   │
│              (Nexus.CustomerOrder.Api)                  │
├─────────────────────────────────────────────────────────┤
│                   Application Layer                     │
│            (Nexus.CustomerOrder.Application)            │
├─────────────────────────────────────────────────────────┤
│                     Domain Layer                        │
│             (Nexus.CustomerOrder.Domain)                │
├─────────────────────────────────────────────────────────┤
│                 Infrastructure Layer                    │
│          (Nexus.Infrastructure.StorageAccount)          │
└─────────────────────────────────────────────────────────┘
```

### Dependency Flow
- **Inward Dependencies**: All dependencies flow inward toward the Domain layer
- **Domain Layer**: Contains no external dependencies, only pure business logic
- **Application Layer**: Orchestrates domain operations and defines contracts
- **Infrastructure Layer**: Implements external concerns (storage, APIs, etc.)
- **Presentation Layer**: Handles HTTP concerns and user interface

## Project Structure

### Core Services

#### Domain Layer (`Nexus.CustomerOrder.Domain`)
**Purpose**: Contains core business logic and domain entities
- **Entities**: `Account`, `Address` 
- **Value Objects**: Immutable domain concepts
- **Business Rules**: Domain-specific validation and logic

#### Application Layer (`Nexus.CustomerOrder.Application`)
**Purpose**: Application-specific business logic and orchestration
- **Use Cases**: Command and Query handlers
- **DTOs**: Data transfer objects for API contracts
- **Ports**: Interfaces for external dependencies (Repository pattern)
- **Adapters**: Infrastructure implementations

#### Presentation Layer (`Nexus.CustomerOrder.Api`)
**Purpose**: HTTP API endpoints and request/response handling
- **Endpoints**: Feature-based organization
- **Validation**: Request validation and error handling
- **Serialization**: JSON serialization/deserialization

### Infrastructure

#### Storage Infrastructure (`Nexus.Infrastructure.StorageAccount`)
**Purpose**: Azure Storage abstractions and implementations
- **Generic Table Client**: Reusable Azure Table Storage client
- **Configuration Management**: Storage connection and retry policies
- **Repository Implementations**: Data access layer

#### Shared Libraries

##### Core Utilities (`Nexus.Shared.Core`)
- **Maybe Monad**: Null-safe value handling
- **Common Extensions**: Reusable utility methods

##### Service Defaults (`Nexus.ServiceDefaults`)
- **OpenTelemetry Configuration**: Distributed tracing setup
- **Health Checks**: Application health monitoring
- **Service Discovery**: Inter-service communication

##### Test Utilities (`Nexus.Shared.Core.Tests`)
- **Test Helpers**: Common testing utilities
- **Random Data Generators**: Test data creation

### Hosting and Orchestration

#### App Host (`Nexus.AppHost`)
**Purpose**: .NET Aspire orchestration and cloud deployment configuration
- **Service Orchestration**: Local development coordination
- **Azure Resource Definition**: Cloud infrastructure as code
- **Environment Configuration**: Multi-environment support

## Data Architecture

### Azure Table Storage Design

#### Account Entity Structure
```csharp
public class AccountTableEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "accounts";
    public string RowKey { get; set; }  // Account GUID
    
    // Account Properties
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    
    // Flattened Address Properties
    public string Address_Street1 { get; set; }
    public string Address_Street2 { get; set; }
    public string Address_City { get; set; }
    public string Address_State { get; set; }
    public string Address_PostalCode { get; set; }
    public string Address_Country { get; set; }
}
```

#### Partitioning Strategy
- **Single Partition**: All accounts use partition key `"accounts"`
- **Row Key**: GUID-based for unique identification
- **Scalability Consideration**: May require partition splitting for large datasets

### Generic Table Client Pattern

The infrastructure provides a generic table client that can be reused across different entities:

```csharp
public interface ITableClient<TTableStorageConfiguration, TTableEntity>
    where TTableStorageConfiguration : ITableStorageConfiguration
    where TTableEntity : ITableEntity
{
    Task<Maybe<TTableEntity>> GetByIdAsync(string partitionKey, string rowKey);
    Task<Response> AddAsync(TTableEntity entity);
    Task<Response> UpsertAsync(TTableEntity entity);
    Task<Response> DeleteAsync(TTableEntity entity);
    AsyncPageable<TTableEntity> QueryAsync(Expression<Func<TTableEntity, bool>> filter);
}
```

## Communication Patterns

### CQRS with MediatR

The application layer implements Command Query Responsibility Segregation (CQRS) using MediatR:

#### Commands (Write Operations)
```csharp
public record CreateAccountCommand(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    Address Address
) : IRequest<Guid>;
```

#### Queries (Read Operations)
```csharp
public record GetAccountsCommand(int PageSize, string ContinuationToken)
    : IRequest<PagedResult<GetAccountDto>>;
```

#### Handler Pattern
```csharp
public class CreateAccountHandler : IRequestHandler<CreateAccountCommand, Guid>
{
    private readonly IAccountRepository _repository;
    
    public async Task<Guid> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        // Business logic implementation
    }
}
```

## API Design

### RESTful Endpoints

The API follows REST conventions with feature-based organization:

```
GET    /api/accounts           - List accounts (paginated)
POST   /api/accounts           - Create new account
GET    /api/accounts/{id}      - Get specific account
PUT    /api/accounts/{id}      - Update account
DELETE /api/accounts/{id}      - Delete account
```

### Minimal API Implementation

Using ASP.NET Core Minimal APIs with feature grouping:

```csharp
public static class AccountsEndpoint
{
    public static void MapAccounts(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/accounts").WithTags("Accounts");
        
        group.MapCreateAccountEndpoint();
        group.MapGetAccountsEndpoint();
        // Additional endpoints...
    }
}
```

## Cloud Architecture

### Azure Container Apps Deployment

The application deploys to Azure Container Apps with the following configuration:

#### Container Configuration
- **Base Image**: .NET 9 runtime
- **Port Configuration**: HTTP (8080) and Health Check (8081)
- **Scaling**: 0-10 replicas based on load
- **Health Checks**: Liveness and readiness probes

#### Resource Dependencies
- **Azure Storage Account**: Table and blob storage
- **Container Environment**: Managed by Azure Container Apps Environment
- **Application Insights**: Observability and monitoring

### Infrastructure as Code

The infrastructure is defined using .NET Aspire and Azure Bicep:

```csharp
var storage = builder.AddAzureStorage("storage")
    .ConfigureInfrastructure(infra =>
    {
        var storageAccount = infra.GetProvisionableResources()
                                  .OfType<StorageAccount>()
                                  .Single();
        storageAccount.AllowBlobPublicAccess = true;
    });

var api = builder.AddProject<Nexus_CustomerOrder_Api>("nexus-customer-order-api")
    .WithReference(storage.AddTables("Tables"))
    .PublishAsAzureContainerApp();
```

## Security Considerations

### Authentication and Authorization
- **Azure Entra ID**: Identity provider configuration
- **Federated Credentials**: GitHub Actions integration
- **Managed Identity**: Secure access to Azure resources

### Data Security
- **Connection Strings**: Secured via Azure Key Vault or configuration
- **HTTPS Enforcement**: All external communication encrypted
- **Input Validation**: Request validation at API boundaries

## Observability

### OpenTelemetry Integration

The platform includes comprehensive observability:

#### Tracing
- **Distributed Tracing**: Cross-service request correlation
- **Custom Spans**: Business operation tracking
- **Performance Monitoring**: Request duration and throughput

#### Metrics
- **ASP.NET Core Metrics**: HTTP request metrics
- **Custom Metrics**: Business-specific measurements
- **Resource Utilization**: Container and storage metrics

#### Logging
- **Structured Logging**: JSON-formatted log entries
- **Correlation IDs**: Request tracking across services
- **Log Aggregation**: Azure Application Insights integration

### Health Checks

Multi-level health monitoring:

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"])
    .AddCheck<StorageHealthCheck>("storage", ["ready"]);
```

## Testing Strategy

### Test Architecture Layers

#### Unit Tests
- **Domain Logic**: Pure business rule validation
- **Application Handlers**: Use case testing with mocked dependencies
- **Infrastructure**: Repository and client testing

#### Integration Tests
- **API Endpoints**: Full request/response cycle testing
- **Database Integration**: Real storage operations
- **Configuration Testing**: Environment-specific behavior

#### Test Organization
```
tests/
├── Nexus.CustomerOrder.Api.Tests.Units/
│   ├── Features/Accounts/           # Feature-specific API tests
│   └── Shared/ApiFactory.cs         # Test infrastructure
└── Nexus.Infrastructure.StorageAccount.Tests.Units/
    ├── Tables/TableClientTests.cs   # Storage client tests
    └── ClientTestsBase.cs           # Test base classes
```

## Development Workflow

### Local Development Environment

#### Prerequisites
- .NET 9 SDK
- Docker (for Azurite storage emulator)
- Azure Developer CLI

#### Local Services
```bash
# Azure Storage Emulator
docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 \
  mcr.microsoft.com/azure-storage/azurite

# Application
dotnet run --project src/Nexus.AppHost
```

### Deployment Pipeline

#### CI/CD with GitHub Actions
1. **Build**: Restore, build, and test
2. **Infrastructure**: Provision Azure resources
3. **Deploy**: Container deployment to Azure Container Apps

#### Environment Management
- **Development**: Local development with emulators
- **Test**: Automated testing environment
- **Production**: Production Azure deployment

## Scalability Considerations

### Horizontal Scaling
- **Container Apps**: Auto-scaling based on CPU/memory/custom metrics
- **Storage**: Azure Table Storage scales automatically
- **Stateless Design**: No server-side session state

### Performance Optimization
- **Caching**: In-memory caching for frequently accessed data
- **Connection Pooling**: Efficient database connection management
- **Asynchronous Operations**: Non-blocking I/O operations

### Monitoring and Alerting
- **Performance Thresholds**: Automated alerting on performance degradation
- **Resource Monitoring**: CPU, memory, and storage utilization tracking
- **Business Metrics**: Custom metrics for domain-specific monitoring

## Future Enhancements

### Planned Improvements
- **Event-Driven Architecture**: Azure Service Bus integration
- **CQRS Read Models**: Dedicated read stores for complex queries
- **Multi-Tenancy**: Tenant isolation and data segregation
- **Advanced Security**: OAuth 2.0/OpenID Connect implementation

### Architectural Evolution
- **Microservice Decomposition**: Service boundary refinement
- **Domain Event Publishing**: Cross-service communication
- **API Gateway**: Centralized API management
- **Service Mesh**: Advanced inter-service communication