using FluentAssertions;
using Nexus.CustomerOrder.Application.Features.Accounts.Models;
using Nexus.CustomerOrder.Application.Features.Accounts.Ports;
using Nexus.Shared.Core.Tests;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace Nexus.CustomerOrder.Api.Tests.Units.Features.V1.Accounts;

public class AccountUpdateTests : IClassFixture<AccountsFixture>
{
    private readonly AccountsFixture _fixture;
    private IAccountRepository AccountRepository => _fixture.AccountRepository;
    private HttpClient HttpClient => _fixture.HttpClient;

    public AccountUpdateTests(AccountsFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetSubstitutes();
    }

    [Fact]
    public async Task Update_WhenAccountExistsAndValidData_ThenNoContentReturned()
    {
        // Arrange
        var accountId = RandomValue.String;
        var dto = CreateValidAccountDto();

        AccountRepository.UpdateAsync(
                accountId,
                dto.FirstName,
                dto.LastName,
                dto.Email,
                dto.Phone,
                Arg.Any<Domain.Features.Accounts.Address>(),
                Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await HttpClient.PutAsJsonAsync(AccountsFixture.GetUrl(accountId), dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_WhenAccountExistsAndValidData_ThenRepositoryIsCalledOnce()
    {
        // Arrange
        var accountId = RandomValue.String;
        var dto = CreateValidAccountDto();

        AccountRepository.UpdateAsync(
                accountId,
                dto.FirstName,
                dto.LastName,
                dto.Email,
                dto.Phone,
                Arg.Any<Domain.Features.Accounts.Address>(),
                Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        await HttpClient.PutAsJsonAsync(AccountsFixture.GetUrl(accountId), dto);

        // Assert
        await AccountRepository.Received(1)
            .UpdateAsync(
                accountId,
                dto.FirstName,
                dto.LastName,
                dto.Email,
                dto.Phone,
                Arg.Any<Domain.Features.Accounts.Address>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_WhenAccountDoesNotExist_ThenNotFoundReturned()
    {
        // Arrange
        var accountId = RandomValue.String;
        var dto = CreateValidAccountDto();

        AccountRepository.UpdateAsync(
                accountId,
                dto.FirstName,
                dto.LastName,
                dto.Email,
                dto.Phone,
                Arg.Any<Domain.Features.Accounts.Address>(),
                Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var response = await HttpClient.PutAsJsonAsync(AccountsFixture.GetUrl(accountId), dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WhenAccountDoesNotExist_ThenRepositoryIsCalledOnce()
    {
        // Arrange
        var accountId = RandomValue.String;
        var dto = CreateValidAccountDto();

        AccountRepository.UpdateAsync(
                accountId,
                dto.FirstName,
                dto.LastName,
                dto.Email,
                dto.Phone,
                Arg.Any<Domain.Features.Accounts.Address>(),
                Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await HttpClient.PutAsJsonAsync(AccountsFixture.GetUrl(accountId), dto);

        // Assert
        await AccountRepository.Received(1)
            .UpdateAsync(
                accountId,
                dto.FirstName,
                dto.LastName,
                dto.Email,
                dto.Phone,
                Arg.Any<Domain.Features.Accounts.Address>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_WhenRepositoryThrowsException_ThenInternalServerErrorReturned()
    {
        // Arrange
        var accountId = RandomValue.String;
        var dto = CreateValidAccountDto();

        AccountRepository.UpdateAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Domain.Features.Accounts.Address>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var response = await HttpClient.PutAsJsonAsync(AccountsFixture.GetUrl(accountId), dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Update_WhenFirstNameIsEmpty_ThenBadRequestReturned()
    {
        // Arrange
        var accountId = RandomValue.String;
        var dto = CreateValidAccountDto() with { FirstName = "" };

        // Act
        var response = await HttpClient.PutAsJsonAsync(AccountsFixture.GetUrl(accountId), dto);

        // Assert
        // Now that validation filter is added, empty FirstName returns BadRequest
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WhenLastNameIsEmpty_ThenBadRequestReturned()
    {
        // Arrange
        var accountId = RandomValue.String;
        var dto = CreateValidAccountDto() with { LastName = "" };

        // Act
        var response = await HttpClient.PutAsJsonAsync(AccountsFixture.GetUrl(accountId), dto);

        // Assert
        // Now that validation filter is added, empty LastName returns BadRequest
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WhenPhoneIsEmpty_ThenBadRequestReturned()
    {
        // Arrange
        var accountId = RandomValue.String;
        var dto = CreateValidAccountDto() with { Phone = "" };

        // Act
        var response = await HttpClient.PutAsJsonAsync(AccountsFixture.GetUrl(accountId), dto);

        // Assert
        // Now that validation filter is added, empty Phone returns BadRequest
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WhenAccountIdIsInvalid_ThenAccountNotFoundReturned()
    {
        // Arrange
        var accountId = "invalid_account_id";
        var dto = CreateValidAccountDto();
        
        // Repository returns false for invalid account ID (not found)
        AccountRepository.UpdateAsync(
                accountId,
                dto.FirstName,
                dto.LastName,
                dto.Email,
                dto.Phone,
                Arg.Any<Domain.Features.Accounts.Address>(),
                Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var response = await HttpClient.PutAsJsonAsync(AccountsFixture.GetUrl(accountId), dto);

        // Assert
        // Invalid account ID results in NotFound (repository returns false)
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WhenAccountExistsWithDifferentData_ThenUpdatedDataIsPassedToRepository()
    {
        // Arrange
        var accountId = RandomValue.String;
        var originalDto = CreateValidAccountDto();
        var updatedDto = originalDto with
        {
            FirstName = "UpdatedFirstName",
            LastName = "UpdatedLastName",
            Email = "updated@example.com",
            Phone = "+9876543210",
            Address = originalDto.Address with
            {
                Street1 = "456 Updated St",
                City = "Updated City",
                PostalCode = "54321"
            }
        };

        AccountRepository.UpdateAsync(
                accountId,
                updatedDto.FirstName,
                updatedDto.LastName,
                updatedDto.Email,
                updatedDto.Phone,
                Arg.Any<Domain.Features.Accounts.Address>(),
                Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await HttpClient.PutAsJsonAsync(AccountsFixture.GetUrl(accountId), updatedDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await AccountRepository.Received(1)
            .UpdateAsync(
                accountId,
                updatedDto.FirstName,
                updatedDto.LastName,
                updatedDto.Email,
                updatedDto.Phone,
                Arg.Any<Domain.Features.Accounts.Address>(),
                Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("", "Main St", "City", "12345", "US")] // Empty Street1
    [InlineData("123 Main St", "City", "", "12345", "US")] // Empty City
    [InlineData("123 Main St", "City", "City", "", "US")] // Empty PostalCode
    [InlineData("123 Main St", "City", "City", "12345", "")] // Empty Country
    public async Task Update_WhenRequiredAddressFieldsAreEmpty_ThenBadRequestReturned(
        string street1, string street2, string city, string postalCode, string country)
    {
        // Arrange
        var accountId = RandomValue.String;
        var addressDto = new AddressDto(street1, street2, city, null, postalCode, country);
        var dto = CreateValidAccountDto() with { Address = addressDto };

        // Act
        var response = await HttpClient.PutAsJsonAsync(AccountsFixture.GetUrl(accountId), dto);

        // Assert
        // Now that validation filter is added, empty required address fields return BadRequest
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WhenAddressIsNull_ThenBadRequestReturned()
    {
        // Arrange
        var accountId = RandomValue.String;
        var dto = CreateValidAccountDto() with { Address = null! };

        // Act
        var response = await HttpClient.PutAsJsonAsync(AccountsFixture.GetUrl(accountId), dto);

        // Assert
        // Now that validation filter is added, null address returns BadRequest
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static CreateAccountDto CreateValidAccountDto()
    {
        return new CreateAccountDto(
            FirstName: RandomValue.String,
            LastName: RandomValue.String,
            Phone: "+1234567890",
            Email: $"{RandomValue.String}@example.com",
            Address: CreateValidAddressDto()
        );
    }

    private static AddressDto CreateValidAddressDto()
    {
        return new AddressDto(
            Street1: $"{RandomValue.Int} Main Street",
            Street2: "Apt 123",
            City: "Test City",
            State: "TS",
            PostalCode: "12345",
            Country: "US"
        );
    }
}