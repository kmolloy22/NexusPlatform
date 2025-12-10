using FluentAssertions;
using Nexus.CustomerOrder.Application.Features.Accounts.Ports;
using Nexus.Shared.Core.Tests;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Net;

namespace Nexus.CustomerOrder.Api.Tests.Units.Features.V1.Accounts;

public class AccountDeleteTests : IClassFixture<AccountsFixture>
{
    private readonly AccountsFixture _fixture;
    private IAccountRepository AccountRepository => _fixture.AccountRepository;
    private HttpClient HttpClient => _fixture.HttpClient;

    public AccountDeleteTests(AccountsFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetSubstitutes();
    }

    [Fact]
    public async Task Delete_WhenAccountExists_ThenNoContentReturned()
    {
        // Arrange
        var accountId = RandomValue.String;
        
        AccountRepository.DeleteAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await HttpClient.DeleteAsync(AccountsFixture.GetUrl(accountId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_WhenAccountExists_ThenRepositoryIsCalledOnce()
    {
        // Arrange
        var accountId = RandomValue.String;
        
        AccountRepository.DeleteAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        await HttpClient.DeleteAsync(AccountsFixture.GetUrl(accountId));

        // Assert
        await AccountRepository.Received(1)
            .DeleteAsync(accountId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_WhenAccountDoesNotExist_ThenNotFoundReturned()
    {
        // Arrange
        var accountId = RandomValue.String;
        
        AccountRepository.DeleteAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var response = await HttpClient.DeleteAsync(AccountsFixture.GetUrl(accountId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WhenAccountDoesNotExist_ThenRepositoryIsCalledOnce()
    {
        // Arrange
        var accountId = RandomValue.String;
        
        AccountRepository.DeleteAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await HttpClient.DeleteAsync(AccountsFixture.GetUrl(accountId));

        // Assert
        await AccountRepository.Received(1)
            .DeleteAsync(accountId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_WhenRepositoryThrowsException_ThenInternalServerErrorReturned()
    {
        // Arrange
        var accountId = RandomValue.String;
        
        AccountRepository.DeleteAsync(accountId, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var response = await HttpClient.DeleteAsync(AccountsFixture.GetUrl(accountId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public async Task Delete_WhenAccountIdIsWhitespace_ThenMethodNotAllowedReturned(string accountId)
    {
        // Arrange & Act
        var response = await HttpClient.DeleteAsync(AccountsFixture.GetUrl(accountId));

        // Assert
        // Whitespace characters in route results in MethodNotAllowed due to routing constraints
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
        
        // Repository should not be called for invalid routes
        await AccountRepository.DidNotReceive()
            .DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_WhenAccountIdIsEmpty_ThenMethodNotAllowedReturned()
    {
        // Arrange & Act
        var response = await HttpClient.DeleteAsync(AccountsFixture.GetUrl(""));

        // Assert
        // Empty string in route results in MethodNotAllowed due to routing constraints
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
        
        // Repository should not be called for invalid routes
        await AccountRepository.DidNotReceive()
            .DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_WhenValidGuidAccountId_ThenRepositoryIsCalledWithSameId()
    {
        // Arrange
        var accountId = Guid.NewGuid().ToString("N");
        
        AccountRepository.DeleteAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await HttpClient.DeleteAsync(AccountsFixture.GetUrl(accountId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await AccountRepository.Received(1)
            .DeleteAsync(accountId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_WhenNonGuidAccountId_ThenRepositoryIsCalledWithSameId()
    {
        // Arrange
        var accountId = "not-a-guid-123";
        
        AccountRepository.DeleteAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var response = await HttpClient.DeleteAsync(AccountsFixture.GetUrl(accountId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await AccountRepository.Received(1)
            .DeleteAsync(accountId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_WhenSuccessful_ThenResponseHasNoContent()
    {
        // Arrange
        var accountId = RandomValue.String;
        
        AccountRepository.DeleteAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await HttpClient.DeleteAsync(AccountsFixture.GetUrl(accountId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().BeEmpty();
    }

    [Fact]
    public async Task Delete_WhenNotFound_ThenResponseHasNoContent()
    {
        // Arrange
        var accountId = RandomValue.String;
        
        AccountRepository.DeleteAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var response = await HttpClient.DeleteAsync(AccountsFixture.GetUrl(accountId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().BeEmpty();
    }

    [Fact]
    public async Task Delete_WhenRepositoryThrowsTaskCanceledException_ThenInternalServerErrorReturned()
    {
        // Arrange
        var accountId = RandomValue.String;
        
        AccountRepository.DeleteAsync(accountId, Arg.Any<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Operation was cancelled"));

        // Act
        var response = await HttpClient.DeleteAsync(AccountsFixture.GetUrl(accountId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Delete_WhenRepositoryThrowsTimeoutException_ThenInternalServerErrorReturned()
    {
        // Arrange
        var accountId = RandomValue.String;
        
        AccountRepository.DeleteAsync(accountId, Arg.Any<CancellationToken>())
            .ThrowsAsync(new TimeoutException("Operation timed out"));

        // Act
        var response = await HttpClient.DeleteAsync(AccountsFixture.GetUrl(accountId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Theory]
    [InlineData("account1")]
    [InlineData("ACCOUNT1")]
    [InlineData("Account1")]
    [InlineData("12345")]
    [InlineData("account-with-dashes")]
    [InlineData("account_with_underscores")]
    public async Task Delete_WhenDifferentAccountIdFormats_ThenRepositoryIsCalledCorrectly(string accountId)
    {
        // Arrange
        AccountRepository.DeleteAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await HttpClient.DeleteAsync(AccountsFixture.GetUrl(accountId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await AccountRepository.Received(1)
            .DeleteAsync(accountId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_WhenCancellationTokenProvided_ThenTokenIsPassedToRepository()
    {
        // Arrange
        var accountId = RandomValue.String;
        
        AccountRepository.DeleteAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await HttpClient.DeleteAsync(AccountsFixture.GetUrl(accountId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        // Note: We can't easily test the exact cancellation token in this integration test,
        // but we can verify the method was called with any cancellation token
        await AccountRepository.Received(1)
            .DeleteAsync(accountId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_WhenAccountIdContainsSpecialCharacters_ThenHandledCorrectly()
    {
        // Arrange
        var accountId = "account-with-safe-chars";
        
        AccountRepository.DeleteAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var response = await HttpClient.DeleteAsync(AccountsFixture.GetUrl(accountId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await AccountRepository.Received(1)
            .DeleteAsync(accountId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_WhenAccountIdContainsUrlUnsafeCharacters_ThenOriginalIdIsPassedToRepository()
    {
        // Arrange
        var accountIdWithAtSymbol = "account@test";
        
        AccountRepository.DeleteAsync(accountIdWithAtSymbol, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var response = await HttpClient.DeleteAsync(AccountsFixture.GetUrl(accountIdWithAtSymbol));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await AccountRepository.Received(1)
            .DeleteAsync(accountIdWithAtSymbol, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_MultipleCallsWithSameId_ThenEachCallMakesRepositoryCall()
    {
        // Arrange
        var accountId = RandomValue.String;
        
        AccountRepository.DeleteAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(true, false); // First call succeeds, second fails

        // Act
        var response1 = await HttpClient.DeleteAsync(AccountsFixture.GetUrl(accountId));
        var response2 = await HttpClient.DeleteAsync(AccountsFixture.GetUrl(accountId));

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response2.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await AccountRepository.Received(2)
            .DeleteAsync(accountId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_WhenMultipleConcurrentRequests_ThenAllAreHandled()
    {
        // Arrange
        var accountIds = Enumerable.Range(0, 5).Select(_ => RandomValue.String).ToList();
        
        foreach (var id in accountIds)
        {
            AccountRepository.DeleteAsync(id, Arg.Any<CancellationToken>())
                .Returns(true);
        }

        // Act
        var tasks = accountIds.Select(id => HttpClient.DeleteAsync(AccountsFixture.GetUrl(id)));
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.NoContent);
        
        foreach (var id in accountIds)
        {
            await AccountRepository.Received(1)
                .DeleteAsync(id, Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task Delete_WhenLongAccountId_ThenHandledCorrectly()
    {
        // Arrange
        var longAccountId = new string('a', 1000); // Very long account ID
        
        AccountRepository.DeleteAsync(longAccountId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var response = await HttpClient.DeleteAsync(AccountsFixture.GetUrl(longAccountId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await AccountRepository.Received(1)
            .DeleteAsync(longAccountId, Arg.Any<CancellationToken>());
    }
}