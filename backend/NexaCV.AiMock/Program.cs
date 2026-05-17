using System.Text.Json;
using System.Text.Json.Nodes;
using NexaCV.AiMock.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<MockEngine>();

var app = builder.Build();

// Eagerly initialise MockEngine so missing JSON files surface at startup rather than on first request.
_ = app.Services.GetRequiredService<MockEngine>();

// ── POST /api/ai/generate ─────────────────────────────────────────────────
// Accepts the full RawResumeData JSON and returns a polished finalData payload
// alongside job-title and skill suggestions.
app.MapPost("/api/ai/generate", async (JsonElement body, MockEngine engine) =>
{
    JsonNode rawData;
    try
    {
        rawData = JsonNode.Parse(body.GetRawText()) ?? new JsonObject();
    }
    catch
    {
        return Results.BadRequest(new { error = "Invalid JSON payload." });
    }

    var response = await engine.GenerateAsync(rawData);
    return Results.Ok(response);
});

// ── POST /api/ai/regenerate ───────────────────────────────────────────────
// Accepts an AiRegenerateContext JSON object and returns a mocked regenerated
// string for a single resume section.
app.MapPost("/api/ai/regenerate", async (JsonElement body, MockEngine engine) =>
{
    JsonNode input;
    try
    {
        input = JsonNode.Parse(body.GetRawText()) ?? new JsonObject();
    }
    catch
    {
        return Results.BadRequest(new { error = "Invalid JSON payload." });
    }

    var response = await engine.RegenerateAsync(input);
    return Results.Ok(response);
});

app.Run();
