using FluentAssertions;
using Nexus.CustomerOrder.Application.Features.Accounts.Infrastructure.StorageAccount;
using Nexus.CustomerOrder.Application.Features.Accounts.Models;
using Nexus.CustomerOrder.Application.Features.Accounts.Ports;
using Nexus.Shared.Core.Tests;
using Nexus.Shared.Core.Tests.Httpresponses;
using NSubstitute;
using System.Net;

namespace Nexus.CustomerOrder.Api.Tests.Units.Features.V1.Accounts;

public class AccountGetTests : IClassFixture<AccountsFixture>
{
    private readonly AccountsFixture _fixture;
    private IAccountRepository AccountRepository => _fixture.AccountRepository;
    private HttpClient HttpClient => _fixture.HttpClient;

    public AccountGetTests(AccountsFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetSubstitutes();
    }

    [Fact]
    public async Task Get_WhenAccountDoesNotExist_Then404NotFoundIsReturned()
    {
        var id = RandomValue.String;

        AccountRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((AccountTableEntity?)null);

        // Act
        var response = await HttpClient.GetAsync(AccountsFixture.GetUrl(id));

        // Assert :: NotFound response
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Assert :: Exists was called
        await AccountRepository.Received(1)
            .GetByIdAsync(id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Get_WhenAccountExists_ThenResponseIsReturned()
    {
        var payload = EmbeddedData.DeserializeObject<GetAccountDto>("account-get.json");
        var id = payload.id;

        // Map GetAccountDto to AccountTableEntity
        var accountTableEntity = CreateEntity(payload);

        // Intercept call to repository
        AccountRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<AccountTableEntity?>(accountTableEntity));

        // Act
        var response = await HttpClient.GetAsync(AccountsFixture.GetUrl(id));

        // Assert :: Mapped Correctly
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var accountResponse = await response.DeserializeBodyAsync<GetAccountDto>();

        accountResponse.Should().BeEquivalentTo(payload);
    }

    [Fact]
    public async Task Get_WhenAccountExists_RepositoryIsCalledOnce_WithCorrectId()
    {
        var payload = EmbeddedData.DeserializeObject<GetAccountDto>("account-get.json");
        var id = payload.id;

        var entity = CreateEntity(payload);

        AccountRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<AccountTableEntity?>(entity));

        var response = await HttpClient.GetAsync(AccountsFixture.GetUrl(id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await AccountRepository.Received(1).GetByIdAsync(id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Get_WhenPhoneNumberIsNull_ThenPhoneIsEmptyString()
    {
        var payload = EmbeddedData.DeserializeObject<GetAccountDto>("account-get.json");
        var id = payload.id;

        var entity = CreateEntity(payload, e => e.PhoneNumber = null); // simulate missing phone

        AccountRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<AccountTableEntity?>(entity));

        var response = await HttpClient.GetAsync(AccountsFixture.GetUrl(id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.DeserializeBodyAsync<GetAccountDto>();
        dto.Phone.Should().BeEmpty();
    }

    [Fact]
    public async Task Get_WhenOptionalAddressFieldsAreNull_ThenTheyRemainNullInResponse()
    {
        var payload = EmbeddedData.DeserializeObject<GetAccountDto>("account-get.json");
        var id = payload.id;

        var entity = CreateEntity(payload, e =>
        {
            e.Address_Street2 = null; // optional
            e.Address_State = null;   // optional
        });

        AccountRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<AccountTableEntity?>(entity));

        var response = await HttpClient.GetAsync(AccountsFixture.GetUrl(id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.DeserializeBodyAsync<GetAccountDto>();
        dto.Address.Street2.Should().BeNull();
        dto.Address.State.Should().BeNull();
    }

    [Fact]
    public async Task Get_Response_IsJson()
    {
        var payload = EmbeddedData.DeserializeObject<GetAccountDto>("account-get.json");
        var id = payload.id;

        var entity = CreateEntity(payload);

        AccountRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<AccountTableEntity?>(entity));

        var response = await HttpClient.GetAsync(AccountsFixture.GetUrl(id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    // Helper to reduce duplication across tests
    private static AccountTableEntity CreateEntity(GetAccountDto payload, Action<AccountTableEntity>? mutate = null)
    {
        var entity = new AccountTableEntity
        {
            PartitionKey = AccountTableEntity.DefaultPartitionKey,
            RowKey = payload.id,
            FirstName = payload.FirstName,
            LastName = payload.LastName,
            Email = payload.Email,
            PhoneNumber = payload.Phone,
            Address_Street1 = payload.Address.Street1,
            Address_Street2 = payload.Address.Street2,
            Address_City = payload.Address.City,
            Address_State = payload.Address.State,
            Address_PostalCode = payload.Address.PostalCode,
            Address_Country = payload.Address.Country
        };

        mutate?.Invoke(entity);
        return entity;
    }
}