using MediatR;
using Nexus.CustomerOrder.Application.Features.Accounts.Ports;
using Nexus.CustomerOrder.Domain.Features.Accounts;

namespace Nexus.CustomerOrder.Application.Features.Accounts;

public record CreateAccountCommand(
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    Address Address
) : IRequest<CreateAccountResult>;

public sealed record CreateAccountResult(Guid Id, DateTimeOffset CreatedAt);

public class CreateAccountHandler(IAccountRepository repository) : IRequestHandler<CreateAccountCommand, CreateAccountResult>
{
    public async Task<CreateAccountResult> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var createdAt = DateTimeOffset.UtcNow;
        
        var account = new Account(
            Guid.NewGuid(),
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.Address);

        await repository.AddAsync(account, cancellationToken);

        return new CreateAccountResult(account.Id, createdAt);
    }
}