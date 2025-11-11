using MediatR;
using Nexus.CustomerOrder.Application.Features.Accounts.Ports;

namespace Nexus.CustomerOrder.Application.Features.Accounts;

public record DeleteAccountCommand(string AccountId) : IRequest<bool>;

internal class DeleteAccountHandler(IAccountRepository repository) : IRequestHandler<DeleteAccountCommand, bool>
{
    public async Task<bool> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        return await repository.DeleteAsync(request.AccountId, cancellationToken);
    }
}
