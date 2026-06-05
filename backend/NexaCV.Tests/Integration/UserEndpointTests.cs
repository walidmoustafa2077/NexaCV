using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace NexaCV.Tests.Integration;

/// <summary>Integration tests for User endpoints (5 tests).</summary>
public class UserEndpointTests : IClassFixture<NexaCVWebFactory>
{
    private readonly NexaCVWebFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public UserEndpointTests(NexaCVWebFactory factory) => _factory = factory;

    // ── GET /api/users/me ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetMe_Authenticated_Returns200Profile()
    {
        var client = _factory.CreateClient();
        var req = ApiHelper.BuildRegisterRequest();
        var (token, _, _) = await ApiHelper.RegisterAsync(client, req);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var res = await client.GetAsync("/api/users/me");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.GetProperty("email").GetString().Should().Be(req.Email);
    }

    [Fact]
    public async Task GetMe_NoToken_Returns401()
    {
        var res = await _factory.CreateClient().GetAsync("/api/users/me");

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── PUT /api/users/me ─────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateMe_UpdateFirstName_Returns200WithUpdatedProfile()
    {
        var client = _factory.CreateClient();
        var (token, _, _) = await ApiHelper.RegisterAsync(client);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var res = await client.PutAsJsonAsync("/api/users/me",
            new { firstName = "UpdatedName" });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.GetProperty("firstName").GetString().Should().Be("UpdatedName");
    }

    [Fact]
    public async Task UpdateMe_UpdatePassword_Returns200()
    {
        var client = _factory.CreateClient();
        var (token, _, _) = await ApiHelper.RegisterAsync(client);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var res = await client.PutAsJsonAsync("/api/users/me",
            new { password = "N3wP@ss!123" });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateMe_NoToken_Returns401()
    {
        var res = await _factory.CreateClient().PutAsJsonAsync("/api/users/me",
            new { firstName = "Anything" });

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
