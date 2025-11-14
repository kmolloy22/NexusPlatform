namespace Nexus.CustomerOrder.Application.Features.Accounts.Models;
public record GetAccountDto(
    string id,
    string FirstName,
    string LastName,
    string? Email,
    string Phone,

    AddressDto Address);