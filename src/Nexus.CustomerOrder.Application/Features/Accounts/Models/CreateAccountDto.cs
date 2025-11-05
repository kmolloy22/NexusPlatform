namespace Nexus.CustomerOrder.Application.Features.Accounts.Models;

public  record CreateAccountDto(
    string FirstName,
    string LastName,
    string Phone,
    string? Email,
    AddressDto Address);