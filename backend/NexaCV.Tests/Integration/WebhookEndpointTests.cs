using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace NexaCV.Tests.Integration;

/// <summary>Integration tests for Webhook endpoints (5 tests).</summary>
public class WebhookEndpointTests : IClassFixture<NexaCVWebFactory>
{
    private readonly NexaCVWebFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public WebhookEndpointTests(NexaCVWebFactory factory) => _factory = factory;

    private async Task<(HttpClient Client, string Token)> AuthClientAsync()
    {
        var client = _factory.CreateClient();
        var (token, _, _) = await ApiHelper.RegisterAsync(client);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return (client, token);
    }

    private static HttpRequestMessage StubWebhook(string? stubRef)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/webhooks/payment");
        if (stubRef is not null)
            req.Headers.Add("X-Stub-Ref", stubRef);
        req.Content = new StringContent("{}", Encoding.UTF8, "application/json");
        return req;
    }

    // ── POST /api/webhooks/payment ────────────────────────────────────────────

    [Fact]
    public async Task Payment_WithoutXStubRefHeader_Returns400()
    {
        var res = await _factory.CreateClient().SendAsync(StubWebhook(null));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Payment_UnknownRefId_Returns404()
    {
        // X-Stub-Ref with a valid-looking but non-existent transaction GUID
        var res = await _factory.CreateClient().SendAsync(
            StubWebhook(Guid.NewGuid().ToString()));

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Payment_ValidRefId_Returns200()
    {
        var (client, _) = await AuthClientAsync();
        var resumeId = await ApiHelper.CreateResumeAsync(client);

        var checkoutRes = await client.PostAsJsonAsync("/api/transactions/checkout",
            new { resumeId, currency = "USD" });
        var checkout = await checkoutRes.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var txId = checkout.GetProperty("transactionId").GetGuid().ToString();

        var res = await _factory.CreateClient().SendAsync(StubWebhook(txId));

        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task FullFlow_RegisterCreateCheckoutWebhook_ResumeBecomesPaymentPaid()
    {
        // 1. Register & authenticate
        var (client, _) = await AuthClientAsync();

        // 2. Create resume (stub AI sets it to COMPLETED immediately)
        var resumeId = await ApiHelper.CreateResumeAsync(client);

        // 3. Checkout
        var checkoutRes = await client.PostAsJsonAsync("/api/transactions/checkout",
            new { resumeId, currency = "USD" });
        checkoutRes.EnsureSuccessStatusCode();
        var checkout = await checkoutRes.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var txId = checkout.GetProperty("transactionId").GetGuid().ToString();

        // 4. Webhook fulfillment
        var webhookRes = await _factory.CreateClient().SendAsync(StubWebhook(txId));
        webhookRes.EnsureSuccessStatusCode();

        // 5. Transaction should now be completed
        var txRes = await client.GetAsync($"/api/transactions/{txId}");
        var txBody = await txRes.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);

        txRes.StatusCode.Should().Be(HttpStatusCode.OK);
        txBody.GetProperty("paymentStatus").GetString()
              .Should().BeEquivalentTo("Success");
    }

    [Fact]
    public async Task FullFlow_PaidResume_DownloadReturns501()
    {
        // Verifies that a PAID resume can pass the download validation
        // and reach the 501 Not Implemented stub
        var (client, _) = await AuthClientAsync();
        var resumeId = await ApiHelper.CreateResumeAsync(client);

        var checkoutRes = await client.PostAsJsonAsync("/api/transactions/checkout",
            new { resumeId, currency = "USD" });
        var checkout = await checkoutRes.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var txId = checkout.GetProperty("transactionId").GetGuid().ToString();

        (await _factory.CreateClient().SendAsync(StubWebhook(txId))).EnsureSuccessStatusCode();

        var res = await client.GetAsync($"/api/resumes/{resumeId}/download");

        res.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
    }
}
