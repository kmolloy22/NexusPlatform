namespace Nexus.CustomerOrder.Domain.Features.Accounts;

public sealed class Account
{
    public Guid Id { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string? Email { get; }
    public string? Phone { get; set; }
    public Address Address { get; }

    public Account(Guid id, string firstName, string lastName, string? email, string? phone, Address address)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("FirstName and LastName are required");

        Id = id;
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        Address = address ?? throw new ArgumentNullException(nameof(address));
    }
}