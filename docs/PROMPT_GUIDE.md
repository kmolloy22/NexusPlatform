# Prompt Guide for Nexus Platform

This guide provides standardized prompts and best practices for AI-assisted development within the Nexus Platform ecosystem.

## Table of Contents
- [General Principles](#general-principles)
- [Code Generation Prompts](#code-generation-prompts)
- [Testing Prompts](#testing-prompts)
- [Documentation Prompts](#documentation-prompts)
- [Refactoring Prompts](#refactoring-prompts)
- [Architecture Prompts](#architecture-prompts)
- [Azure & Cloud Prompts](#azure--cloud-prompts)

## General Principles

### Context-First Approach
Always provide relevant context about the Nexus Platform when requesting AI assistance:

```
Context: Working on Nexus Platform - a .NET 9 microservices application using Clean Architecture, Azure Container Apps, and Azure Table Storage.

Current project structure:
- Domain: Nexus.CustomerOrder.Domain
- Application: Nexus.CustomerOrder.Application (MediatR/CQRS)
- API: Nexus.CustomerOrder.Api (Minimal APIs)
- Infrastructure: Nexus.Infrastructure.StorageAccount

Request: [Your specific request here]
```

### Consistency Guidelines
- Follow existing naming conventions (`Nexus.*` namespace pattern)
- Maintain Clean Architecture separation
- Use established patterns (MediatR handlers, repository pattern)
- Apply project coding standards (.editorconfig rules)

## Code Generation Prompts

### New Feature Development

#### Domain Entity Creation
```
Create a new domain entity for [FEATURE_NAME] in the Nexus.CustomerOrder.Domain project.

Requirements:
- Follow existing Account.cs pattern
- Include validation in constructor
- Use immutable properties where appropriate
- Add XML documentation
- Ensure nullable reference type compliance

Similar to: src/Nexus.CustomerOrder.Domain/Features/Accounts/Account.cs
```

#### MediatR Handler Creation
```
Create a MediatR command/query handler for [OPERATION] in Nexus.CustomerOrder.Application.

Requirements:
- Follow CQRS pattern like existing handlers
- Include proper error handling
- Use repository pattern for data access
- Add cancellation token support
- Include XML documentation

Pattern: src/Nexus.CustomerOrder.Application/Features/Accounts/CreateAccountHandler.cs
```

#### Minimal API Endpoint
```
Create a Minimal API endpoint for [FEATURE] in Nexus.CustomerOrder.Api.

Requirements:
- Follow feature-based organization
- Use proper HTTP status codes
- Include OpenAPI documentation (WithName, WithSummary, WithDescription)
- Integrate with MediatR handlers
- Add proper validation

Pattern: src/Nexus.CustomerOrder.Api/Features/Accounts/AccountCreate/CreateAccountEndpoint.cs
```

### Repository Implementation
```
Create a repository implementation for [ENTITY] using Azure Table Storage.

Requirements:
- Implement I[Entity]Repository interface
- Use generic TableClient<TConfig, TEntity> pattern
- Follow AccountRepository.cs structure
- Include proper async/await patterns
- Add error handling for storage operations

Reference: src/Nexus.CustomerOrder.Application/Features/Accounts/Infrastructure/StorageAccount/AccountRepository.cs
```

### Azure Table Storage Entity
```
Create an Azure Table Storage entity for [DOMAIN_ENTITY].

Requirements:
- Implement ITableEntity interface
- Use flattened properties for complex objects
- Follow AccountTableEntity.cs naming pattern
- Include proper partition/row key strategy
- Add DefaultPartitionKey constant

Pattern: src/Nexus.CustomerOrder.Application/Features/Accounts/Infrastructure/StorageAccount/AccountTableEntity.cs
```

## Testing Prompts

### Unit Test Creation
```
Create unit tests for [CLASS_NAME] following Nexus Platform testing patterns.

Requirements:
- Use xUnit, FluentAssertions, and NSubstitute
- Follow AAA pattern (Arrange, Act, Assert)
- Include edge cases and validation scenarios
- Use meaningful test method names
- Add test data builders if needed

Reference testing patterns in: tests/Nexus.CustomerOrder.Api.Tests.Units/
```

### Integration Test Setup
```
Create integration tests for [API_ENDPOINT] using WebApplicationFactory.

Requirements:
- Follow existing ApiFactory.cs pattern
- Test full HTTP request/response cycle
- Include both success and error scenarios
- Mock external dependencies appropriately
- Verify proper status codes and response content

Pattern: tests/Nexus.CustomerOrder.Api.Tests.Units/Shared/ApiFactory.cs
```

### Storage Integration Tests
```
Create storage integration tests for [REPOSITORY] using Azure Storage Emulator.

Requirements:
- Follow ClientTestsBase.cs pattern
- Include setup and cleanup in test lifecycle
- Test CRUD operations thoroughly
- Verify proper entity mapping
- Include concurrent access scenarios

Pattern: tests/Nexus.Infrastructure.StorageAccount.Tests.Units/Tables/TableClientTests.cs
```

## Documentation Prompts

### API Documentation
```
Generate OpenAPI documentation for [ENDPOINT_GROUP] endpoints.

Requirements:
- Include comprehensive descriptions
- Document all parameters and response types
- Add example requests/responses
- Follow existing documentation patterns
- Include error response documentation

Context: Minimal APIs with WithSummary/WithDescription patterns
```

### README Section Updates
```
Update README.md section for [NEW_FEATURE].

Requirements:
- Maintain existing structure and tone
- Include setup/usage instructions
- Add relevant code examples
- Update technology stack if needed
- Follow markdown formatting standards

Current README: README.md
```

### Architecture Documentation
```
Document architectural decisions for [COMPONENT/PATTERN].

Requirements:
- Follow existing ARCHITECTURE.md structure
- Include diagrams where helpful
- Explain design rationale
- Document trade-offs and alternatives
- Reference related components

Pattern: docs/ARCHITECTURE.md
```

## Refactoring Prompts

### Clean Architecture Compliance
```
Refactor [CODE_SECTION] to better follow Clean Architecture principles.

Requirements:
- Ensure proper dependency direction (inward)
- Separate concerns appropriately
- Move infrastructure code to proper layer
- Maintain existing interfaces
- Preserve functionality

Architecture reference: docs/ARCHITECTURE.md
```

### Performance Optimization
```
Optimize [METHOD/CLASS] for better performance in Azure Container Apps environment.

Considerations:
- Async/await patterns
- Memory allocation reduction
- Database query optimization
- Caching opportunities
- Container startup time

Context: Azure Container Apps with auto-scaling
```

### Error Handling Enhancement
```
Improve error handling for [COMPONENT] following Nexus Platform standards.

Requirements:
- Use Result pattern or appropriate error handling
- Include proper logging with correlation IDs
- Handle Azure service exceptions
- Provide meaningful error messages
- Maintain security (don't leak sensitive info)

Pattern: Existing error handling in handlers
```

## Architecture Prompts

### New Service Design
```
Design architecture for new [SERVICE_NAME] service in Nexus Platform.

Requirements:
- Follow existing project structure
- Define clear boundaries and responsibilities
- Plan data storage strategy
- Consider inter-service communication
- Design for Azure Container Apps deployment

Reference: Existing CustomerOrder service structure
```

### Database Schema Design
```
Design Azure Table Storage schema for [DOMAIN_AREA].

Considerations:
- Partition key strategy for scalability
- Query patterns and performance
- Data relationship modeling
- Migration strategy from existing data
- Consistency requirements

Reference: AccountTableEntity design patterns
```

### Service Integration
```
Design integration between [SERVICE_A] and [SERVICE_B] in Nexus Platform.

Requirements:
- Define communication patterns (sync/async)
- Plan for failure scenarios
- Consider data consistency needs
- Design for independent deployment
- Include monitoring and observability

Context: Microservices architecture with Azure Container Apps
```

## Azure & Cloud Prompts

### Azure Resource Configuration
```
Configure Azure resources for [FEATURE] using .NET Aspire.

Requirements:
- Follow existing AppHost.cs patterns
- Include proper health checks
- Configure auto-scaling policies
- Set up monitoring and alerting
- Plan for different environments

Reference: src/Nexus.AppHost/AppHost.cs
```

### CI/CD Pipeline Updates
```
Update GitHub Actions workflow for [NEW_REQUIREMENT].

Requirements:
- Follow existing azure-dev.yml structure
- Maintain security best practices
- Include proper testing stages
- Configure environment-specific deployments
- Add necessary secrets/variables

Reference: .github/workflows/azure-dev.yml
```

### Observability Enhancement
```
Enhance observability for [COMPONENT] using OpenTelemetry.

Requirements:
- Add custom metrics for business operations
- Include distributed tracing spans
- Configure structured logging
- Set up dashboards and alerting
- Follow existing ServiceDefaults patterns

Reference: src/Nexus.ServiceDefaults/Extensions.cs
```

### Storage Optimization
```
Optimize Azure Table Storage usage for [SCENARIO].

Considerations:
- Query performance optimization
- Partition strategy improvements
- Data archival strategies
- Cost optimization opportunities
- Backup and disaster recovery

Context: Current TableClient implementation patterns
```

## Best Practices for AI Assistance

### Do's
✅ **Provide specific context** about the Nexus Platform architecture  
✅ **Reference existing code patterns** for consistency  
✅ **Include requirements** for error handling, testing, and documentation  
✅ **Specify technology constraints** (.NET 9, Azure Container Apps, etc.)  
✅ **Ask for explanations** of design decisions  

### Don'ts
❌ **Don't request generic solutions** without platform context  
❌ **Don't ignore existing patterns** and architectural decisions  
❌ **Don't skip testing requirements** in code generation requests  
❌ **Don't forget about deployment considerations** for Azure  
❌ **Don't overlook security implications** in implementation requests  

## Prompt Templates

### Quick Code Generation
```
Generate [TYPE] for [PURPOSE] in Nexus Platform.
Context: [RELEVANT_CONTEXT]
Requirements: [SPECIFIC_REQUIREMENTS]
Pattern: [REFERENCE_FILE]
```

### Architecture Review
```
Review [COMPONENT] architecture for Nexus Platform compliance.
Focus: [SPECIFIC_CONCERNS]
Standards: Clean Architecture, SOLID principles, Azure best practices
Reference: docs/ARCHITECTURE.md
```

### Testing Strategy
```
Design testing strategy for [FEATURE] in Nexus Platform.
Scope: [UNIT/INTEGRATION/E2E]
Requirements: xUnit, FluentAssertions, Azure Storage testing
Reference: Existing test patterns
```

## Common Pitfalls to Avoid

1. **Generic Solutions**: Always specify Nexus Platform context
2. **Breaking Patterns**: Reference existing code patterns for consistency
3. **Missing Tests**: Include testing requirements in all code generation
4. **Ignoring Azure**: Consider Azure-specific requirements and constraints
5. **Incomplete Documentation**: Request documentation updates with code changes

---

*This guide should be updated as new patterns and practices are established in the Nexus Platform.*