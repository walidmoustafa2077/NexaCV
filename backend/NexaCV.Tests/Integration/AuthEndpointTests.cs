using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using NexaCV.Api.DTOs.Auth;

namespace NexaCV.Tests.Integration;

/// <summary>Integration tests for Auth endpoints (10 tests).</summary>
public class AuthEndpointTests : IClassFixture<NexaCVWebFactory>
{
    private readonly NexaCVWebFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public AuthEndpointTests(NexaCVWebFactory factory) => _factory = factory;

    private HttpClient NewClient() => _factory.CreateClient();

    // ── POST /api/auth/register ───────────────────────────────────────────────

    [Fact]
    public async Task Register_ValidRequest_Returns201WithToken()
    {
        var res = await NewClient().PostAsJsonAsync("/api/auth/register",
            ApiHelper.BuildRegisterRequest());

        res.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await res.Content.ReadFromJsonAsync<AuthResponse>(JsonOpts);
        body!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var client = NewClient();
        var req = ApiHelper.BuildRegisterRequest();
        await client.PostAsJsonAsync("/api/auth/register", req);

        var res = await client.PostAsJsonAsync("/api/auth/register",
            ApiHelper.BuildRegisterRequest(email: req.Email));

        res.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_DuplicateUsername_Returns409()
    {
        var client = NewClient();
        var username = $"dup{Guid.NewGuid():N}";
        await client.PostAsJsonAsync("/api/auth/register",
            ApiHelper.BuildRegisterRequest(username: username));

        var res = await client.PostAsJsonAsync("/api/auth/register",
            ApiHelper.BuildRegisterRequest(username: username));

        res.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_InvalidEmail_Returns422()
    {
        var req = ApiHelper.BuildRegisterRequest(email: "not-an-email");
        var res = await NewClient().PostAsJsonAsync("/api/auth/register", req);

        res.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Register_PasswordTooShort_Returns422()
    {
        var req = ApiHelper.BuildRegisterRequest();
        req.Password = "Ab1!"; // < 8 chars

        var res = await NewClient().PostAsJsonAsync("/api/auth/register", req);

        res.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Register_PasswordNoSpecialChar_Returns422()
    {
        var req = ApiHelper.BuildRegisterRequest();
        req.Password = "Password123"; // no special character

        var res = await NewClient().PostAsJsonAsync("/api/auth/register", req);

        res.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    // ── POST /api/auth/login ──────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        var client = NewClient();
        var req = ApiHelper.BuildRegisterRequest();
        await client.PostAsJsonAsync("/api/auth/register", req);

        var res = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = req.Email, Password = req.Password });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadFromJsonAsync<AuthResponse>(JsonOpts);
        body!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var client = NewClient();
        var req = ApiHelper.BuildRegisterRequest();
        await client.PostAsJsonAsync("/api/auth/register", req);

        var res = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = req.Email, Password = "WrongP@ss1!" });

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_UnknownEmail_Returns401()
    {
        var res = await NewClient().PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = "nobody@nowhere.com", Password = "P@ssw0rd!" });

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── POST /api/auth/logout ─────────────────────────────────────────────────

    [Fact]
    public async Task Logout_WithValidToken_Returns204()
    {
        var client = NewClient();
        var (token, _, _) = await ApiHelper.RegisterAsync(client);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var res = await client.PostAsync("/api/auth/logout", null);

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
