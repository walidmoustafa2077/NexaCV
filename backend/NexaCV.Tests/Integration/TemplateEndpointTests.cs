using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace NexaCV.Tests.Integration;

/// <summary>Integration tests for Template endpoints (6 tests).</summary>
public class TemplateEndpointTests : IClassFixture<NexaCVWebFactory>
{
    private readonly NexaCVWebFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public TemplateEndpointTests(NexaCVWebFactory factory) => _factory = factory;

    private HttpClient NewClient() => _factory.CreateClient();

    // ── GET /api/templates ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_NoFilter_Returns200WithList()
    {
        var res = await NewClient().GetAsync("/api/templates");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAll_WithIndustryCategoryFilter_ReturnsOnlyMatchingTemplates()
    {
        // The DataSeeder seeds at least one "Corporate" template
        var res = await NewClient().GetAsync("/api/templates?industryCategory=Corporate");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var items = body.EnumerateArray().ToList();
        items.Should().NotBeEmpty();
        items.Should().AllSatisfy(t =>
            t.GetProperty("industryCategory").GetString()
             .Should().Be("Corporate"));
    }

    // ── GET /api/templates/{id} ───────────────────────────────────────────────

    [Fact]
    public async Task GetById_ValidId_Returns200()
    {
        var res = await NewClient().GetAsync("/api/templates/1");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.GetProperty("id").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        var res = await NewClient().GetAsync("/api/templates/99999");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── GET /api/templates/{id}/preview ──────────────────────────────────────

    [Fact]
    public async Task Preview_ValidId_Returns200Html()
    {
        var res = await NewClient().GetAsync("/api/templates/1/preview");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Content.Headers.ContentType!.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task Preview_NotFound_Returns404()
    {
        var res = await NewClient().GetAsync("/api/templates/99999/preview");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
