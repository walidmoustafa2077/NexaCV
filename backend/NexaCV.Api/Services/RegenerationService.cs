using System.Text.Json.Nodes;
using NexaCV.Api.DTOs.Resumes;
using NexaCV.Api.Extensions;
using NexaCV.Api.Models;
using NexaCV.Api.Repositories;

namespace NexaCV.Api.Services;

public class RegenerationService : IRegenerationService
{
    private const int MaxRegenerationsPerSection = 3;

    private readonly IResumeRepository _resumes;
    private readonly IRegenerationRepository _regenerations;
    private readonly IResumeHistoryRepository _history;
    private readonly IAiService _ai;

    public RegenerationService(
        IResumeRepository resumes,
        IRegenerationRepository regenerations,
        IResumeHistoryRepository history,
        IAiService ai)
    {
        _resumes = resumes;
        _regenerations = regenerations;
        _history = history;
        _ai = ai;
    }

    public async Task<RegenerateResponse> RegenerateAsync(Guid resumeId, Guid userId, RegenerateRequest req)
    {
        var resume = await _resumes.GetWithTemplateAsync(resumeId)
            ?? throw new KeyNotFoundException("Resume not found.");

        if (resume.UserId != userId)
            throw new ForbiddenException("Access denied.");

        var count = await _regenerations.CountBySectionAsync(resumeId, req.SectionIdentifier);

        if (count >= MaxRegenerationsPerSection)
            throw new TooManyRegenerationsException();

        // Parse FinalData — supports both legacy { settings, content } and new { content } shapes.
        var root = ParseFinalData(resume.FinalData);
        var content = root["content"]?.AsObject() ?? root; // backward-compat: old docs have a "content" key

        // Extract resume context so the AI can produce coherent output
        var personal = content["personal"]?.AsObject();
        var titleFromPersonal = personal?["title"]?.GetValue<string>();
        var nameFromPersonal = $"{personal?["firstName"]?.GetValue<string>()} {personal?["lastName"]?.GetValue<string>()}".Trim();
        var resumeTitle = titleFromPersonal ?? (nameFromPersonal.Length > 0 ? nameFromPersonal : null);

        var skillsNode = content["skills"];
        var skills = skillsNode is JsonArray arr
            ? string.Join(", ", arr.Select(s =>
                s is JsonValue sv && sv.TryGetValue<string>(out var sv2) ? sv2 :
                s is JsonObject so ? so["name"]?.GetValue<string>() ?? string.Empty :
                string.Empty).Where(s => s.Length > 0))
            : skillsNode?.GetValue<string>();

        // Settings are now template-defined; no CurrentDescriptionFormat from stored data.
        // Resolve content for the AI: direct key (e.g. "summary", "experience") OR an
        // entry ID inside an array section (e.g. "exp_002" inside content["experience"]).
        JsonObject? entryNode = content.ContainsKey(req.SectionIdentifier)
            ? null
            : FindEntryById(content, req.SectionIdentifier);

        var currentSectionContent = entryNode?.ToJsonString()
            ?? content[req.SectionIdentifier]?.ToJsonString()
            ?? string.Empty;

        var aiContext = new AiRegenerateContext(
            SectionIdentifier: req.SectionIdentifier,
            UserPrompt: req.UserPrompt,
            TargetFormat: req.TargetFormat,
            NewTitleSuggestion: req.NewTitleSuggestion,
            CurrentSectionContent: currentSectionContent,
            ResumeTitle: resumeTitle,
            Skills: string.IsNullOrWhiteSpace(skills) ? null : skills,
            CurrentDescriptionFormat: null);

        var result = await _ai.RegenerateAsync(aiContext);

        // Patch the target section inside content.
        // If the identifier matched an individual array entry (e.g. exp_002), update its
        // "description" field in-place so the rest of the array is preserved.
        if (entryNode != null)
        {
            entryNode["description"] = result.UpdatedContent;
        }
        else
        {
            // result.UpdatedContent may be a plain string (summary, description) OR a
            // JSON-encoded array/object (e.g. skills returned as "[\"C#\",\"React\",...]").
            // JsonValue.Create(string) throws "The node must be of type 'JsonValue'" when
            // the string happens to be valid JSON that represents an array or object,
            // so we try to parse it first and only fall back to a string JsonValue.
            JsonNode? parsed = null;
            if (result.UpdatedContent is not null)
            {
                try { parsed = JsonNode.Parse(result.UpdatedContent); }
                catch { /* not valid JSON — treat as a plain string below */ }
            }
            content[req.SectionIdentifier] = parsed ?? JsonValue.Create(result.UpdatedContent);
        }

        // Rebuild FinalData as content-only (settings are template-defined).
        // Detach content from its current parent before moving it into the new root,
        // otherwise JsonNode throws "The node already has a parent".
        root.Remove("content");
        var updatedRoot = new JsonObject { ["content"] = content };
        resume.FinalData = updatedRoot.ToJsonString();
        resume.UpdatedAt = DateTime.UtcNow;
        await _resumes.UpdateAsync(resume);

        await _history.AddAsync(new ResumeHistory
        {
            Id = Guid.NewGuid(),
            ResumeId = resumeId,
            SnapshotData = resume.FinalData,
            Reason = $"REGEN_{req.SectionIdentifier}",
            CreatedAt = DateTime.UtcNow
        });
        await _history.PruneAsync(resumeId);

        var regen = new Regeneration
        {
            Id = Guid.NewGuid(),
            ResumeId = resumeId,
            SectionIdentifier = req.SectionIdentifier,
            UserPrompt = req.UserPrompt,
            CostUsd = 0.25m,
            CreatedAt = DateTime.UtcNow
        };
        await _regenerations.AddAsync(regen);

        return regen.ToResponseDto(count + 1, result.UpdatedContent ?? string.Empty, result.AiAvailable);
    }

    /// <summary>Safely parses a FinalData JSON string into a JsonObject. Returns an empty object on failure.</summary>
    private static JsonObject ParseFinalData(string? finalDataJson)
    {
        if (string.IsNullOrEmpty(finalDataJson)) return new JsonObject();
        try { return JsonNode.Parse(finalDataJson)?.AsObject() ?? new JsonObject(); }
        catch { return new JsonObject(); }
    }

    /// <summary>
    /// Searches every JsonArray property of <paramref name="content"/> for an entry whose
    /// <c>"id"</c> value equals <paramref name="entryId"/>.
    /// Returns the matching JsonObject, or <c>null</c> if not found.
    /// </summary>
    private static JsonObject? FindEntryById(JsonObject content, string entryId)
    {
        foreach (var (_, node) in content)
        {
            if (node is not JsonArray arr) continue;
            foreach (var item in arr)
            {
                if (item is JsonObject obj &&
                    obj["id"]?.GetValue<string>() == entryId)
                    return obj;
            }
        }
        return null;
    }
}
