using MediatR;
using Nexus.CustomerOrder.Application.Features.Accounts.Models;
using Nexus.CustomerOrder.Application.Features.Accounts.Ports;

namespace Nexus.CustomerOrder.Application.Features.Accounts;

public record GetAccountCommand(string AccountId) : IRequest<GetAccountDto?>;

internal class GetAccountHandler(IAccountRepository repository) : IRequestHandler<GetAccountCommand, GetAccountDto?>
{
	public async Task<GetAccountDto?> Handle(GetAccountCommand cmd, CancellationToken cancellationToken)
	{
		var entity = await repository.GetByIdAsync(cmd.AccountId, cancellationToken);
		if (entity is null)
			return null;

		return new GetAccountDto(
			entity.RowKey,
			entity.FirstName,
			entity.LastName,
			entity.Email,
			entity.PhoneNumber ?? string.Empty,
			new AddressDto(
				entity.Address_Street1,
				entity.Address_Street2,
				entity.Address_City,
				entity.Address_State,
				entity.Address_PostalCode,
				entity.Address_Country
			)
		);
	}
}
