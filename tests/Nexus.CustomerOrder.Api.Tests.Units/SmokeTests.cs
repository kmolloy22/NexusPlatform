using FluentAssertions;
using Nexus.CustomerOrder.Api.Tests.Units.Shared;
using Nexus.CustomerOrder.Application.Features.Accounts.Models;
using System.Net;
using System.Net.Http.Json;

namespace Nexus.CustomerOrder.Api.Tests.Smoke;

public class SmokeTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public SmokeTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_Then_Get_By_Id_Works()
    {
        var create = new CreateAccountDto(
            "Ada",
            "Lovelace",
            "555-0001",
            "ada@example.test",
            new AddressDto("1 Main", null, "London", null, "EC1A1", "UK"));

        var createResp = await _client.PostAsJsonAsync("/api/accounts", create);
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var location = createResp.Headers.Location!.ToString();
        var id = location.Split('/').Last();

        var getResp = await _client.GetAsync($"/api/accounts/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await getResp.Content.ReadFromJsonAsync<GetAccountView>();
        dto!.id.Should().Be(id);
        dto.FirstName.Should().Be(create.FirstName);
        dto.LastName.Should().Be(create.LastName);
    }

    [Fact]
    public async Task List_And_Update_And_Delete_Work()
    {
        // create
        var create = new CreateAccountDto(
            "Grace", "Hopper", "555-0002", "grace@example.test",
            new AddressDto("2 Fleet", null, "London", null, "EC2A2", "UK"));
        var created = await _client.PostAsJsonAsync("/api/accounts", create);
        var id = created.Headers.Location!.ToString().Split('/').Last();

        // list
        var list = await _client.GetAsync("/api/accounts?take=10");
        list.StatusCode.Should().Be(HttpStatusCode.OK);

        // update
        var update = create with { FirstName = "Grace Marie" };
        var put = await _client.PutAsJsonAsync($"/api/accounts/{id}", update);
        put.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // delete
        var del = await _client.DeleteAsync($"/api/accounts/{id}");
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private sealed record GetAccountView(string id, string FirstName, string LastName, string? Email, string Phone, AddressDto Address);
}