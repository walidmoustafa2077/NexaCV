using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace NexaCV.Tests.Integration;

/// <summary>Integration tests for Resume endpoints (25 tests).</summary>
public class ResumeEndpointTests : IClassFixture<NexaCVWebFactory>
{
    private readonly NexaCVWebFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public ResumeEndpointTests(NexaCVWebFactory factory) => _factory = factory;

    /// <summary>Registers a user and returns an authorized HttpClient + the user token.</summary>
    private async Task<(HttpClient Client, string Token)> AuthClientAsync()
    {
        var client = _factory.CreateClient();
        var (token, _, _) = await ApiHelper.RegisterAsync(client);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return (client, token);
    }

    // ── POST /api/resumes ─────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidRequest_Returns201WithDetail()
    {
        var (client, _) = await AuthClientAsync();

        var res = await client.PostAsJsonAsync("/api/resumes",
            ApiHelper.BuildCreateResumeRequest());

        res.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.GetProperty("id").GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task Create_NoToken_Returns401()
    {
        var res = await _factory.CreateClient().PostAsJsonAsync("/api/resumes",
            ApiHelper.BuildCreateResumeRequest());

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_InvalidTemplateId_Returns404()
    {
        var (client, _) = await AuthClientAsync();

        var res = await client.PostAsJsonAsync("/api/resumes",
            ApiHelper.BuildCreateResumeRequest(templateId: 99999));

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_MissingFirstName_Returns422()
    {
        var (client, _) = await AuthClientAsync();
        var req = ApiHelper.BuildCreateResumeRequest();
        req.RawData.Content.Personal.FirstName = string.Empty;

        var res = await client.PostAsJsonAsync("/api/resumes", req);

        res.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    // ── GET /api/resumes ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_Authenticated_Returns200List()
    {
        var (client, _) = await AuthClientAsync();
        await ApiHelper.CreateResumeAsync(client);

        var res = await client.GetAsync("/api/resumes");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAll_NoToken_Returns401()
    {
        var res = await _factory.CreateClient().GetAsync("/api/resumes");

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_NewUser_ReturnsEmptyArray()
    {
        var (client, _) = await AuthClientAsync();

        var res = await client.GetAsync("/api/resumes");
        var body = await res.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        body.GetArrayLength().Should().Be(0);
    }

    // ── GET /api/resumes/{id} ─────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ValidId_Returns200Detail()
    {
        var (client, _) = await AuthClientAsync();
        var id = await ApiHelper.CreateResumeAsync(client);

        var res = await client.GetAsync($"/api/resumes/{id}");
        var body = await res.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        body.GetProperty("id").GetGuid().Should().Be(id);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        var (client, _) = await AuthClientAsync();

        var res = await client.GetAsync($"/api/resumes/{Guid.NewGuid()}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_WrongUser_Returns403()
    {
        // User A creates a resume
        var (ownerClient, _) = await AuthClientAsync();
        var id = await ApiHelper.CreateResumeAsync(ownerClient);

        // User B tries to access it
        var (otherClient, _) = await AuthClientAsync();
        var res = await otherClient.GetAsync($"/api/resumes/{id}");

        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetById_NoToken_Returns401()
    {
        var (ownerClient, _) = await AuthClientAsync();
        var id = await ApiHelper.CreateResumeAsync(ownerClient);

        var res = await _factory.CreateClient().GetAsync($"/api/resumes/{id}");

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── PUT /api/resumes/{id} ─────────────────────────────────────────────────

    [Fact]
    public async Task UpdateFinalData_ValidRequest_Returns200()
    {
        var (client, _) = await AuthClientAsync();
        var id = await ApiHelper.CreateResumeAsync(client);
        var payload = new { finalData = new { updated = true } };

        var res = await client.PutAsJsonAsync($"/api/resumes/{id}", payload);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateFinalData_WrongUser_Returns403()
    {
        var (ownerClient, _) = await AuthClientAsync();
        var id = await ApiHelper.CreateResumeAsync(ownerClient);

        var (otherClient, _) = await AuthClientAsync();
        var res = await otherClient.PutAsJsonAsync($"/api/resumes/{id}",
            new { finalData = new { } });

        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── DELETE /api/resumes/{id} ──────────────────────────────────────────────

    [Fact]
    public async Task Delete_ValidRequest_Returns204()
    {
        var (client, _) = await AuthClientAsync();
        var id = await ApiHelper.CreateResumeAsync(client);

        var res = await client.DeleteAsync($"/api/resumes/{id}");

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_WrongUser_Returns403()
    {
        var (ownerClient, _) = await AuthClientAsync();
        var id = await ApiHelper.CreateResumeAsync(ownerClient);

        var (otherClient, _) = await AuthClientAsync();
        var res = await otherClient.DeleteAsync($"/api/resumes/{id}");

        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_PaidResume_Returns400()
    {
        // Full flow: create → checkout → fulfill via webhook → delete
        var (client, _) = await AuthClientAsync();
        var id = await ApiHelper.CreateResumeAsync(client);

        var checkoutRes = await client.PostAsJsonAsync("/api/transactions/checkout",
            new { resumeId = id, currency = "USD" });
        checkoutRes.EnsureSuccessStatusCode();
        var checkout = await checkoutRes.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var txId = checkout.GetProperty("transactionId").GetGuid().ToString();

        var webhookReq = new HttpRequestMessage(HttpMethod.Post, "/api/webhooks/payment");
        webhookReq.Headers.Add("X-Stub-Ref", txId);
        webhookReq.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        var webhookRes = await client.SendAsync(webhookReq);
        webhookRes.EnsureSuccessStatusCode();

        var res = await client.DeleteAsync($"/api/resumes/{id}");

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── PATCH /api/resumes/{id}/name ──────────────────────────────────────────

    [Fact]
    public async Task Rename_ValidName_Returns200()
    {
        var (client, _) = await AuthClientAsync();
        var id = await ApiHelper.CreateResumeAsync(client);

        var res = await client.PatchAsJsonAsync($"/api/resumes/{id}/name",
            new { name = "My Updated Resume" });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.GetProperty("name").GetString().Should().Be("My Updated Resume");
    }

    [Fact]
    public async Task Rename_EmptyName_Returns422()
    {
        var (client, _) = await AuthClientAsync();
        var id = await ApiHelper.CreateResumeAsync(client);

        var res = await client.PatchAsJsonAsync($"/api/resumes/{id}/name",
            new { name = "" });

        res.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Rename_NameTooLong_Returns422()
    {
        var (client, _) = await AuthClientAsync();
        var id = await ApiHelper.CreateResumeAsync(client);
        var longName = new string('A', 101);

        var res = await client.PatchAsJsonAsync($"/api/resumes/{id}/name",
            new { name = longName });

        res.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Rename_WrongUser_Returns403()
    {
        var (ownerClient, _) = await AuthClientAsync();
        var id = await ApiHelper.CreateResumeAsync(ownerClient);

        var (otherClient, _) = await AuthClientAsync();
        var res = await otherClient.PatchAsJsonAsync($"/api/resumes/{id}/name",
            new { name = "Stolen Name" });

        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── POST /api/resumes/{id}/regenerate ─────────────────────────────────────

    [Fact]
    public async Task Regenerate_ValidRequest_Returns200()
    {
        var (client, _) = await AuthClientAsync();
        var id = await ApiHelper.CreateResumeAsync(client);

        var res = await client.PostAsJsonAsync($"/api/resumes/{id}/regenerate",
            new { sectionIdentifier = "summary", userPrompt = "Make it concise" });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Regenerate_MissingSectionIdentifier_Returns422()
    {
        var (client, _) = await AuthClientAsync();
        var id = await ApiHelper.CreateResumeAsync(client);

        var res = await client.PostAsJsonAsync($"/api/resumes/{id}/regenerate",
            new { sectionIdentifier = "", userPrompt = "Make it concise" });

        res.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Regenerate_AtLimit_Returns429()
    {
        var (client, _) = await AuthClientAsync();
        var id = await ApiHelper.CreateResumeAsync(client);

        // Exhaust 3 regenerations for the same section
        for (var i = 0; i < 3; i++)
        {
            var okRes = await client.PostAsJsonAsync($"/api/resumes/{id}/regenerate",
                new { sectionIdentifier = "summary", userPrompt = $"Pass {i + 1}" });
            okRes.EnsureSuccessStatusCode();
        }

        // 4th call must hit the limit
        var res = await client.PostAsJsonAsync($"/api/resumes/{id}/regenerate",
            new { sectionIdentifier = "summary", userPrompt = "Over the limit" });

        res.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    // ── GET /api/resumes/{id}/render ──────────────────────────────────────────

    [Fact]
    public async Task Render_ValidRequest_Returns200Html()
    {
        var (client, _) = await AuthClientAsync();
        var id = await ApiHelper.CreateResumeAsync(client);

        var res = await client.GetAsync($"/api/resumes/{id}/render");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Content.Headers.ContentType!.MediaType.Should().Be("text/html");
    }

    // ── GET /api/resumes/{id}/download ────────────────────────────────────────

    [Fact]
    public async Task Download_DraftResume_Returns401()
    {
        // A freshly created resume is COMPLETED (stub AI), not PAID — download must be denied
        var (client, _) = await AuthClientAsync();
        var id = await ApiHelper.CreateResumeAsync(client);

        var res = await client.GetAsync($"/api/resumes/{id}/download");

        // GetForDownloadAsync throws UnauthorizedAccessException if not paid → 401
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
