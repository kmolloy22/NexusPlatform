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
- **Partition Strategy**: Hash-based partitioning for horizontal scalability

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
    // Hash-based partition key (e.g., "ACC-001" through "ACC-100")
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; }  // Account GUID
    
    // Account Properties
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public bool? IsActive { get; set; }
    
    // Flattened Address Properties
    public string Address_Street1 { get; set; }
    public string Address_Street2 { get; set; }
    public string Address_City { get; set; }
    public string Address_State { get; set; }
    public string Address_PostalCode { get; set; }
    public string Address_Country { get; set; }
    
    // Metadata
    public DateTime CreatedUtc { get; set; }
    public DateTime? ModifiedUtc { get; set; }
    public int PartitionStrategyVersion { get; set; } = 1;
    
    // Azure Table Storage properties
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
```

#### Hash-Based Partitioning Strategy

**Overview**: The platform uses hash-based partitioning to distribute account entities evenly across multiple partitions, enabling horizontal scalability and high throughput.

**Partition Key Format**: `{Prefix}-{HashNumber}` (e.g., "ACC-000" through "ACC-099")

**Key Characteristics**:
- **Deterministic**: Same GUID always maps to the same partition
- **Even Distribution**: Hash function ensures uniform distribution across partitions
- **Scalable**: Supports 100x more throughput than single partition (200,000+ ops/sec theoretical)
- **Consistent**: No dependency on business logic or temporal data

**Implementation**:
```csharp
public interface IPartitionKeyStrategy
{
    string GeneratePartitionKey(Guid entityId);
    IEnumerable<string> GetAllPartitionKeys();
    string GetPartitionKey(Guid entityId);
}

public class HashPartitionKeyStrategy : IPartitionKeyStrategy
{
    private readonly int _partitionCount;  // Default: 100
    private readonly string _prefix;       // Default: "ACC"
    
    public string GeneratePartitionKey(Guid entityId)
    {
        var hashCode = GetStableHashCode(entityId.ToString());
        var partitionNumber = Math.Abs(hashCode % _partitionCount);
        return $"{_prefix}-{partitionNumber:D3}";
    }
    
    // ... implementation details
}
```

**Configuration**:
```json
{
  "PartitionStrategy": {
    "PartitionCount": 100,
    "PartitionPrefix": "ACC"
  }
}
```

**Benefits**:
- **High Throughput**: Each partition supports ~2,000 ops/sec (100 partitions = 200,000 ops/sec)
- **No Hot Partitions**: Uniform distribution prevents bottlenecks
- **Future-Proof**: Can be extended with composite strategies (e.g., region-based prefixes)
- **Simple Migration**: Existing data can be migrated incrementally

**Trade-offs**:
- **List Queries**: Require scatter-gather across all partitions (mitigated by parallel queries)
- **Continuation Tokens**: More complex than single-partition pagination
- **Initial Migration**: One-time migration needed for existing single-partition data

**Example Distribution** (10,000 accounts):
```
Partition    Accounts    Throughput
ACC-000      ~100        2,000 ops/sec
ACC-001      ~100        2,000 ops/sec
...
ACC-099      ~100        2,000 ops/sec
────────────────────────────────────
Total        10,000      200,000 ops/sec
```

**Query Patterns**:

*Point Query (GetById)*:
```csharp
// 1. Calculate partition key from GUID
var partitionKey = _partitionStrategy.GetPartitionKey(accountId);

// 2. Single partition query (fast)
var entity = await _tableClient.GetByIdAsync(partitionKey, accountId);
```
**Performance**: ~10-20ms (same as single partition)

*List Query (GetAll)*:
```csharp
// 1. Query all partitions in parallel
var allPartitions = _partitionStrategy.GetAllPartitionKeys();
var tasks = allPartitions.Select(p => QueryPartitionAsync(p));
var results = await Task.WhenAll(tasks);

// 2. Merge, sort, and page results
var sorted = results.SelectMany(r => r).OrderBy(...).ToList();
```
**Performance**: ~100-300ms (parallelized across partitions)

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
    string? Email,
    string? Phone,
    bool? IsActive,
    Address Address
) : IRequest<CreateAccountResult>;

public sealed record CreateAccountResult(Guid Id, DateTimeOffset CreatedAt);
```

#### Queries (Read Operations)
```csharp
public record GetAccountsCommand(int PageSize, string? ContinuationToken)
    : IRequest<PagedResult<GetAccountDto>>;
```

#### Handler Pattern
```csharp
public class CreateAccountHandler : IRequestHandler<CreateAccountCommand, CreateAccountResult>
{
    private readonly IAccountRepository _repository;
    
    public async Task<CreateAccountResult> Handle(
        CreateAccountCommand request, 
        CancellationToken cancellationToken)
    {
        var account = new Account(
            Guid.NewGuid(),
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.IsActive,
            request.Address);
        
        await _repository.AddAsync(account, cancellationToken);
        
        return new CreateAccountResult(account.Id, DateTimeOffset.UtcNow);
    }
}
```

### Pagination Pattern

The platform uses a custom `PagedResult<T>` for pagination:
```csharp
public record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    string? ContinuationToken
);
```

**Benefits**:
- **Clean Separation**: No Azure SDK types in domain layer
- **Simple Testing**: Easy to mock and test
- **Type-Safe**: Compiler-verified contracts
- **Consistent**: Same pattern across all paged queries

**Usage**:
```csharp
// Repository returns PagedResult
public async Task<PagedResult<AccountTableEntity>> QueryAsync(
    int pageSize,
    string? continuationToken = null,
    CancellationToken cancellationToken = default)
{
    // Query logic with hash-based partition strategy
    var pagedEntities = await QueryWithScatterGatherAsync(pageSize, continuationToken);
    return new PagedResult<AccountTableEntity>(pagedEntities, nextToken);
}

// Handler maps to DTO
public async Task<PagedResult<GetAccountDto>> Handle(
    GetAccountsCommand request,
    CancellationToken cancellationToken)
{
    var entities = await _repository.QueryAsync(
        request.PageSize,
        request.ContinuationToken,
        cancellationToken);
    
    var dtos = entities.Items.Select(e => MapToDto(e)).ToList();
    return new PagedResult<GetAccountDto>(dtos, entities.ContinuationToken);
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
- **Azure Storage Account**: Table and blob storage with hash-based partitioning
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
- **Partition-Aware Tracing**: Track queries across partitions

#### Metrics
- **ASP.NET Core Metrics**: HTTP request metrics
- **Custom Metrics**: Business-specific measurements
- **Resource Utilization**: Container and storage metrics
- **Partition Metrics**: Per-partition throughput and distribution

#### Logging
- **Structured Logging**: JSON-formatted log entries
- **Correlation IDs**: Request tracking across services
- **Log Aggregation**: Azure Application Insights integration
- **Partition Context**: Log partition keys for debugging

### Health Checks

Multi-level health monitoring:
```csharp
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"])
    .AddCheck<StorageHealthCheck>("storage", ["ready"]);
```

### Partition Health Monitoring

Monitor partition distribution and performance:
- **Distribution Balance**: Ensure even distribution across partitions
- **Hot Partition Detection**: Alert on partitions exceeding thresholds
- **Query Performance**: Track scatter-gather query latency
- **Throughput Metrics**: Per-partition operations per second

## Testing Strategy

### Test Architecture Layers

#### Unit Tests
- **Domain Logic**: Pure business rule validation
- **Application Handlers**: Use case testing with mocked dependencies
- **Infrastructure**: Repository and client testing
- **Partition Strategy**: Hash distribution and consistency tests

#### Integration Tests
- **API Endpoints**: Full request/response cycle testing
- **Database Integration**: Real storage operations
- **Configuration Testing**: Environment-specific behavior
- **Partition Migration**: Validate data migration correctness

#### Test Organization
```
tests/
├── Nexus.CustomerOrder.Api.Tests.Units/
│   ├── Features/Accounts/           # Feature-specific API tests
│   └── Shared/ApiFactory.cs         # Test infrastructure
├── Nexus.Infrastructure.StorageAccount.Tests.Units/
│   ├── Tables/TableClientTests.cs   # Storage client tests
│   ├── Partitioning/HashPartitionKeyStrategyTests.cs  # Partition strategy tests
│   └── ClientTestsBase.cs           # Test base classes
└── tools/
    └── Nexus.Migration.Tool/        # Data migration utility
```

## Development Workflow

### Local Development Environment

#### Prerequisites
- .NET 9 SDK
- Docker (for Azurite storage emulator)
- Azure Developer CLI

#### Local Services
```bash
# Azure Storage Emulator via .NET Aspire
dotnet run --project src/Nexus.AppHost

# Or run Azurite directly
docker run -p 10000:10000 -p 10001:10001 -p 11002:11002 \
  mcr.microsoft.com/azure-storage/azurite
```

#### Local Configuration
```json
{
  "PartitionStrategy": {
    "PartitionCount": 10,      // Fewer partitions for local dev
    "PartitionPrefix": "ACC"
  },
  "Storage": {
    "ConnectionString": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=...;TableEndpoint=http://127.0.0.1:11002/devstoreaccount1;"
  }
}
```

### Deployment Pipeline

#### CI/CD with GitHub Actions
1. **Build**: Restore, build, and test
2. **Infrastructure**: Provision Azure resources
3. **Deploy**: Container deployment to Azure Container Apps
4. **Migration**: Run partition migration if needed

#### Environment Management
- **Development**: Local development with emulators (10 partitions)
- **Test**: Automated testing environment (50 partitions)
- **Production**: Production Azure deployment (100 partitions)

## Data Migration

### Partition Migration Strategy

When migrating from single partition to hash-based partitioning:

1. **Preparation**: Deploy new code with dual-write support
2. **Migration**: Run migration tool to redistribute data
3. **Validation**: Verify data consistency across partitions
4. **Cutover**: Switch to read from new partitions
5. **Cleanup**: Remove old partition data after safety period

**Migration Tool**: `tools/Nexus.Migration.Tool`
- Reads from old single partition
- Calculates new partition keys using hash strategy
- Writes to new partitions with metadata
- Supports dry-run mode and batch processing

**Usage**:
```bash
cd tools/Nexus.Migration.Tool

# Dry run
dotnet run -- --DryRun=true

# Actual migration
dotnet run -- --DryRun=false --BatchSize=100
```

## Scalability Considerations

### Horizontal Scaling
- **Container Apps**: Auto-scaling based on CPU/memory/custom metrics
- **Storage**: Azure Table Storage scales automatically across partitions
- **Stateless Design**: No server-side session state
- **Partition Strategy**: Supports 200,000+ operations per second

### Performance Optimization
- **Parallel Queries**: Scatter-gather queries execute in parallel
- **Connection Pooling**: Efficient database connection management
- **Asynchronous Operations**: Non-blocking I/O operations
- **Partition Caching**: Optional in-memory cache for frequent queries

### Monitoring and Alerting
- **Performance Thresholds**: Automated alerting on performance degradation
- **Resource Monitoring**: CPU, memory, and storage utilization tracking
- **Business Metrics**: Custom metrics for domain-specific monitoring
- **Partition Metrics**: Per-partition health and distribution monitoring

## Future Enhancements

### Planned Improvements
- **Event-Driven Architecture**: Azure Service Bus integration
- **CQRS Read Models**: Dedicated read stores for complex queries
- **Multi-Tenancy**: Tenant isolation and data segregation
- **Advanced Security**: OAuth 2.0/OpenID Connect implementation
- **Composite Partitioning**: Region + hash for global distribution

### Architectural Evolution
- **Microservice Decomposition**: Service boundary refinement
- **Domain Event Publishing**: Cross-service communication
- **API Gateway**: Centralized API management
- **Service Mesh**: Advanced inter-service communication
- **Partition Auto-Scaling**: Dynamic partition count adjustment

## Architecture Decision Records

### ADR-001: Hash-Based Partition Strategy

**Context**: Single partition limited Azure Table Storage throughput to ~2,000 operations per second, creating scalability bottleneck.

**Decision**: Implement hash-based partitioning with 100 partitions using stable GUID hashing.

**Consequences**:
- ✅ 100x throughput increase (theoretical 200,000 ops/sec)
- ✅ Predictable, even distribution across partitions
- ✅ No business logic dependency
- ⚠️ List queries require scatter-gather pattern
- ⚠️ More complex pagination logic
- ⚠️ One-time migration required

**Alternatives Considered**:
- Geographic partitioning: Rejected due to uneven distribution
- Date-based partitioning: Rejected due to hot partitions on recent data
- Composite strategy: Deferred to future enhancement

**Status**: Implemented (2025-01)