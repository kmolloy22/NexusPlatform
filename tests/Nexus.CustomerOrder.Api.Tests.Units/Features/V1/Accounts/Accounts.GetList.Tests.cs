using FluentAssertions;
using Nexus.CustomerOrder.Application.Features.Accounts.Models;
using Nexus.CustomerOrder.Application.Features.Accounts.Ports;
using Nexus.CustomerOrder.Application.Shared.Results;
using Nexus.Shared.Core.Tests;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Net;
using Azure;
using Nexus.CustomerOrder.Application.Features.Accounts.Infrastructure.StorageAccount;

namespace Nexus.CustomerOrder.Api.Tests.Units.Features.V1.Accounts;

public class AccountsGetListTests : IClassFixture<AccountsFixture>
{
    private readonly AccountsFixture _fixture;
    private IAccountRepository AccountRepository => _fixture.AccountRepository;
    private HttpClient HttpClient => _fixture.HttpClient;

    public AccountsGetListTests(AccountsFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetSubstitutes();
    }

    [Fact]
    public async Task GetAccounts_WhenNoParameters_ThenReturnsDefaultPageSize()
    {
        // Arrange
        var mockPage = CreateMockPage(CreateTestAccounts(50));
        AccountRepository.QueryAsync(50, null, Arg.Any<CancellationToken>())
            .Returns(mockPage);

        // Act
        var response = await HttpClient.GetAsync("/api/accounts/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await AccountRepository.Received(1)
            .QueryAsync(50, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAccounts_WhenTakeParameterProvided_ThenUsesSpecifiedPageSize()
    {
        // Arrange
        var mockPage = CreateMockPage(CreateTestAccounts(25));
        AccountRepository.QueryAsync(25, null, Arg.Any<CancellationToken>())
            .Returns(mockPage);

        // Act
        var response = await HttpClient.GetAsync("/api/accounts/?take=25");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await AccountRepository.Received(1)
            .QueryAsync(25, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAccounts_WhenContinuationTokenProvided_ThenPassesToRepository()
    {
        // Arrange
        var continuationToken = "2!8!MDAwMDMxMDEwMDAwMDEwMDAxMDEwMQ==";
        var mockPage = CreateMockPage(CreateTestAccounts(50));
        AccountRepository.QueryAsync(50, continuationToken, Arg.Any<CancellationToken>())
            .Returns(mockPage);

        // Act
        var response = await HttpClient.GetAsync($"/api/accounts/?continuationToken={continuationToken}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await AccountRepository.Received(1)
            .QueryAsync(50, continuationToken, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAccounts_WhenBothParametersProvided_ThenUsesBoth()
    {
        // Arrange
        var continuationToken = "2!8!MDAwMDMxMDEwMDAwMDEwMDAxMDEwMQ==";
        var mockPage = CreateMockPage(CreateTestAccounts(100));
        AccountRepository.QueryAsync(100, continuationToken, Arg.Any<CancellationToken>())
            .Returns(mockPage);

        // Act
        var response = await HttpClient.GetAsync($"/api/accounts/?take=100&continuationToken={continuationToken}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await AccountRepository.Received(1)
            .QueryAsync(100, continuationToken, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task GetAccounts_WhenTakeIsZeroOrNegative_ThenReturnsBadRequest(int take)
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/accounts/?take={take}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        // Repository should not be called
        await AccountRepository.DidNotReceive()
            .QueryAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAccounts_WhenTakeExceedsMaximum_ThenReturnsBadRequest()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/accounts/?take=1001");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        // Repository should not be called
        await AccountRepository.DidNotReceive()
            .QueryAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public async Task GetAccounts_WhenContinuationTokenIsWhitespace_ThenReturnsBadRequest(string token)
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/accounts/?continuationToken={Uri.EscapeDataString(token)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        // Repository should not be called
        await AccountRepository.DidNotReceive()
            .QueryAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAccounts_WhenContinuationTokenIsEmpty_ThenPassesEmptyStringToRepository()
    {
        // Arrange
        var mockPage = CreateMockPage(CreateTestAccounts(50));
        AccountRepository.QueryAsync(50, "", Arg.Any<CancellationToken>())
            .Returns(mockPage);

        // Act
        var response = await HttpClient.GetAsync("/api/accounts/?continuationToken=");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await AccountRepository.Received(1)
            .QueryAsync(50, "", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAccounts_WhenRepositoryReturnsPagedResult_ThenReturnsCorrectFormat()
    {
        // Arrange
        var testAccounts = CreateTestAccounts(3);
        var continuationToken = "next_page_token";
        var mockPage = CreateMockPage(testAccounts, continuationToken);
        
        AccountRepository.QueryAsync(50, null, Arg.Any<CancellationToken>())
            .Returns(mockPage);

        // Act
        var response = await HttpClient.GetAsync("/api/accounts/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("continuationToken");
        content.Should().Contain(continuationToken);
    }

    [Fact]
    public async Task GetAccounts_WhenLastPage_ThenContinuationTokenIsNull()
    {
        // Arrange
        var testAccounts = CreateTestAccounts(2);
        var mockPage = CreateMockPage(testAccounts, null); // No more pages
        
        AccountRepository.QueryAsync(50, null, Arg.Any<CancellationToken>())
            .Returns(mockPage);

        // Act
        var response = await HttpClient.GetAsync("/api/accounts/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"continuationToken\":null");
    }

    [Fact]
    public async Task GetAccounts_WhenRepositoryThrowsException_ThenReturnsInternalServerError()
    {
        // Arrange
        AccountRepository.QueryAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsyncForAnyArgs(new InvalidOperationException("Database error"));

        // Act
        var response = await HttpClient.GetAsync("/api/accounts/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1000)]
    public async Task GetAccounts_WhenValidTakeValues_ThenAcceptsRequest(int take)
    {
        // Arrange
        var mockPage = CreateMockPage(CreateTestAccounts(take));
        AccountRepository.QueryAsync(take, null, Arg.Any<CancellationToken>())
            .Returns(mockPage);

        // Act
        var response = await HttpClient.GetAsync($"/api/accounts/?take={take}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await AccountRepository.Received(1)
            .QueryAsync(take, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAccounts_WhenMultipleConcurrentRequests_ThenHandlesCorrectly()
    {
        // Arrange
        var mockPage = CreateMockPage(CreateTestAccounts(10));
        AccountRepository.QueryAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(mockPage);

        // Act
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => HttpClient.GetAsync("/api/accounts/?take=10"));
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.OK);
        await AccountRepository.Received(5)
            .QueryAsync(10, null, Arg.Any<CancellationToken>());
    }

    private static Page<AccountTableEntity> CreateMockPage(
        List<AccountTableEntity> entities, 
        string? continuationToken = null)
    {
        // Create a substitute for Response since we can't easily create a real one
        var mockResponse = Substitute.For<Response>();
        return Page<AccountTableEntity>.FromValues(entities, continuationToken, mockResponse);
    }

    private static List<AccountTableEntity> CreateTestAccounts(int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => new AccountTableEntity
            {
                PartitionKey = AccountTableEntity.DefaultPartitionKey,
                RowKey = Guid.NewGuid().ToString("N"),
                FirstName = $"FirstName{i}",
                LastName = $"LastName{i}",
                Email = $"test{i}@example.com",
                PhoneNumber = $"+123456789{i:00}",
                Address_Street1 = $"{i} Test Street",
                Address_Street2 = null,
                Address_City = "Test City",
                Address_State = "TS",
                Address_PostalCode = $"1234{i:0}",
                Address_Country = "US"
            })
            .ToList();
    }
}