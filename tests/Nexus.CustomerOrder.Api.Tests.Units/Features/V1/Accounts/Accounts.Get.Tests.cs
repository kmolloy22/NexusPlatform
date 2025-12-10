using FluentAssertions;
using Azure;
using Nexus.CustomerOrder.Application.Features.Accounts.Infrastructure.StorageAccount;
using Nexus.CustomerOrder.Application.Features.Accounts.Models;
using Nexus.CustomerOrder.Application.Features.Accounts.Ports;
using Nexus.CustomerOrder.Application.Shared.Results;
using Nexus.Shared.Core.Tests;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Nexus.CustomerOrder.Api.Tests.Units.Features.V1.Accounts;

public class AccountsGetTests : IClassFixture<AccountsFixture>
{
    private readonly AccountsFixture _fixture;
    private IAccountRepository AccountRepository => _fixture.AccountRepository;
    private HttpClient HttpClient => _fixture.HttpClient;

    public AccountsGetTests(AccountsFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetSubstitutes();
    }

    [Fact]
    public async Task GetAccounts_WhenNoQueryParams_ThenUsesDefaultPageSize()
    {
        // Arrange
        var accounts = CreateValidAccountsList(3);
        
        AccountRepository.QueryAsync(50, null, Arg.Any<CancellationToken>())
            .Returns(CreatePageFromAccounts(accounts));

        // Act
        var response = await HttpClient.GetAsync(AccountsFixture.GetAccountsUrl());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await AccountRepository.Received(1)
            .QueryAsync(50, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAccounts_WhenValidAccounts_ThenReturnsOkWithAccounts()
    {
        // Arrange
        var accounts = CreateValidAccountsList(2);
        
        AccountRepository.QueryAsync(50, null, Arg.Any<CancellationToken>())
            .Returns(CreatePageFromAccounts(accounts, "next-token-123"));

        // Act
        var response = await HttpClient.GetAsync(AccountsFixture.GetAccountsUrl());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetAccountDto>>(responseContent, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.ContinuationToken.Should().Be("next-token-123");
    }

    [Fact]
    public async Task GetAccounts_WhenEmptyResult_ThenReturnsOkWithEmptyList()
    {
        // Arrange
        var emptyAccounts = new List<GetAccountDto>();
        
        AccountRepository.QueryAsync(50, null, Arg.Any<CancellationToken>())
            .Returns(CreatePageFromAccounts(emptyAccounts));

        // Act
        var response = await HttpClient.GetAsync(AccountsFixture.GetAccountsUrl());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetAccountDto>>(responseContent, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.ContinuationToken.Should().BeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(100)]
    public async Task GetAccounts_WhenValidTakeParameter_ThenUsesSpecifiedPageSize(int take)
    {
        // Arrange
        var accounts = CreateValidAccountsList(Math.Min(take, 5)); // Create up to 5 accounts for testing
        
        AccountRepository.QueryAsync(take, null, Arg.Any<CancellationToken>())
            .Returns(CreatePageFromAccounts(accounts));

        // Act
        var response = await HttpClient.GetAsync(AccountsFixture.GetAccountsUrl($"?take={take}"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await AccountRepository.Received(1)
            .QueryAsync(take, null, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0)] // Zero or negative should return BadRequest with validation
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task GetAccounts_WhenInvalidTakeParameter_ThenReturnsBadRequest(int take)
    {
        // Act
        var response = await HttpClient.GetAsync(AccountsFixture.GetAccountsUrl($"?take={take}"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        // Repository should not be called for invalid parameters
        await AccountRepository.DidNotReceive()
            .QueryAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAccounts_WhenContinuationTokenProvided_ThenPassesToRepository()
    {
        // Arrange
        var continuationToken = "abc123token";
        var accounts = CreateValidAccountsList(2);
        
        AccountRepository.QueryAsync(50, continuationToken, Arg.Any<CancellationToken>())
            .Returns(CreatePageFromAccounts(accounts));

        // Act
        var response = await HttpClient.GetAsync(AccountsFixture.GetAccountsUrl($"?continuationToken={continuationToken}"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await AccountRepository.Received(1)
            .QueryAsync(50, continuationToken, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAccounts_WhenBothTakeAndContinuationTokenProvided_ThenUsesCorrectParameters()
    {
        // Arrange
        var take = 25;
        var continuationToken = "xyz789token";
        var accounts = CreateValidAccountsList(3);
        
        AccountRepository.QueryAsync(take, continuationToken, Arg.Any<CancellationToken>())
            .Returns(CreatePageFromAccounts(accounts));

        // Act
        var response = await HttpClient.GetAsync(AccountsFixture.GetAccountsUrl($"?take={take}&continuationToken={continuationToken}"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await AccountRepository.Received(1)
            .QueryAsync(take, continuationToken, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAccounts_WhenRepositoryThrowsException_ThenInternalServerErrorReturned()
    {
        // Arrange
        AccountRepository.QueryAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var response = await HttpClient.GetAsync(AccountsFixture.GetAccountsUrl());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetAccounts_WhenRepositoryReturnsEmptyPage_ThenReturnsOkWithEmptyResult()
    {
        // Arrange
        var emptyPage = Page<AccountTableEntity>.FromValues(new List<AccountTableEntity>(), null, Substitute.For<Response>());
        
        AccountRepository.QueryAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(emptyPage);

        // Act
        var response = await HttpClient.GetAsync(AccountsFixture.GetAccountsUrl());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetAccountDto>>(responseContent, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.ContinuationToken.Should().BeNull();
    }

    [Fact]
    public async Task GetAccounts_Response_IsJson()
    {
        // Arrange
        var accounts = CreateValidAccountsList(1);
        
        AccountRepository.QueryAsync(50, null, Arg.Any<CancellationToken>())
            .Returns(CreatePageFromAccounts(accounts));

        // Act
        var response = await HttpClient.GetAsync(AccountsFixture.GetAccountsUrl());

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task GetAccounts_WhenRepositoryReturnsAccountsWithCompleteData_ThenMapsAllFields()
    {
        // Arrange
        var accountEntities = new List<AccountTableEntity>
        {
            new AccountTableEntity
            {
                RowKey = Guid.NewGuid().ToString("N"),
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "+1234567890",
                Address_Street1 = "123 Main St",
                Address_Street2 = "Apt 456",
                Address_City = "Anytown",
                Address_State = "ST",
                Address_PostalCode = "12345",
                Address_Country = "US"
            }
        };
        
        var page = Page<AccountTableEntity>.FromValues(accountEntities, null, Substitute.For<Response>());
        
        AccountRepository.QueryAsync(50, null, Arg.Any<CancellationToken>())
            .Returns(page);

        // Act
        var response = await HttpClient.GetAsync(AccountsFixture.GetAccountsUrl());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetAccountDto>>(responseContent, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        
        var account = result.Items.First();
        account.id.Should().Be(accountEntities[0].RowKey);
        account.FirstName.Should().Be("John");
        account.LastName.Should().Be("Doe");
        account.Email.Should().Be("john.doe@example.com");
        account.Phone.Should().Be("+1234567890");
        account.Address.Street1.Should().Be("123 Main St");
        account.Address.Street2.Should().Be("Apt 456");
        account.Address.City.Should().Be("Anytown");
        account.Address.State.Should().Be("ST");
        account.Address.PostalCode.Should().Be("12345");
        account.Address.Country.Should().Be("US");
    }

    [Fact]
    public async Task GetAccounts_WhenRepositoryReturnsAccountsWithNullOptionalFields_ThenHandlesNullValues()
    {
        // Arrange
        var accountEntities = new List<AccountTableEntity>
        {
            new AccountTableEntity
            {
                RowKey = Guid.NewGuid().ToString("N"),
                FirstName = "Jane",
                LastName = "Smith",
                Email = null, // Optional field
                PhoneNumber = null, // Optional field
                Address_Street1 = "789 Oak St",
                Address_Street2 = null, // Optional field
                Address_City = "Other City",
                Address_State = null, // Optional field
                Address_PostalCode = "67890",
                Address_Country = "CA"
            }
        };
        
        var page = Page<AccountTableEntity>.FromValues(accountEntities, null, Substitute.For<Response>());
        
        AccountRepository.QueryAsync(50, null, Arg.Any<CancellationToken>())
            .Returns(page);

        // Act
        var response = await HttpClient.GetAsync(AccountsFixture.GetAccountsUrl());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetAccountDto>>(responseContent, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        
        var account = result.Items.First();
        account.Email.Should().BeNull();
        account.Phone.Should().Be(""); // Handler returns empty string for null phone
        account.Address.Street2.Should().BeNull();
        account.Address.State.Should().BeNull();
    }

    [Theory]
    [InlineData("token with spaces")]
    [InlineData("token%20with%20encoding")]
    [InlineData("")]
    [InlineData("very-long-continuation-token-that-might-be-generated-by-azure-storage")]
    public async Task GetAccounts_WhenVariousContinuationTokenFormats_ThenHandlesCorrectly(string continuationToken)
    {
        // Arrange
        var accounts = CreateValidAccountsList(1);
        
        AccountRepository.QueryAsync(50, continuationToken, Arg.Any<CancellationToken>())
            .Returns(CreatePageFromAccounts(accounts));

        // Act
        var response = await HttpClient.GetAsync(AccountsFixture.GetAccountsUrl($"?continuationToken={Uri.EscapeDataString(continuationToken)}"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await AccountRepository.Received(1)
            .QueryAsync(50, continuationToken, Arg.Any<CancellationToken>());
    }

    private static List<GetAccountDto> CreateValidAccountsList(int count)
    {
        var accounts = new List<GetAccountDto>();
        
        for (int i = 0; i < count; i++)
        {
            accounts.Add(new GetAccountDto(
                id: Guid.NewGuid().ToString("N"),
                FirstName: $"FirstName{i}",
                LastName: $"LastName{i}",
                Email: $"user{i}@example.com",
                Phone: $"+123456789{i}",
                Address: new AddressDto(
                    Street1: $"{100 + i} Main St",
                    Street2: i % 2 == 0 ? $"Apt {i}" : null,
                    City: $"City{i}",
                    State: i % 3 == 0 ? $"S{i}" : null,
                    PostalCode: $"{10000 + i}",
                    Country: "US"
                )
            ));
        }
        
        return accounts;
    }

    private static Page<AccountTableEntity> CreatePageFromAccounts(IReadOnlyCollection<GetAccountDto> accounts, string? continuationToken = null)
    {
        var entities = accounts.Select(dto => new AccountTableEntity
        {
            PartitionKey = AccountTableEntity.DefaultPartitionKey,
            RowKey = dto.id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = string.IsNullOrEmpty(dto.Phone) ? null : dto.Phone,
            Address_Street1 = dto.Address.Street1,
            Address_Street2 = dto.Address.Street2,
            Address_City = dto.Address.City,
            Address_State = dto.Address.State,
            Address_PostalCode = dto.Address.PostalCode,
            Address_Country = dto.Address.Country
        }).ToList();

        return Page<AccountTableEntity>.FromValues(entities, continuationToken, Substitute.For<Response>());
    }
}