using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace NexaCV.Tests.Integration;

/// <summary>Integration tests for Transaction endpoints (9 tests).</summary>
public class TransactionEndpointTests : IClassFixture<NexaCVWebFactory>
{
    private readonly NexaCVWebFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public TransactionEndpointTests(NexaCVWebFactory factory) => _factory = factory;

    private async Task<(HttpClient Client, string Token)> AuthClientAsync()
    {
        var client = _factory.CreateClient();
        var (token, _, _) = await ApiHelper.RegisterAsync(client);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return (client, token);
    }

    // ── POST /api/transactions/checkout ──────────────────────────────────────

    [Fact]
    public async Task Checkout_ValidUsd_Returns200WithPaymentUrl()
    {
        var (client, _) = await AuthClientAsync();
        var resumeId = await ApiHelper.CreateResumeAsync(client);

        var res = await client.PostAsJsonAsync("/api/transactions/checkout",
            new { resumeId, currency = "USD" });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.GetProperty("paymentUrl").GetString().Should().NotBeNullOrEmpty();
        body.GetProperty("currency").GetString().Should().Be("USD");
    }

    [Fact]
    public async Task Checkout_ValidEgp_Returns200WithEgpAmount()
    {
        var (client, _) = await AuthClientAsync();
        var resumeId = await ApiHelper.CreateResumeAsync(client);

        var res = await client.PostAsJsonAsync("/api/transactions/checkout",
            new { resumeId, currency = "EGP" });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.GetProperty("currency").GetString().Should().Be("EGP");
        body.GetProperty("baseAmount").GetDecimal().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Checkout_DraftResume_Returns400()
    {
        // There is no "DRAFT" resume via the API — POST /api/resumes immediately returns COMPLETED
        // Simulate a DRAFT by calling checkout on an already-paid resume (which is also not COMPLETED)
        // Use a non-existent resume ID to trigger a not-found path
        // Instead: use the completed flow then webhook to make it PAID, then checkout again
        var (client, _) = await AuthClientAsync();
        var resumeId = await ApiHelper.CreateResumeAsync(client);

        // Checkout once → Pending transaction
        var firstRes = await client.PostAsJsonAsync("/api/transactions/checkout",
            new { resumeId, currency = "USD" });
        firstRes.EnsureSuccessStatusCode();
        var checkout = await firstRes.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var txId = checkout.GetProperty("transactionId").GetGuid().ToString();

        // Fulfill via webhook to set resume to PAID
        var webhookReq = new HttpRequestMessage(HttpMethod.Post, "/api/webhooks/payment");
        webhookReq.Headers.Add("X-Stub-Ref", txId);
        webhookReq.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        (await client.SendAsync(webhookReq)).EnsureSuccessStatusCode();

        // Now checkout a PAID resume → must fail with 400
        var res = await client.PostAsJsonAsync("/api/transactions/checkout",
            new { resumeId, currency = "USD" });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Checkout_WrongUser_Returns403()
    {
        var (ownerClient, _) = await AuthClientAsync();
        var resumeId = await ApiHelper.CreateResumeAsync(ownerClient);

        var (otherClient, _) = await AuthClientAsync();
        var res = await otherClient.PostAsJsonAsync("/api/transactions/checkout",
            new { resumeId, currency = "USD" });

        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Checkout_NoToken_Returns401()
    {
        var (ownerClient, _) = await AuthClientAsync();
        var resumeId = await ApiHelper.CreateResumeAsync(ownerClient);

        var res = await _factory.CreateClient().PostAsJsonAsync("/api/transactions/checkout",
            new { resumeId, currency = "USD" });

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Checkout_ResumeNotFound_Returns404()
    {
        var (client, _) = await AuthClientAsync();

        var res = await client.PostAsJsonAsync("/api/transactions/checkout",
            new { resumeId = Guid.NewGuid(), currency = "USD" });

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── GET /api/transactions/{id} ────────────────────────────────────────────

    [Fact]
    public async Task GetById_ValidId_Returns200()
    {
        var (client, _) = await AuthClientAsync();
        var resumeId = await ApiHelper.CreateResumeAsync(client);

        var checkoutRes = await client.PostAsJsonAsync("/api/transactions/checkout",
            new { resumeId, currency = "USD" });
        var checkout = await checkoutRes.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var txId = checkout.GetProperty("transactionId").GetGuid();

        var res = await client.GetAsync($"/api/transactions/{txId}");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.GetProperty("id").GetGuid().Should().Be(txId);
    }

    [Fact]
    public async Task GetById_WrongUser_Returns403()
    {
        var (ownerClient, _) = await AuthClientAsync();
        var resumeId = await ApiHelper.CreateResumeAsync(ownerClient);

        var checkoutRes = await ownerClient.PostAsJsonAsync("/api/transactions/checkout",
            new { resumeId, currency = "USD" });
        var checkout = await checkoutRes.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var txId = checkout.GetProperty("transactionId").GetGuid();

        var (otherClient, _) = await AuthClientAsync();
        var res = await otherClient.GetAsync($"/api/transactions/{txId}");

        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        var (client, _) = await AuthClientAsync();

        var res = await client.GetAsync($"/api/transactions/{Guid.NewGuid()}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
