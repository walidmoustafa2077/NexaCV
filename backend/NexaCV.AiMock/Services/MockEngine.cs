using System.Text.Json;
using System.Text.Json.Nodes;

namespace NexaCV.AiMock.Services;

// ── Internal data models (loaded from Mocks/*.json) ────────────────────────

internal sealed record ExperienceMockData(
    List<string> MetricsDriven,
    List<string> Technical,
    List<string> Leadership);

internal sealed record SummaryMockData(
    Dictionary<string, List<string>> Summaries,
    Dictionary<string, List<string>> Objectives);

internal sealed record JobTitleEntry(string Title, List<string> BoostSkills);
internal sealed record SkillCluster(List<string> Triggers, List<string> Suggestions);

internal sealed record DefaultProjectEntry(
    string Id, string Name, string Role, string Description,
    string? Link, List<string> Technologies);

internal sealed record DefaultLanguageEntry(string Language, string Level);

internal sealed record DefaultVolunteerEntry(
    string Id, string Organization, string Role,
    string StartDate, string? EndDate, string Description);

internal sealed record DefaultOtherEntry(string Label, string Value);

internal sealed record SuggestionsMockData(
    List<JobTitleEntry> JobTitles,
    List<SkillCluster> SkillClusters,
    List<string> MasterSkillPool,
    List<DefaultProjectEntry> DefaultProjects,
    List<DefaultLanguageEntry> DefaultLanguages,
    List<DefaultVolunteerEntry> DefaultVolunteers,
    List<string> DefaultHobbies,
    List<DefaultOtherEntry> DefaultOther);

// ── Public response types ──────────────────────────────────────────────────

public sealed record GenerateResponse(
    string FinalDataJson,
    bool AiAvailable,
    List<JobTitleSuggestion> JobTitleSuggestions,
    List<string> SkillSuggestions);

public sealed record JobTitleSuggestion(string Title, int Score);

public sealed record RegenerateResponse(string UpdatedContent, bool AiAvailable);

// ── MockEngine ─────────────────────────────────────────────────────────────

/// <summary>
/// Template-based mock engine that simulates an AI resume expert without calling OpenAI.
/// Deterministic output is produced using a seed derived from the candidate's email address.
/// A 1.5-second artificial delay is applied to simulate realistic AI processing latency.
/// </summary>
public sealed class MockEngine
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ExperienceMockData _expMocks;
    private readonly SummaryMockData _sumMocks;
    private readonly SuggestionsMockData _sugMocks;

    public MockEngine(IWebHostEnvironment env)
    {
        var dir = Path.Combine(env.ContentRootPath, "Mocks");
        _expMocks = Load<ExperienceMockData>(Path.Combine(dir, "experience_mocks.json"));
        _sumMocks = Load<SummaryMockData>(Path.Combine(dir, "summary_mocks.json"));
        _sugMocks = Load<SuggestionsMockData>(Path.Combine(dir, "suggestions_mocks.json"));
    }

    private static T Load<T>(string path) =>
        JsonSerializer.Deserialize<T>(File.ReadAllText(path), JsonOpts)
        ?? throw new InvalidOperationException($"Failed to load mock data from: {path}");

    // ── Generate ───────────────────────────────────────────────────────────

    /// <summary>
    /// Transforms <paramref name="rawData"/> into a polished <c>finalData</c> payload.
    /// Settings are template-defined; the mock uses sensible defaults for AI content selection.
    /// </summary>
    public async Task<GenerateResponse> GenerateAsync(JsonNode rawData)
    {
        await Task.Delay(1500); // Simulate AI processing latency

        // Settings are template-defined — use professional defaults for all content selection.
        const string tone = "Professional";
        const string aiFocus = "Technical";
        const string descFormat = "Paragraph";
        const string summaryType = "Summary";
        const string narrativeVoice = "FirstPerson";

        // Content comes directly from rawData (no settings wrapper expected).
        var content = rawData["content"];

        // ── Deterministic seed from candidate email ────────────────────────
        var email = content?["personal"]?["email"]?.GetValue<string>() ?? string.Empty;
        var seed = Math.Abs(string.IsNullOrEmpty(email) ? 42 : email.GetHashCode());
        var rnd = new Random(seed);

        var firstName = content?["personal"]?["firstName"]?.GetValue<string>() ?? string.Empty;

        // ── Build finalData content ────────────────────────────────────────
        var personal = content?["personal"]?.DeepClone() ?? new JsonObject();
        var summary = SelectSummary(tone, summaryType, narrativeVoice, firstName, rnd);
        var experience = BuildExperience(content?["experience"], aiFocus, tone, descFormat, rnd);
        var education = content?["education"]?.DeepClone() ?? new JsonArray();
        var courses = content?["courses"]?.DeepClone() ?? new JsonArray();

        // Skills: preserve existing + enrich with suggestions
        var existingSkills = ExtractSkillNames(content?["skills"]);
        var skillSuggestions = SuggestSkills(existingSkills);
        var skills = BuildSkillsNode(content?["skills"], skillSuggestions.Take(5));

        // Optional sections: pass through only what the user filled in.
        // Never inject default content for sections left empty — that is the user's choice.
        var languages = IsNodeEmpty(content?["languages"]) ? null : content!["languages"]!.DeepClone();
        var projects = IsNodeEmpty(content?["projects"]) ? null : BuildProjectsWithFormattedDescriptions(content!["projects"]!, descFormat);
        var volunteers = IsNodeEmpty(content?["volunteers"]) ? null : content!["volunteers"]!.DeepClone();
        var hobbies = IsNodeEmpty(content?["hobbies"]) ? null : content!["hobbies"]!.DeepClone();
        var other = IsNodeEmpty(content?["other"]) ? null : content!["other"]!.DeepClone();

        // ── Suggestions ────────────────────────────────────────────────────
        var jobTitleSuggestions = SuggestJobTitles(existingSkills);

        // ── Assemble finalData (content-only; no settings block) ───────────
        var finalData = new JsonObject
        {
            ["content"] = new JsonObject
            {
                ["personal"] = personal,
                ["summary"] = summary,
                ["experience"] = experience,
                ["education"] = education,
                ["courses"] = courses,
                ["skills"] = skills,
                ["languages"] = languages,
                ["projects"] = projects,
                ["volunteers"] = volunteers,
                ["hobbies"] = hobbies,
                ["other"] = other
            }
        };

        return new GenerateResponse(
            FinalDataJson: finalData.ToJsonString(),
            AiAvailable: true,
            JobTitleSuggestions: jobTitleSuggestions,
            SkillSuggestions: skillSuggestions);
    }

    // ── Regenerate ─────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a mocked regenerated content string for a single resume section.
    /// Incorporates the user's prompt text so the frontend can verify the AI "read" it.
    /// Respects <c>targetFormat</c> (or <c>currentDescriptionFormat</c> as fallback) for bullet vs paragraph output.
    /// </summary>
    public async Task<RegenerateResponse> RegenerateAsync(JsonNode body)
    {
        await Task.Delay(1500);

        var section = body["sectionIdentifier"]?.GetValue<string>()?.ToLowerInvariant() ?? "section";
        var prompt = body["userPrompt"]?.GetValue<string>() ?? string.Empty;
        var targetFormat = body["targetFormat"]?.GetValue<string>();
        var currentFormat = body["currentDescriptionFormat"]?.GetValue<string>() ?? "Paragraph";
        var effectiveFormat = !string.IsNullOrWhiteSpace(targetFormat) ? targetFormat : currentFormat;
        var seed = Math.Abs(prompt.GetHashCode());

        // Select realistic content based on section type
        var rawContent = section switch
        {
            "summary" => Pick(_sumMocks.Summaries["professional"], seed),
            _ when section.StartsWith("exp_") => Pick(PickExperienceBank("Technical"), seed),
            "skills" => BuildRegeneratedSkillsText(seed),
            _ when section.StartsWith("edu_") => "Graduated with distinction. Final-year project delivered measurable research outcomes and was presented at a departmental symposium. Received academic excellence recognition from the faculty.",
            _ => "Spearheaded cross-functional initiatives that improved team efficiency by 30%. Collaborated with senior stakeholders to define objectives, managed delivery timelines, and ensured alignment with organizational goals throughout the project lifecycle."
        };

        // Apply bullet/paragraph formatting for text sections
        var isTextSection = section is "summary" || section.StartsWith("exp_") || section.StartsWith("edu_");
        var updatedContent = isTextSection ? FormatDescription(rawContent, effectiveFormat) : rawContent;

        return new RegenerateResponse(updatedContent, AiAvailable: true);
    }

    // ── Summary selection ──────────────────────────────────────────────────

    private string SelectSummary(string tone, string summaryType, string narrativeVoice, string firstName, Random rnd)
    {
        var toneKey = tone.ToLowerInvariant() switch
        {
            "executive" => "executive",
            "creative" => "creative",
            "academic" => "academic",
            _ => "professional"
        };

        var isObjective = summaryType.Equals("Objective", StringComparison.OrdinalIgnoreCase);
        var bank = isObjective
            ? (_sumMocks.Objectives.GetValueOrDefault(toneKey) ?? _sumMocks.Objectives["professional"])
            : (_sumMocks.Summaries.GetValueOrDefault(toneKey) ?? _sumMocks.Summaries["professional"]);

        var text = Pick(bank, rnd.Next());

        // Apply NarrativeVoice: ThirdPerson prepends the candidate's first name
        if (narrativeVoice.Equals("ThirdPerson", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(firstName))
        {
            text = $"{firstName} is a {char.ToLowerInvariant(text[0])}{text[1..]}";
        }

        return text;
    }

    // ── Experience building ────────────────────────────────────────────────

    private JsonArray BuildExperience(JsonNode? experienceNode, string aiFocus, string tone, string descFormat, Random rnd)
    {
        var result = new JsonArray();
        if (experienceNode is not JsonArray rawArr) return result;

        var bank = SelectExperienceBank(aiFocus, tone);

        for (var i = 0; i < rawArr.Count; i++)
        {
            var entry = rawArr[i]?.DeepClone() as JsonObject ?? new JsonObject();
            var pickSeed = rnd.Next() + i;
            var rawDesc = Pick(bank, pickSeed);
            entry["description"] = FormatDescription(rawDesc, descFormat);
            result.Add(entry);
        }

        return result;
    }

    private List<string> SelectExperienceBank(string aiFocus, string tone)
    {
        // MetricsDriven always wins; then Leadership if Executive tone; else Technical
        if (aiFocus.Equals("MetricsDriven", StringComparison.OrdinalIgnoreCase))
            return _expMocks.MetricsDriven;

        if (aiFocus.Equals("Leadership", StringComparison.OrdinalIgnoreCase)
            || tone.Equals("Executive", StringComparison.OrdinalIgnoreCase))
            return _expMocks.Leadership;

        return _expMocks.Technical;
    }

    private List<string> PickExperienceBank(string aiFocus) =>
        aiFocus.Equals("MetricsDriven", StringComparison.OrdinalIgnoreCase) ? _expMocks.MetricsDriven
        : aiFocus.Equals("Leadership", StringComparison.OrdinalIgnoreCase) ? _expMocks.Leadership
        : _expMocks.Technical;

    // ── Projects ───────────────────────────────────────────────────────────

    private JsonArray BuildDefaultProjects(string descFormat, Random rnd)
    {
        var result = new JsonArray();
        foreach (var p in _sugMocks.DefaultProjects)
        {
            result.Add(new JsonObject
            {
                ["id"] = p.Id,
                ["name"] = p.Name,
                ["role"] = p.Role,
                ["description"] = FormatDescription(p.Description, descFormat),
                ["link"] = p.Link ?? string.Empty,
                ["technologies"] = JsonNode.Parse(JsonSerializer.Serialize(p.Technologies))
            });
        }
        return result;
    }

    private JsonArray BuildProjectsWithFormattedDescriptions(JsonNode projectsNode, string descFormat)
    {
        if (projectsNode is not JsonArray arr) return new JsonArray();
        var result = new JsonArray();
        foreach (var item in arr)
        {
            var proj = item?.DeepClone() as JsonObject ?? new JsonObject();
            if (proj["description"]?.GetValue<string>() is string desc)
                proj["description"] = FormatDescription(desc, descFormat);
            result.Add(proj);
        }
        return result;
    }

    // ── Default section builders ───────────────────────────────────────────

    private JsonArray BuildDefaultLanguages()
    {
        var result = new JsonArray();
        foreach (var l in _sugMocks.DefaultLanguages)
            result.Add(new JsonObject { ["language"] = l.Language, ["level"] = l.Level });
        return result;
    }

    private JsonArray BuildDefaultVolunteers(string descFormat)
    {
        var result = new JsonArray();
        foreach (var v in _sugMocks.DefaultVolunteers)
        {
            result.Add(new JsonObject
            {
                ["id"] = v.Id,
                ["organization"] = v.Organization,
                ["role"] = v.Role,
                ["startDate"] = v.StartDate,
                ["endDate"] = v.EndDate,
                ["description"] = FormatDescription(v.Description, descFormat)
            });
        }
        return result;
    }

    private JsonArray BuildDefaultHobbies()
    {
        var result = new JsonArray();
        foreach (var h in _sugMocks.DefaultHobbies.Take(5))
            result.Add(JsonValue.Create(h));
        return result;
    }

    private JsonArray BuildDefaultOther()
    {
        var result = new JsonArray();
        foreach (var o in _sugMocks.DefaultOther)
            result.Add(new JsonObject { ["label"] = o.Label, ["value"] = o.Value });
        return result;
    }

    // ── Skills ─────────────────────────────────────────────────────────────

    private static JsonArray BuildSkillsNode(JsonNode? rawSkills, IEnumerable<string> suggestions)
    {
        var result = new JsonArray();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Preserve original skill objects
        if (rawSkills is JsonArray arr)
        {
            foreach (var item in arr)
            {
                var name = item is JsonValue v
                    ? v.GetValue<string>()
                    : item?["name"]?.GetValue<string>();

                if (!string.IsNullOrWhiteSpace(name) && seen.Add(name))
                    result.Add(item?.DeepClone() ?? new JsonObject { ["name"] = name });
            }
        }

        // Inject AI-suggested skills as { "name": "..." } objects
        foreach (var s in suggestions)
        {
            if (seen.Add(s))
                result.Add(new JsonObject { ["name"] = s });
        }

        return result;
    }

    private string BuildRegeneratedSkillsText(int seed)
    {
        var pool = _sugMocks.MasterSkillPool;
        var count = Math.Min(10, pool.Count);
        var picked = Enumerable.Range(0, count)
            .Select(i => pool[(seed + i * 7) % pool.Count])
            .Distinct()
            .Take(10);
        return string.Join(", ", picked);
    }

    // ── Suggestions ────────────────────────────────────────────────────────

    private List<JobTitleSuggestion> SuggestJobTitles(HashSet<string> skills)
    {
        return [.. _sugMocks.JobTitles
            .Select(jt => new JobTitleSuggestion(
                jt.Title,
                Math.Min(10, 5 + jt.BoostSkills.Count(s => skills.Contains(s)))))
            .OrderByDescending(x => x.Score)
            .Take(10)];
    }

    private List<string> SuggestSkills(HashSet<string> existing)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<string>();

        // Cluster-based suggestions first
        foreach (var cluster in _sugMocks.SkillClusters)
        {
            if (!cluster.Triggers.Any(t => existing.Contains(t))) continue;
            foreach (var candidate in cluster.Suggestions)
            {
                if (!existing.Contains(candidate) && seen.Add(candidate))
                    result.Add(candidate);
            }
            if (result.Count >= 10) break;
        }

        // Fill remaining from master pool
        foreach (var skill in _sugMocks.MasterSkillPool)
        {
            if (result.Count >= 10) break;
            if (!existing.Contains(skill) && seen.Add(skill))
                result.Add(skill);
        }

        return result.Take(10).ToList();
    }

    // ── Utility helpers ────────────────────────────────────────────────────

    private static HashSet<string> ExtractSkillNames(JsonNode? skillsNode)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (skillsNode is not JsonArray arr) return result;

        foreach (var item in arr)
        {
            var name = item is JsonValue v
                ? v.GetValue<string>()
                : item?["name"]?.GetValue<string>();

            if (!string.IsNullOrWhiteSpace(name))
                result.Add(name);
        }

        return result;
    }

    /// <summary>Formats a paragraph description as bullet points when <paramref name="format"/> is Bulleted.</summary>
    private static string FormatDescription(string text, string format)
    {
        var isBulleted = format.Equals("Bulleted", StringComparison.OrdinalIgnoreCase)
                      || format.Equals("BULLET", StringComparison.OrdinalIgnoreCase)
                      || format.Equals("Bullet", StringComparison.OrdinalIgnoreCase);

        if (!isBulleted) return text;

        var bullets = text
            .Split(". ", StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim().TrimEnd('.'))
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => $"• {s}.")
            .ToList();

        return bullets.Count > 0 ? string.Join("\n", bullets) : text;
    }

    private static bool IsNodeEmpty(JsonNode? node) =>
        node is null || (node is JsonArray arr && arr.Count == 0);

    private static T Pick<T>(IReadOnlyList<T> list, int seed) =>
        list[Math.Abs(seed) % list.Count];
}
