namespace Nexus.CustomerOrder.Application.Features.Accounts.Models;

public record AddressDto(
    string Street1,
    string? Street2,
    string City,
    string? State,
    string PostalCode,
    string Country);
