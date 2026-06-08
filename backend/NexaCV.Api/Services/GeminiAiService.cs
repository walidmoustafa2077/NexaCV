using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexaCV.Api.Settings;

namespace NexaCV.Api.Services;

/// <summary>
/// AI service that uses Google Gemini Flash to polish resume content and regenerate sections.
/// Falls back to a local stub on error so the rest of the API remains functional.
/// </summary>
public sealed class GeminiAiService(
    IHttpClientFactory httpClientFactory,
    IOptions<AiServiceSettings> options,
    ILogger<GeminiAiService> logger) // Added Logger for diagnostic visibility
    : IResumeGenerationService, IResumeSectionRegenerationService
{
    private const string GeminiBaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    // ── Generate ──────────────────────────────────────────────
    public async Task<AiGenerationResult> GenerateAsync(string rawDataJson)
    {
        try
        {
            JsonNode rawNode;
            try { rawNode = JsonNode.Parse(rawDataJson) ?? new JsonObject(); }
            catch { rawNode = new JsonObject(); }

            var contentNode = rawNode["content"] ?? rawNode;

            var prompt = $$"""
                You are an expert professional resume writer.
                Transform the following resume content JSON into polished, professional content.
                Keep the exact same JSON structure and all field names, but improve all text fields
                (summary, descriptions, achievements, etc.) to be more professional, impactful, and ATS-friendly.

                Resume Content:
                {{contentNode.ToJsonString()}}

                Return a JSON object with exactly these fields:
                - "content": the improved resume content object (same structure as the input above, all fields preserved)
                - "jobTitleSuggestions": array of {"title": "string", "score": integer_1_to_10}, provide 3-5 relevant suggestions
                - "skillSuggestions": array of skill name strings, provide 5-10 relevant skills
                """;

            var responseText = await CallGeminiAsync(prompt, jsonMode: true);
            if (responseText == null) return LocalGenerate(rawDataJson);

            var responseNode = JsonNode.Parse(responseText);
            if (responseNode == null) return LocalGenerate(rawDataJson);

            var improvedContent = responseNode["content"];
            if (improvedContent == null) return LocalGenerate(rawDataJson);

            var finalDataNode = new JsonObject { ["content"] = improvedContent.DeepClone() };
            var parsed = JsonSerializer.Deserialize<GeminiGenerateResponse>(responseText, JsonOpts);

            return new AiGenerationResult(
                finalDataNode.ToJsonString(),
                AiAvailable: true,
                parsed?.JobTitleSuggestions,
                parsed?.SkillSuggestions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Gemini GenerateAsync failed. Falling back to local stub.");
            return LocalGenerate(rawDataJson);
        }
    }

    // ── Regenerate ────────────────────────────────────────────
    public async Task<AiRegenerationResult> RegenerateAsync(AiRegenerateContext context)
    {
        try
        {
            var formatInstruction = context.TargetFormat?.ToLowerInvariant() switch
            {
                "bulleted" => "Format the output as bullet points, starting each point with a dash (-).",
                "paragraph" => "Format the output as a single flowing paragraph.",
                _ => ""
            };
            var titleHint = context.NewTitleSuggestion is not null
                ? $"New Title Suggestion: {context.NewTitleSuggestion}"
                : string.Empty;

            var prompt = $$"""
                You are an expert professional resume writer.
                Improve the following resume section based on the user's request.

                Section: {{context.SectionIdentifier}}
                Resume Title: {{context.ResumeTitle ?? "Not specified"}}
                Current Content: {{context.CurrentSectionContent}}
                Relevant Skills: {{context.Skills ?? "Not specified"}}
                User Request: {{context.UserPrompt}}
                {{titleHint}}
                {{formatInstruction}}

                Return a JSON object with a single field:
                - "updatedContent": the improved section content as a plain string
                """;

            var responseText = await CallGeminiAsync(prompt, jsonMode: true);
            if (responseText == null)
                return new AiRegenerationResult(context.UserPrompt, AiAvailable: false);

            var parsed = JsonSerializer.Deserialize<GeminiRegenerateResponse>(responseText, JsonOpts);
            var updatedContent = parsed?.UpdatedContent;

            if (string.IsNullOrWhiteSpace(updatedContent))
                return new AiRegenerationResult(context.UserPrompt, AiAvailable: false);

            return new AiRegenerationResult(updatedContent, AiAvailable: true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Gemini RegenerateAsync failed. Falling back to the user's original prompt.");
            return new AiRegenerationResult(context.UserPrompt, AiAvailable: false);
        }
    }

    // ── Core Gemini HTTP call ─────────────────────────────────
    private async Task<string?> CallGeminiAsync(string prompt, bool jsonMode = false)
    {
        try
        {
            var apiKey = options.Value.ApiKey;
            var model = options.Value.Model;
            var url = $"{GeminiBaseUrl}/{model}:generateContent?key={apiKey}";

            using var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(options.Value.TimeoutSeconds + 25);

            object requestBody = jsonMode
                ? new
                {
                    contents = new[] { new { parts = new[] { new { text = prompt } } } },
                    generationConfig = new { responseMimeType = "application/json" }
                }
                : new
                {
                    contents = new[] { new { parts = new[] { new { text = prompt } } } }
                };

            var response = await client.PostAsJsonAsync(url, requestBody);
            response.EnsureSuccessStatusCode();

            var geminiResponse = await response.Content.ReadFromJsonAsync<GeminiApiResponse>(JsonOpts);

            // Safe traversal: FirstOrDefault() avoids throwing an exception if lists are empty
            var firstCandidate = geminiResponse?.Candidates?.FirstOrDefault();
            var rawText = firstCandidate?.Content?.Parts?.FirstOrDefault()?.Text;

            if (string.IsNullOrWhiteSpace(rawText)) return null;

            // Defensive: Strip markdown code blocks if the model outputs them despite instructions
            if (jsonMode && rawText.StartsWith("```"))
            {
                rawText = rawText.Trim();
                if (rawText.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
                {
                    rawText = rawText[7..^3].Trim();
                }
                else if (rawText.StartsWith("```"))
                {
                    rawText = rawText[3..^3].Trim();
                }
            }

            return rawText;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "HTTP request directly to Google Gemini API failed.");
            throw; // Let the caller log context-specific errors and gracefully transition to stubs
        }
    }

    // ── Local fallback (API unavailable / error) ──────────────
    private static AiGenerationResult LocalGenerate(string rawDataJson)
    {
        JsonNode rawNode;
        try { rawNode = JsonNode.Parse(rawDataJson) ?? new JsonObject(); }
        catch { rawNode = new JsonObject(); }

        var content = rawNode["content"]?.DeepClone() ?? new JsonObject();
        var root = new JsonObject { ["content"] = content };
        return new AiGenerationResult(root.ToJsonString(), AiAvailable: false);
    }

    // ── Gemini REST API response DTOs ─────────────────────────
    private sealed record GeminiApiResponse(
        [property: JsonPropertyName("candidates")] List<GeminiCandidate>? Candidates);

    private sealed record GeminiCandidate(
        [property: JsonPropertyName("content")] GeminiContent? Content);

    private sealed record GeminiContent(
        [property: JsonPropertyName("parts")] List<GeminiPart>? Parts);

    private sealed record GeminiPart(
        [property: JsonPropertyName("text")] string? Text);

    private sealed record GeminiGenerateResponse(
        [property: JsonPropertyName("jobTitleSuggestions")] List<AiJobTitleSuggestion>? JobTitleSuggestions,
        [property: JsonPropertyName("skillSuggestions")] List<string>? SkillSuggestions);

    private sealed record GeminiRegenerateResponse(
        [property: JsonPropertyName("updatedContent")] string? UpdatedContent);
}