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
) : IRequest<Guid>;

public class CreateAccountHandler(IAccountRepository repository) : IRequestHandler<CreateAccountCommand, Guid>
{
    public async Task<Guid> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = new Account(
            Guid.NewGuid(),
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.Address);

        await repository.AddAsync(account, cancellationToken);

        return account.Id;
    }
}