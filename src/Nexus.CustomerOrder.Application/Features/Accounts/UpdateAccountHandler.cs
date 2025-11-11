using MediatR;
using Nexus.CustomerOrder.Application.Features.Accounts.Ports;
using Nexus.CustomerOrder.Domain.Features.Accounts;

namespace Nexus.CustomerOrder.Application.Features.Accounts;

public record UpdateAccountCommand(
    string AccountId,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    Address Address) : IRequest<bool>;

internal class UpdateAccountHandler(IAccountRepository repository) : IRequestHandler<UpdateAccountCommand, bool>
{
    public async Task<bool> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        return await repository.UpdateAsync(
            request.AccountId,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.Address,
            cancellationToken);
    }
}
