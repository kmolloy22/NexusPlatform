# Copilot Instructions for Nexus.CustomerOrder (.NET 9, C# 13)

## General Guidelines
- Target .NET 9 and C# 13. Keep `nullable` enabled and use async/await by default.
- Include XML docs on public types/members. Prefer small, focused methods.
- Always accept `CancellationToken` in async public APIs.

## Architecture & Layering
- Layers: `Api` ➜ `Application` ➜ `Infrastructure` (`Core` shared).
- Do NOT reference API DTOs from `Application` or `Infrastructure`.
- In `Application`, expose read models (e.g., `AccountReadModel`) and map storage entities to them.

## Dependency Injection
- Register feature via `services.AddAccountsInfrastructure()` in `Api`.
- Do not register raw `Azure.Data.Tables.TableClient` unless explicitly needed.
- Use `AddTableClient<AccountsTableStorageConfiguration, AccountTableEntity>()` for typed table access.
- Connection string: set `"Storage:ConnectionString"` in `Program.cs` by copying from `"ConnectionStrings:tables"`.

## Configuration
- In `Program.cs`:
  - Read `builder.Configuration.GetConnectionString("tables")`.
  - Set `builder.Configuration["Storage:ConnectionString"]` so table clients can resolve it.
- Prefer environment variables or `appsettings` for secrets/config.

## Azure Table Storage Usage
- Use `ITableClient<TConfig, TEntity>` for data access.
- Query accounts only within the default partition: `e.PartitionKey == AccountTableEntity.DefaultPartitionKey`.
- Pagination:
  - Repository should return a single page: `Task<Page<AccountTableEntity>?> QueryAsync(int pageSize, string? continuationToken, CancellationToken)`.
  - Use `AsyncPageable.AsPages(continuationToken, pageSize)` and return the first page or `null`.
- Concurrency: use `ETag` when updating/deleting entities.

## CQRS with MediatR
- Requests are `record`s; handlers implement `IRequestHandler<TRequest, TResponse>`.
- Handlers in `Application` should:
  - Call repositories.
  - Map `AccountTableEntity` to `AccountReadModel`.
  - Return `PagedResult<AccountReadModel>` with `ContinuationToken`.

## API Endpoints
- Minimal APIs should delegate to MediatR handlers.
- Use RESTful routes and plural nouns (e.g., `/accounts`).
- For listing:
  - Accept `take` (page size) and `continuationToken`.
  - Return items and the next `continuationToken`.

## Coding Conventions
- PascalCase: classes, methods, properties; camelCase: locals and parameters.
- Validate inputs early; return 400 for invalid API inputs.
- Keep async flows cancellable: `.WithCancellation(ct)` for async enumeration.

## Testing
- Prefer DI-friendly code. Mock repositories/clients.
- Keep unit tests in dedicated test projects; avoid external dependencies unless using emulators.

## Repo & .github
- `.github` folder lives at repo root; no need to include it in any `.csproj`.
- Commit workflow/config files under `.github` as-is.