using MediatR;
using Nexus.CustomerOrder.Application.Features.Accounts.Models;
using Nexus.CustomerOrder.Application.Features.Accounts.Ports;
using Nexus.CustomerOrder.Application.Shared.Results;

public record GetAccountsCommand(int PageSize, string? ContinuationToken)
    : IRequest<PagedResult<GetAccountDto>>;

internal class GetAccountsHandler : IRequestHandler<GetAccountsCommand, PagedResult<GetAccountDto>>
{
    private readonly IAccountRepository _repository;

    public GetAccountsHandler(IAccountRepository repository) =>
        _repository = repository;

    public async Task<PagedResult<GetAccountDto>> Handle(GetAccountsCommand request, CancellationToken cancellationToken)
    {
        var pagedEntities = await _repository.QueryAsync(
            request.PageSize,
            request.ContinuationToken,
            cancellationToken);

        var items = pagedEntities.Items.Select(e => new GetAccountDto(
            e.RowKey,
            e.FirstName,
            e.LastName,
            e.Email,
            e.PhoneNumber ?? string.Empty,
            e.IsActive,
            e.CreatedUtc,
            e.ModifiedUtc,
            new AddressDto(
                e.Address_Street1,
                e.Address_Street2,
                e.Address_City,
                e.Address_State,
                e.Address_PostalCode,
                e.Address_Country
            )
        )).ToList();

        return new PagedResult<GetAccountDto>(items, pagedEntities.ContinuationToken);
    }
}