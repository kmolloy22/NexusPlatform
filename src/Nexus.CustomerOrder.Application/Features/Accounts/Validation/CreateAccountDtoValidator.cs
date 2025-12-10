using FluentValidation;
using Nexus.CustomerOrder.Application.Features.Accounts.Models;
using Nexus.Shared.Kernel.Extensions;

namespace Nexus.CustomerOrder.Application.Features.Accounts.Validation;

/// <summary>
/// Validator for CreateAccountDto following Nexus Platform validation patterns
/// </summary>
public sealed class CreateAccountDtoValidator : AbstractValidator<CreateAccountDto>
{
    public CreateAccountDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .Must(x => x.IsPresent())
            .WithMessage("First name is required")
            .WithErrorCode("ACCOUNT_FIRST_NAME_REQUIRED");

        RuleFor(x => x.LastName)
            .Must(x => x.IsPresent())
            .WithMessage("Last name is required")
            .WithErrorCode("ACCOUNT_LAST_NAME_REQUIRED");

        RuleFor(x => x.Phone)
            .Must(x => x.IsPresent())
            .WithMessage("Phone number is required")
            .WithErrorCode("ACCOUNT_PHONE_REQUIRED")
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Phone number must be in valid international format")
            .WithErrorCode("ACCOUNT_PHONE_INVALID");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => x.Email != null && x.Email.IsPresent())
            .WithMessage("Email must be in valid format")
            .WithErrorCode("ACCOUNT_EMAIL_INVALID");

        RuleFor(x => x.Address)
            .NotNull()
            .WithMessage("Address is required")
            .WithErrorCode("ACCOUNT_ADDRESS_REQUIRED")
            .SetValidator(new AddressDtoValidator());
    }
}

/// <summary>
/// Validator for AddressDto following Nexus Platform validation patterns
/// </summary>
public sealed class AddressDtoValidator : AbstractValidator<AddressDto>
{
    public AddressDtoValidator()
    {
        RuleFor(x => x.Street1)
            .Must(x => x.IsPresent())
            .WithMessage("Street address is required")
            .WithErrorCode("ADDRESS_STREET1_REQUIRED");

        RuleFor(x => x.City)
            .Must(x => x.IsPresent())
            .WithMessage("City is required")
            .WithErrorCode("ADDRESS_CITY_REQUIRED");

        RuleFor(x => x.PostalCode)
            .Must(x => x.IsPresent())
            .WithMessage("Postal code is required")
            .WithErrorCode("ADDRESS_POSTAL_CODE_REQUIRED");

        RuleFor(x => x.Country)
            .Must(x => x.IsPresent())
            .WithMessage("Country is required")
            .WithErrorCode("ADDRESS_COUNTRY_REQUIRED")
            .Length(2)
            .WithMessage("Country must be a 2-letter ISO country code")
            .WithErrorCode("ADDRESS_COUNTRY_INVALID");
    }
}