using FluentAssertions;
using Nexus.CustomerOrder.Application.Features.Accounts.Models;
using Nexus.CustomerOrder.Application.Features.Accounts.Ports;
using Nexus.CustomerOrder.Domain.Features.Accounts;
using Nexus.Shared.Core.Tests;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Nexus.CustomerOrder.Api.Tests.Units.Features.V1.Accounts;

public class AccountCreateTests : IClassFixture<AccountsFixture>
{
    private readonly AccountsFixture _fixture;
    private IAccountRepository AccountRepository => _fixture.AccountRepository;
    private HttpClient HttpClient => _fixture.HttpClient;
    public AccountCreateTests(AccountsFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetSubstitutes();
    }

    [Fact]
    public async Task Create_WhenValidAccountData_ThenAccountIsCreated()
    {
        // Arrange
        var dto = CreateValidAccountDto();
        AccountRepository.AddAsync(Arg.Any<Account>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var response = await HttpClient.PostAsJsonAsync(AccountsFixture.CreateUrl(), dto);

        // Assert :: Created response
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/api/accounts/");
    }

    [Fact]
    public async Task Create_WhenValidAccountData_ThenRepositoryIsCalledOnce()
    {
        // Arrange
        var dto = CreateValidAccountDto();
        AccountRepository.AddAsync(Arg.Any<Account>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await HttpClient.PostAsJsonAsync(AccountsFixture.CreateUrl(), dto);

        // Assert
        await AccountRepository.Received(1)
            .AddAsync(Arg.Any<Account>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_WhenValidAccountData_ThenResponseContainsAccountId()
    {
        // Arrange
        var dto = CreateValidAccountDto();
        AccountRepository.AddAsync(Arg.Any<Account>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var response = await HttpClient.PostAsJsonAsync(AccountsFixture.CreateUrl(), dto);

        // Assert
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseDto = JsonSerializer.Deserialize<CreateAccountResponseDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        responseDto.Should().NotBeNull();
        responseDto!.Id.Should().NotBeNullOrEmpty();
        Guid.TryParse(responseDto.Id.Replace("-", ""), out _).Should().BeTrue();
    }

    [Fact]
    public async Task Create_WhenValidAccountData_ThenLocationHeaderIsCorrect()
    {
        // Arrange
        var dto = CreateValidAccountDto();
        AccountRepository.AddAsync(Arg.Any<Account>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var response = await HttpClient.PostAsJsonAsync(AccountsFixture.CreateUrl(), dto);

        // Assert
        response.Headers.Location.Should().NotBeNull();
        var location = response.Headers.Location!.ToString();
        location.Should().MatchRegex(@"^/api/accounts/[a-f0-9]{32}$");
    }

    [Fact]
    public async Task Create_WhenFirstNameIsEmpty_ThenBadRequestIsReturned()
    {
        // Arrange
        var dto = CreateValidAccountDto() with { FirstName = "" };

        // Act
        var response = await HttpClient.PostAsJsonAsync(AccountsFixture.CreateUrl(), dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WhenLastNameIsEmpty_ThenBadRequestIsReturned()
    {
        // Arrange
        var dto = CreateValidAccountDto() with { LastName = "" };

        // Act
        var response = await HttpClient.PostAsJsonAsync(AccountsFixture.CreateUrl(), dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WhenPhoneIsEmpty_ThenBadRequestIsReturned()
    {
        // Arrange
        var dto = CreateValidAccountDto() with { Phone = "" };

        // Act
        var response = await HttpClient.PostAsJsonAsync(AccountsFixture.CreateUrl(), dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WhenAddressIsNull_ThenBadRequestIsReturned()
    {
        // Arrange
        var dto = CreateValidAccountDto() with { Address = null! };

        // Act
        var response = await HttpClient.PostAsJsonAsync(AccountsFixture.CreateUrl(), dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("", "Main St", "City", "12345", "US")] // Empty Street1
    [InlineData("123 Main St", "City", "", "12345", "US")] // Empty City (different field)
    [InlineData("123 Main St", "City", "City", "", "US")] // Empty PostalCode
    [InlineData("123 Main St", "City", "City", "12345", "")] // Empty Country
    public async Task Create_WhenRequiredAddressFieldsAreEmpty_ThenBadRequestIsReturned(
        string street1, string street2, string city, string postalCode, string country)
    {
        // Arrange
        var addressDto = new AddressDto(street1, street2, city, null, postalCode, country);
        var dto = CreateValidAccountDto() with { Address = addressDto };

        // Act
        var response = await HttpClient.PostAsJsonAsync(AccountsFixture.CreateUrl(), dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WhenEmailIsNull_ThenAccountIsCreated()
    {
        // Arrange
        var dto = CreateValidAccountDto() with { Email = null };
        AccountRepository.AddAsync(Arg.Any<Account>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var response = await HttpClient.PostAsJsonAsync(AccountsFixture.CreateUrl(), dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_WhenAddressStreet2IsNull_ThenAccountIsCreated()
    {
        // Arrange
        var addressDto = CreateValidAddressDto() with { Street2 = null };
        var dto = CreateValidAccountDto() with { Address = addressDto };
        AccountRepository.AddAsync(Arg.Any<Account>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var response = await HttpClient.PostAsJsonAsync(AccountsFixture.CreateUrl(), dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_WhenAddressStateIsNull_ThenAccountIsCreated()
    {
        // Arrange
        var addressDto = CreateValidAddressDto() with { State = null };
        var dto = CreateValidAccountDto() with { Address = addressDto };
        AccountRepository.AddAsync(Arg.Any<Account>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var response = await HttpClient.PostAsJsonAsync(AccountsFixture.CreateUrl(), dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_WhenRepositoryThrowsException_ThenInternalServerErrorIsReturned()
    {
        // Arrange
        var dto = CreateValidAccountDto();
        AccountRepository.AddAsync(Arg.Any<Account>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var response = await HttpClient.PostAsJsonAsync(AccountsFixture.CreateUrl(), dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Create_WhenRequestBodyIsInvalid_ThenBadRequestIsReturned()
    {
        // Arrange
        var invalidJson = "{ invalid json }";
        var content = new StringContent(invalidJson, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync(AccountsFixture.CreateUrl(), content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WhenContentTypeIsNotJson_ThenUnsupportedMediaTypeIsReturned()
    {
        // Arrange
        var content = new StringContent("some content", System.Text.Encoding.UTF8, "text/plain");

        // Act
        var response = await HttpClient.PostAsync(AccountsFixture.CreateUrl(), content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    public async Task Create_Response_IsJson()
    {
        // Arrange
        var dto = CreateValidAccountDto();
        AccountRepository.AddAsync(Arg.Any<Account>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var response = await HttpClient.PostAsJsonAsync(AccountsFixture.CreateUrl(), dto);

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
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

