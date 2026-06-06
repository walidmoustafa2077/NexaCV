using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NexaCV.Api.Services;

/// <summary>
/// Renders an HTML resume template by replacing PascalCase {{Token}} placeholders and
/// expanding &lt;!-- START SECTION --&gt;…&lt;!-- END SECTION --&gt; loop blocks
/// with real data from the resume's finalData JSON.
///
/// Token reference (scalars):
///   {{FullName}}, {{FirstName}}, {{MiddleName}}, {{LastName}}, {{TargetTitle}},
///   {{Email}}, {{Phone}}, {{Location}}, {{LinkedIn}}, {{Website}}, {{GitHub}}, {{Initials}},
///   {{Summary}}, {{PhotoUrl}}
///
/// Loop sections (and inner tokens):
///   EXPERIENCE        → {{JobTitle}}, {{CompanyName}}, {{StartDate}}, {{EndDate}}
///     RESPONSIBILITIES  → {{Responsibility}}  (nested inside EXPERIENCE)
///   EDUCATION         → {{Degree}}, {{FieldOfStudy}}, {{Institution}}, {{EducationLocation}}, {{GradYear}}
///   ACHIEVEMENTS      → {{Achievement}}  (sourced from content.achievements[])
///   SKILLS            → {{SkillName}}, {{SkillLevel}}
///   SKILL_CATEGORIES  → {{CategoryName}}, {{CategorySkills}}
///     CATEGORY_SKILLS   → {{SkillName}}  (nested inside SKILL_CATEGORIES)
///   CERTIFICATIONS    → {{CertName}}, {{CertIssuer}}, {{CertYear}}  (sourced from courses[])
///   LANGUAGES         → {{Language}}, {{LanguageLevel}}
///   PROJECTS          → {{ProjectName}}, {{ProjectDate}}, {{ProjectTechStack}}
///     PROJECT_BULLETS   → {{ProjectBullet}}  (nested inside PROJECTS)
///   VOLUNTEERS        → {{VolunteerOrganization}}, {{VolunteerRole}}, {{VolunteerDate}}, {{VolunteerDescription}}
///   OTHER             → {{OtherLabel}}, {{OtherValue}}
///   INTERESTS         → {{Interest}}  (sourced from hobbies[])
/// </summary>
public class TemplateRendererService : ITemplateRendererService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public string Render(string htmlTemplate, string finalDataJson)
    {
        JsonElement root;
        try { root = JsonSerializer.Deserialize<JsonElement>(finalDataJson, JsonOpts); }
        catch { return htmlTemplate; }

        var content = TryGet(root, "content");
        var personal = TryGet(content, "personal");

        // ── Scalar tokens ─────────────────────────────────────────────────────
        var firstName = Str(personal, "firstName");
        var middleName = Str(personal, "middleName");
        var lastName = Str(personal, "lastName");
        var jobTitle = Str(personal, "jobTitle");
        if (string.IsNullOrEmpty(jobTitle)) jobTitle = Str(content, "targetJobTitle");

        var initials = (firstName.Length > 0 ? firstName[0].ToString() : "")
                     + (lastName.Length > 0 ? lastName[0].ToString() : "");
        var siteUrl = Str(personal, "siteUrl");

        // Build FullName with middle name if present
        var fullNameParts = new[] { firstName, middleName, lastName }
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();
        var fullName = string.Join(" ", fullNameParts);

        var scalars = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["FullName"] = H(fullName),
            ["FirstName"] = H(firstName),
            ["MiddleName"] = H(middleName),
            ["LastName"] = H(lastName),
            ["TargetTitle"] = H(jobTitle),
            ["Email"] = H(Str(personal, "email")),
            ["Phone"] = H(Str(personal, "phone")),
            ["Location"] = H(Str(personal, "location")),
            ["LinkedIn"] = H(Str(personal, "linkedinUrl")),
            ["Website"] = H(siteUrl),
            ["GitHub"] = H(siteUrl),
            ["Initials"] = H(initials),
            ["Summary"] = H(Str(content, "summary")),
            ["PhotoUrl"] = Str(personal, "photoUrl"),
        };

        var html = htmlTemplate;

        // ── Loop sections (processed before scalar substitution) ─────────────
        html = ProcessExperience(html, content);
        html = ProcessEducation(html, content);
        html = ProcessAchievements(html, content);
        html = ProcessSkillFlat(html, content);
        html = ProcessSkillCategories(html, content);
        html = ProcessCertifications(html, content);
        html = ProcessLanguages(html, content);
        html = ProcessProjects(html, content);
        html = ProcessVolunteers(html, content);
        html = ProcessOther(html, content);
        html = ProcessInterests(html, content);

        // ── Conditional blocks (remove if scalar value is empty) ────────────
        html = ProcessConditionalBlocks(html, scalars);

        // ── Scalar substitution ───────────────────────────────────────────────
        foreach (var kv in scalars)
            html = ReplaceToken(html, kv.Key, kv.Value);

        // ── Strip any remaining unreplaced tokens ─────────────────────────────
        html = Regex.Replace(html, @"\{\{\w+\}\}", string.Empty);

        // ── Remove heading-only sections left behind by empty loop sections ───
        // Matches <section> that contains only an <h2> (and whitespace) after
        // all loop blocks have been removed.
        html = Regex.Replace(html,
            @"<section>\s*<h2[^>]*>[^<]*</h2>\s*</section>",
            string.Empty,
            RegexOptions.Singleline);

        // ── Inject CSS variable system + JS layout optimizer ─────────────────
        html = LayoutOptimizerInjector.InjectInto(html);

        return html;
    }

    // ── Section processors ────────────────────────────────────────────────────

    private static string ProcessExperience(string html, JsonElement content)
    {
        return ExpandSection(html, "EXPERIENCE", content, "experience", (item, tpl) =>
        {
            var desc = Str(item, "description");
            var responsibilities = SplitBullets(desc);
            var inner = responsibilities.Count > 0
                ? ExpandSimpleList(tpl, "RESPONSIBILITIES", responsibilities, "Responsibility")
                : RemoveSection(tpl, "RESPONSIBILITIES");

            var company = Str(item, "company");
            var expLocation = Str(item, "location");
            var companyLocation = string.IsNullOrWhiteSpace(expLocation)
                ? company
                : $"{company} \u2014 {expLocation}";

            return ApplyTokens(inner, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["JobTitle"] = H(Str(item, "title")),
                ["CompanyName"] = H(company),
                ["CompanyLocation"] = H(companyLocation),
                ["StartDate"] = H(FormatDate(Str(item, "startDate"))),
                ["EndDate"] = H(FormatDate(Str(item, "endDate"), "Present")),
                ["Location"] = H(expLocation),
            });
        });
    }

    private static string ProcessEducation(string html, JsonElement content)
    {
        return ExpandSection(html, "EDUCATION", content, "education", (item, tpl) =>
        {
            var endDate = Str(item, "endDate");
            var gradYear = endDate.Length >= 4 ? endDate[..4] : endDate;

            return ApplyTokens(tpl, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Degree"] = H(Str(item, "degree")),
                ["FieldOfStudy"] = H(Str(item, "fieldOfStudy")),
                ["Institution"] = H(Str(item, "institution")),
                ["EducationLocation"] = H(Str(item, "location")),
                ["GradYear"] = H(gradYear),
                ["StartDate"] = H(FormatDate(Str(item, "startDate"))),
                ["EndDate"] = H(FormatDate(endDate)),
                ["Grade"] = H(Str(item, "grade")),
            });
        });
    }

    private static string ProcessAchievements(string html, JsonElement content)
    {
        var achievements = new List<string>();
        if (content.ValueKind == JsonValueKind.Object
            && content.TryGetProperty("achievements", out var arr)
            && arr.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in arr.EnumerateArray())
            {
                var val = item.ValueKind == JsonValueKind.String
                    ? item.GetString()
                    : Str(item, "text");
                if (!string.IsNullOrWhiteSpace(val)) achievements.Add(val!);
            }
        }

        return achievements.Count > 0
            ? ExpandSimpleList(html, "ACHIEVEMENTS", achievements, "Achievement")
            : RemoveSection(html, "ACHIEVEMENTS");
    }

    private static string ProcessSkillFlat(string html, JsonElement content)
    {
        return ExpandSection(html, "SKILLS", content, "skills", (item, tpl) =>
        {
            var name = item.ValueKind == JsonValueKind.String
                ? item.GetString() ?? string.Empty
                : Str(item, "name");

            return ApplyTokens(tpl, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["SkillName"] = H(name),
                ["SkillLevel"] = "80",
                ["SkillType"] = H(item.ValueKind == JsonValueKind.Object ? Str(item, "type") : string.Empty),
            });
        });
    }

    private static string ProcessSkillCategories(string html, JsonElement content)
    {
        const string SectionName = "SKILL_CATEGORIES";
        var (open, close) = GetMarkers(SectionName);
        int start = Find(html, open);
        if (start == -1) return html;

        // Build category → skills map
        var categories = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        if (content.ValueKind == JsonValueKind.Object
            && content.TryGetProperty("skills", out var skills)
            && skills.ValueKind == JsonValueKind.Array)
        {
            foreach (var skill in skills.EnumerateArray())
            {
                var name = skill.ValueKind == JsonValueKind.String
                    ? skill.GetString() ?? string.Empty
                    : Str(skill, "name");
                var cat = skill.ValueKind == JsonValueKind.Object ? Str(skill, "category") : string.Empty;
                if (string.IsNullOrWhiteSpace(cat)) cat = string.Empty;
                if (!categories.ContainsKey(cat)) categories[cat] = [];
                if (!string.IsNullOrWhiteSpace(name)) categories[cat].Add(name);
            }
        }

        if (categories.Count == 0) return RemoveSection(html, SectionName);

        int innerStart = start + open.Length;
        int innerEnd = Find(html, close, innerStart);
        if (innerEnd == -1) return html;

        var itemTemplate = html[innerStart..innerEnd];
        var sb = new StringBuilder();

        foreach (var (catName, catSkills) in categories)
        {
            var catHtml = ExpandSimpleList(itemTemplate, "CATEGORY_SKILLS", catSkills, "SkillName");
            sb.Append(ApplyTokens(catHtml, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["CategoryName"] = H(catName),
                ["CategorySkills"] = H(string.Join(", ", catSkills)),
            }));
        }

        int afterClose = innerEnd + close.Length;
        html = html[..start] + sb.ToString() + html[afterClose..];
        return ProcessSkipIfEmptyWrapper(html, SectionName, true);
    }

    private static string ProcessCertifications(string html, JsonElement content)
    {
        return ExpandSection(html, "CERTIFICATIONS", content, "courses", (item, tpl) =>
        {
            var date = Str(item, "date");
            var year = date.Length >= 4 ? date[..4] : date;

            return ApplyTokens(tpl, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["CertName"] = H(Str(item, "name")),
                ["CertIssuer"] = H(Str(item, "provider")),
                ["CertYear"] = H(year),
            });
        });
    }

    private static string ProcessLanguages(string html, JsonElement content)
    {
        return ExpandSection(html, "LANGUAGES", content, "languages", (item, tpl) =>
        {
            return ApplyTokens(tpl, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Language"] = H(Str(item, "language")),
                ["LanguageLevel"] = H(Str(item, "level")),
            });
        });
    }

    private static string ProcessProjects(string html, JsonElement content)
    {
        return ExpandSection(html, "PROJECTS", content, "projects", (item, tpl) =>
        {
            var desc = Str(item, "description");
            var bullets = SplitBullets(desc);
            var inner = bullets.Count > 0
                ? ExpandSimpleList(tpl, "PROJECT_BULLETS", bullets, "ProjectBullet")
                : RemoveSection(tpl, "PROJECT_BULLETS");

            var tech = BuildTechStack(item);

            return ApplyTokens(inner, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["ProjectName"] = H(Str(item, "name")),
                ["ProjectDate"] = string.Empty,
                ["ProjectTechStack"] = H(tech),
                ["ProjectLink"] = H(Str(item, "link")),
            });
        });
    }

    private static string ProcessInterests(string html, JsonElement content)
    {
        var hobbies = new List<string>();
        if (content.ValueKind == JsonValueKind.Object
            && content.TryGetProperty("hobbies", out var h)
            && h.ValueKind == JsonValueKind.Array)
        {
            foreach (var hobby in h.EnumerateArray())
            {
                if (hobby.ValueKind == JsonValueKind.String)
                {
                    var v = hobby.GetString();
                    if (!string.IsNullOrWhiteSpace(v)) hobbies.Add(v);
                }
            }
        }

        return hobbies.Count > 0
            ? ExpandSimpleList(html, "INTERESTS", hobbies, "Interest")
            : RemoveSection(html, "INTERESTS");
    }

    private static string ProcessVolunteers(string html, JsonElement content)
    {
        return ExpandSection(html, "VOLUNTEERS", content, "volunteers", (item, tpl) =>
        {
            var startDate = FormatDate(Str(item, "startDate"));
            var endDate = FormatDate(Str(item, "endDate"), "Present");
            var dateRange = string.IsNullOrWhiteSpace(startDate)
                ? string.Empty
                : $"{startDate} – {endDate}";

            return ApplyTokens(tpl, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["VolunteerOrganization"] = H(Str(item, "organization")),
                ["VolunteerRole"] = H(Str(item, "role")),
                ["VolunteerStartDate"] = H(startDate),
                ["VolunteerEndDate"] = H(endDate),
                ["VolunteerDate"] = H(dateRange),
                ["VolunteerDescription"] = H(Str(item, "description")),
            });
        });
    }

    private static string ProcessOther(string html, JsonElement content)
    {
        return ExpandSection(html, "OTHER", content, "other", (item, tpl) =>
        {
            return ApplyTokens(tpl, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["OtherLabel"] = H(Str(item, "label")),
                ["OtherValue"] = H(Str(item, "value")),
            });
        });
    }

    // ── Generic section helpers ───────────────────────────────────────────────

    /// <summary>Expands a &lt;!-- START X --&gt;…&lt;!-- END X --&gt; block over a JSON array.</summary>
    private static string ExpandSection(
        string html,
        string sectionName,
        JsonElement content,
        string jsonKey,
        Func<JsonElement, string, string> renderItem)
    {
        var (open, close) = GetMarkers(sectionName);
        int start = Find(html, open);
        if (start == -1) return html;
        int innerStart = start + open.Length;
        int innerEnd = Find(html, close, innerStart);
        if (innerEnd == -1) return html;

        var itemTemplate = html[innerStart..innerEnd];
        var sb = new StringBuilder();

        if (content.ValueKind == JsonValueKind.Object
            && content.TryGetProperty(jsonKey, out var array)
            && array.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in array.EnumerateArray())
                sb.Append(renderItem(item, itemTemplate));
        }

        int afterClose = innerEnd + close.Length;
        bool hadItems = sb.Length > 0;
        html = hadItems
            ? html[..start] + sb.ToString() + html[afterClose..]
            : RemoveSectionAt(html, start, afterClose);
        return ProcessSkipIfEmptyWrapper(html, sectionName, hadItems);
    }

    /// <summary>Expands a simple loop section over a flat list of strings.</summary>
    private static string ExpandSimpleList(
        string html,
        string sectionName,
        IList<string> items,
        string tokenName)
    {
        if (items.Count == 0)
            return ProcessSkipIfEmptyWrapper(RemoveSection(html, sectionName), sectionName, false);

        var (open, close) = GetMarkers(sectionName);
        int start = Find(html, open);
        if (start == -1) return html;
        int innerStart = start + open.Length;
        int innerEnd = Find(html, close, innerStart);
        if (innerEnd == -1) return html;

        var itemTemplate = html[innerStart..innerEnd];
        var sb = new StringBuilder();
        foreach (var item in items)
            sb.Append(ReplaceToken(itemTemplate, tokenName, H(item)));

        int afterClose = innerEnd + close.Length;
        html = html[..start] + sb.ToString() + html[afterClose..];
        return ProcessSkipIfEmptyWrapper(html, sectionName, true);
    }

    /// <summary>
    /// Processes &lt;!-- IF:TokenName --&gt;…&lt;!-- /IF:TokenName --&gt; blocks.
    /// Removes the entire block (including markers) when the scalar value is empty;
    /// strips only the markers when the value is non-empty.
    /// Must be called before scalar substitution.
    /// </summary>
    private static string ProcessConditionalBlocks(string html, Dictionary<string, string> scalars)
    {
        foreach (var kv in scalars)
        {
            var open = $"<!-- IF:{kv.Key} -->";
            var close = $"<!-- /IF:{kv.Key} -->";
            bool isEmpty = string.IsNullOrWhiteSpace(kv.Value);

            int start;
            while ((start = Find(html, open)) != -1)
            {
                int innerStart = start + open.Length;
                int innerEnd = Find(html, close, innerStart);
                if (innerEnd == -1) break;

                int afterClose = innerEnd + close.Length;
                if (isEmpty)
                {
                    html = html[..start] + html[afterClose..];
                }
                else
                {
                    var content = html[innerStart..innerEnd];
                    html = html[..start] + content + html[afterClose..];
                }
            }
        }
        return html;
    }

    /// <summary>Removes the entire &lt;!-- START X --&gt;…&lt;!-- END X --&gt; block including its markers.</summary>
    private static string RemoveSection(string html, string sectionName)
    {
        var (open, close) = GetMarkers(sectionName);
        int start = Find(html, open);
        if (start == -1) return html;
        int innerEnd = Find(html, close, start);
        if (innerEnd == -1) return html;
        html = RemoveSectionAt(html, start, innerEnd + close.Length);
        return ProcessSkipIfEmptyWrapper(html, sectionName, false);
    }

    /// <summary>
    /// Processes &lt;!-- SKIP_IF_EMPTY:SectionName --&gt;…&lt;!-- /SKIP_IF_EMPTY:SectionName --&gt; wrappers.
    /// Removes the entire wrapper block when the section had no items;
    /// strips only the markers when items were present.
    /// </summary>
    private static string ProcessSkipIfEmptyWrapper(string html, string sectionName, bool hadItems)
    {
        var open = $"<!-- SKIP_IF_EMPTY:{sectionName} -->";
        var close = $"<!-- /SKIP_IF_EMPTY:{sectionName} -->";
        int start = Find(html, open);
        if (start == -1) return html;
        int innerStart = start + open.Length;
        int innerEnd = Find(html, close, innerStart);
        if (innerEnd == -1) return html;
        int afterClose = innerEnd + close.Length;
        if (!hadItems)
            return html[..start] + html[afterClose..];
        return html[..start] + html[innerStart..innerEnd] + html[afterClose..];
    }

    private static string RemoveSectionAt(string html, int start, int end)
        => html[..start] + html[end..];

    // ── Low-level helpers ─────────────────────────────────────────────────────

    private static (string Open, string Close) GetMarkers(string sectionName)
        => ($"<!-- START {sectionName} -->", $"<!-- END {sectionName} -->");

    private static int Find(string haystack, string needle, int from = 0)
        => haystack.IndexOf(needle, from, StringComparison.OrdinalIgnoreCase);

    private static string ReplaceToken(string html, string token, string value)
        => html.Replace($"{{{{{token}}}}}", value, StringComparison.OrdinalIgnoreCase);

    private static string ApplyTokens(string html, Dictionary<string, string> tokens)
    {
        foreach (var kv in tokens)
            html = ReplaceToken(html, kv.Key, kv.Value);
        return html;
    }

    private static JsonElement TryGet(JsonElement el, string key)
        => el.ValueKind == JsonValueKind.Object && el.TryGetProperty(key, out var v) ? v : default;

    private static string Str(JsonElement el, string key)
    {
        if (el.ValueKind != JsonValueKind.Object) return string.Empty;
        if (!el.TryGetProperty(key, out var v)) return string.Empty;
        return v.ValueKind switch
        {
            JsonValueKind.String => v.GetString() ?? string.Empty,
            JsonValueKind.Null => string.Empty,
            _ => v.ToString(),
        };
    }

    private static string H(string value) => System.Web.HttpUtility.HtmlEncode(value);

    private static string FormatDate(string? raw, string fallback = "")
    {
        if (string.IsNullOrWhiteSpace(raw)) return fallback;
        if (raw.Length >= 7 && DateTime.TryParse(raw + "-01", out var dt))
            return dt.ToString("MMM yyyy");
        if (DateTime.TryParse(raw, out var dt2))
            return dt2.ToString("MMM yyyy");
        return raw;
    }

    private static List<string> SplitBullets(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];
        return text
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(l => Regex.Replace(l, @"^[-•*·]\s*", string.Empty))
            .Where(l => l.Length > 0)
            .ToList();
    }

    private static string BuildTechStack(JsonElement item)
    {
        if (item.ValueKind != JsonValueKind.Object) return string.Empty;
        if (!item.TryGetProperty("technologies", out var tech)
            || tech.ValueKind != JsonValueKind.Array) return string.Empty;
        return string.Join(" · ", tech.EnumerateArray()
            .Select(t => t.GetString() ?? string.Empty)
            .Where(t => t.Length > 0));
    }
}
