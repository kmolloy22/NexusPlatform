using Nexus.Shared.Kernel.Extensions;

namespace Nexus.CustomerOrder.Domain.Features.Accounts;

public class Account
{
    public Guid Id { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string? Email { get; }
    public string? Phone { get; set; }
    public bool? IsActive { get; set; }
    public Address Address { get; }

    public Account(Guid id, string firstName, string lastName, string? email, string? phone, bool? isActive, Address address)
    {
        if (firstName.IsMissing() || lastName.IsMissing())
            throw new ArgumentException("FirstName and LastName are required");

        Id = id;
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.IsMissing() ? null : email.Trim();
        Phone = phone.IsMissing() ? null : phone.Trim();
        IsActive = isActive;
        Address = address ?? throw new ArgumentNullException(nameof(address));
    }
}