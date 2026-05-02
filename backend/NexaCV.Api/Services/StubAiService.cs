using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using NexaCV.Api.Settings;

namespace NexaCV.Api.Services;

public class StubAiService(IHttpClientFactory httpClientFactory, IOptions<AiServiceSettings> options) : IAiService
{
    private readonly string _baseUrl = options.Value.BaseUrl.TrimEnd('/');

    // ── Generate ──────────────────────────────────────────────
    /// <summary>
    /// If BaseUrl is configured, forwards rawData to NexaCV.AiMock for AI-polished output.
    /// Otherwise wraps the wizard's rawData inside the canonical FinalData schema as-is.
    /// </summary>
    public async Task<AiGenerationResult> GenerateAsync(string rawDataJson)
    {
        if (!string.IsNullOrWhiteSpace(_baseUrl))
        {
            try
            {
                using var client = httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(options.Value.TimeoutSeconds + 25);

                var rawNode = JsonNode.Parse(rawDataJson) ?? new JsonObject();
                var response = await client.PostAsJsonAsync($"{_baseUrl}/api/ai/generate", rawNode);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<MockGenerateResponse>();
                return new AiGenerationResult(
                    result?.FinalDataJson ?? rawDataJson,
                    AiAvailable: true,
                    result?.JobTitleSuggestions,
                    result?.SkillSuggestions);
            }
            catch (HttpRequestException ex)
            {
                // Mock server is unreachable — fall through to local stub.
                _ = ex; // suppress warning
            }
        }

        return LocalGenerate(rawDataJson);
    }

    // ── Regenerate ────────────────────────────────────────────
    /// <summary>
    /// If BaseUrl is configured, forwards the full context to NexaCV.AiMock.
    /// Otherwise echoes the user prompt as the regenerated content.
    /// </summary>
    public async Task<AiRegenerationResult> RegenerateAsync(AiRegenerateContext context)
    {
        if (!string.IsNullOrWhiteSpace(_baseUrl))
        {
            try
            {
                using var client = httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(options.Value.TimeoutSeconds + 25);

                var response = await client.PostAsJsonAsync($"{_baseUrl}/api/ai/regenerate", context);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<MockRegenerateResponse>();
                return new AiRegenerationResult(result?.UpdatedContent ?? context.UserPrompt, AiAvailable: true);
            }
            catch (HttpRequestException ex)
            {
                _ = ex;
            }
        }

        return new AiRegenerationResult(context.UserPrompt, AiAvailable: false);
    }

    // ── Local stub (no mock server) ───────────────────────────
    private static AiGenerationResult LocalGenerate(string rawDataJson)
    {
        JsonNode rawNode;
        try { rawNode = JsonNode.Parse(rawDataJson) ?? new JsonObject(); }
        catch { rawNode = new JsonObject(); }

        var rawSettings = rawNode["settings"];
        var content = rawNode["content"]?.DeepClone() ?? new JsonObject();

        var root = new JsonObject
        {
            ["settings"] = new JsonObject
            {
                ["summaryType"] = TryGetString(rawSettings?["summaryType"]) ?? "SUMMARY",
                ["descriptionFormat"] = TryGetString(rawSettings?["descriptionFormat"]) ?? "BULLET",
                ["skillsFormat"] = TryGetString(rawSettings?["skillsFormat"]) ?? "GRID"
            },
            ["content"] = content
        };

        return new AiGenerationResult(root.ToJsonString(), AiAvailable: false);
    }

    /// <summary>
    /// Returns the string value of a JsonNode only when it is a JSON string.
    /// Numeric enum values (from legacy serialization) are intentionally ignored so
    /// the caller's ?? fallback produces a valid default string.
    /// </summary>
    private static string? TryGetString(JsonNode? node) =>
        node is JsonValue val && val.TryGetValue<string>(out var s) ? s : null;

    // ── Private DTOs for mock API responses ───────────────────
    private sealed record MockGenerateResponse(
        string FinalDataJson,
        bool AiAvailable,
        List<AiJobTitleSuggestion>? JobTitleSuggestions,
        List<string>? SkillSuggestions);
    private sealed record MockRegenerateResponse(string UpdatedContent, bool AiAvailable);
}

