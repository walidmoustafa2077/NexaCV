using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NexaCV.Api.Services;

/// <summary>
/// Replaces {{TOKEN}} placeholders and <!--REPEAT:SECTION-->…<!--/REPEAT:SECTION--> blocks
/// in an HTML template string with data from the resume's FinalData JSON.
///
/// Supported scalar tokens:
///   {{FULL_NAME}}, {{FIRST_NAME}}, {{LAST_NAME}}, {{JOB_TITLE}},
///   {{EMAIL}}, {{PHONE}}, {{LOCATION}}, {{LINKEDIN}}, {{WEBSITE}},
///   {{SUMMARY}}, {{SKILLS_LIST}}, {{CURRENT_YEAR}}
///
/// Supported repeat sections (inner tokens listed per section):
///   EXPERIENCE  → {{EXP_TITLE}} {{EXP_COMPANY}} {{EXP_START}} {{EXP_END}} {{EXP_PERIOD}} {{EXP_DESCRIPTION}}
///   EDUCATION   → {{EDU_INSTITUTION}} {{EDU_DEGREE}} {{EDU_FIELD}} {{EDU_GRADE}} {{EDU_START}} {{EDU_END}} {{EDU_PERIOD}}
///   SKILLS      → {{SKILL}}
///   COURSES     → {{COURSE_NAME}} {{COURSE_PROVIDER}} {{COURSE_DATE}}
///   LANGUAGES   → {{LANG_NAME}} {{LANG_LEVEL}}
///   PROJECTS    → {{PROJ_NAME}} {{PROJ_DESCRIPTION}} {{PROJ_PERIOD}} {{PROJ_LINK}} (supports <!--IF:PROJ_LINK-->)
/// </summary>
public class TemplateRendererService : ITemplateRendererService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public string Render(string htmlTemplate, string finalDataJson)
    {
        JsonElement root;
        try { root = JsonSerializer.Deserialize<JsonElement>(finalDataJson, JsonOpts); }
        catch { return htmlTemplate; }

        var content = root.TryGetProperty("content", out var c) ? c : default;

        // ── Scalar tokens ───────────────────────────────────────────────────
        var personal = content.ValueKind == JsonValueKind.Object && content.TryGetProperty("personal", out var p) ? p : default;
        var firstName = Str(personal, "firstName");
        var lastName = Str(personal, "lastName");
        var fullName = $"{firstName} {lastName}".Trim();

        var scalars = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["FULL_NAME"] = fullName,
            ["FIRST_NAME"] = firstName,
            ["LAST_NAME"] = lastName,
            ["JOB_TITLE"] = Str(personal, "jobTitle"),
            ["EMAIL"] = Str(personal, "email"),
            ["PHONE"] = Str(personal, "phone"),
            ["LOCATION"] = Str(personal, "location"),
            ["LINKEDIN"] = Str(personal, "linkedinUrl"),
            ["WEBSITE"] = Str(personal, "siteUrl"),
            ["PHOTO_URL"] = Str(personal, "photoUrl"),
            ["SUMMARY"] = FormatDescription(Str(content, "summary")),
            ["CURRENT_YEAR"] = DateTime.UtcNow.Year.ToString(),
            ["SKILLS_LIST"] = BuildSkillsList(content),
        };

        var html = htmlTemplate;
        foreach (var (token, value) in scalars)
            html = html.Replace($"{{{{{token}}}}}", value, StringComparison.OrdinalIgnoreCase);

        // ── Conditional blocks ──────────────────────────────────────────────
        html = ProcessConditionals(html, scalars);

        // ── Repeat blocks ───────────────────────────────────────────────────
        html = ProcessRepeat(html, "EXPERIENCE", content, RenderExperience);
        html = ProcessRepeat(html, "EDUCATION", content, RenderEducation);
        html = ProcessRepeat(html, "SKILLS", content, RenderSkill);
        html = ProcessRepeat(html, "COURSES", content, RenderCourse);
        html = ProcessRepeat(html, "LANGUAGES", content, RenderLanguage);
        html = ProcessRepeat(html, "PROJECTS", content, RenderProject);

        return html;
    }

    // ── Repeat block engine ─────────────────────────────────────────────────
    private static string ProcessRepeat(
        string html,
        string sectionName,
        JsonElement content,
        Func<JsonElement, string, string> itemRenderer)
    {
        var open = $"<!--REPEAT:{sectionName}-->";
        var close = $"<!--/REPEAT:{sectionName}-->";

        int start = html.IndexOf(open, StringComparison.OrdinalIgnoreCase);
        if (start == -1) return html;

        int innerStart = start + open.Length;
        int innerEnd = html.IndexOf(close, innerStart, StringComparison.OrdinalIgnoreCase);
        if (innerEnd == -1) return html;

        var itemTemplate = html[innerStart..innerEnd];
        var sb = new StringBuilder();

        var arrayKey = sectionName.ToLowerInvariant();
        if (content.ValueKind == JsonValueKind.Object
            && content.TryGetProperty(arrayKey, out var array)
            && array.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in array.EnumerateArray())
                sb.Append(itemRenderer(item, itemTemplate));
        }

        int afterClose = innerEnd + close.Length;

        // When no items were produced, remove the entire containing section block.
        if (sb.Length == 0)
        {
            // 1. Try explicit <!--SECTION:NAME-->…<!--/SECTION:NAME--> markers (most reliable)
            var sectOpenMarker  = $"<!--SECTION:{sectionName}-->";
            var sectCloseMarker = $"<!--/SECTION:{sectionName}-->";
            int so = html.LastIndexOf(sectOpenMarker, start, StringComparison.OrdinalIgnoreCase);
            if (so >= 0)
            {
                int sc = html.IndexOf(sectCloseMarker, afterClose, StringComparison.OrdinalIgnoreCase);
                if (sc >= 0)
                    return html[..so] + html[(sc + sectCloseMarker.Length)..];
            }

            // 2. Fallback: find the nearest enclosing <section> element
            int sectionOpen = html.LastIndexOf("<section", start, StringComparison.OrdinalIgnoreCase);
            if (sectionOpen >= 0)
            {
                int sectionClose = html.IndexOf("</section>", afterClose, StringComparison.OrdinalIgnoreCase);
                if (sectionClose >= 0)
                    return html[..sectionOpen] + html[(sectionClose + "</section>".Length)..];
            }
        }

        return html[..start] + sb + html[afterClose..];
    }

    // ── Conditional block engine ────────────────────────────────────────────
    /// <summary>
    /// Processes &lt;!--IF:TOKEN--&gt;…&lt;!--/IF:TOKEN--&gt; blocks.
    /// Removes the block when the token's value is empty; keeps the inner content when non-empty.
    /// </summary>
    private static string ProcessConditionals(string html, Dictionary<string, string> scalars)
    {
        foreach (var (token, value) in scalars)
        {
            var open  = $"<!--IF:{token}-->";
            var close = $"<!--/IF:{token}-->";
            int idx;
            while ((idx = html.IndexOf(open, StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                int closeIdx = html.IndexOf(close, idx + open.Length, StringComparison.OrdinalIgnoreCase);
                if (closeIdx < 0) break;
                int afterCloseIdx = closeIdx + close.Length;
                html = string.IsNullOrWhiteSpace(value)
                    ? html[..idx] + html[afterCloseIdx..]                                                // remove block
                    : html[..idx] + html[(idx + open.Length)..closeIdx] + html[afterCloseIdx..];        // keep content
            }
        }
        return html;
    }

    // ── Item renderers ──────────────────────────────────────────────────────
    private static string RenderExperience(JsonElement exp, string tpl)
    {
        var start = Str(exp, "startDate");
        var end = Str(exp, "endDate");
        var period = BuildPeriod(start, end);

        return tpl
            .Replace("{{EXP_TITLE}}", H(Str(exp, "title")))
            .Replace("{{EXP_COMPANY}}", H(Str(exp, "company")))
            .Replace("{{EXP_START}}", H(FormatDate(start)))
            .Replace("{{EXP_END}}", H(FormatDate(end, "Present")))
            .Replace("{{EXP_PERIOD}}", H(period))
            .Replace("{{EXP_DESCRIPTION}}", FormatDescription(Str(exp, "description")));
    }

    private static string RenderEducation(JsonElement edu, string tpl)
    {
        var start = Str(edu, "startDate");
        var end = Str(edu, "endDate");
        var period = BuildPeriod(start, end);

        return tpl
            .Replace("{{EDU_INSTITUTION}}", H(Str(edu, "institution")))
            .Replace("{{EDU_DEGREE}}", H(Str(edu, "degree")))
            .Replace("{{EDU_FIELD}}", H(Str(edu, "fieldOfStudy")))
            .Replace("{{EDU_GRADE}}", H(Str(edu, "grade")))
            .Replace("{{EDU_START}}", H(FormatDate(start)))
            .Replace("{{EDU_END}}", H(FormatDate(end)))
            .Replace("{{EDU_PERIOD}}", H(period));
    }

    private static string RenderSkill(JsonElement skill, string tpl)
        => tpl.Replace("{{SKILL}}", H(skill.GetString() ?? string.Empty));

    private static string RenderCourse(JsonElement course, string tpl)
        => tpl
            .Replace("{{COURSE_NAME}}", H(Str(course, "name")))
            .Replace("{{COURSE_PROVIDER}}", H(Str(course, "provider")))
            .Replace("{{COURSE_DATE}}", H(FormatDate(Str(course, "date"))));

    private static string RenderLanguage(JsonElement lang, string tpl)
        => tpl
            .Replace("{{LANG_NAME}}", H(Str(lang, "name")))
            .Replace("{{LANG_LEVEL}}", H(Str(lang, "level")));

    private static string RenderProject(JsonElement proj, string tpl)
    {
        var start = Str(proj, "startDate");
        var end = Str(proj, "endDate");
        var period = BuildPeriod(start, end);
        var link = Str(proj, "link");

        tpl = ApplyItemConditional(tpl, "PROJ_LINK", link);

        return tpl
            .Replace("{{PROJ_NAME}}", H(Str(proj, "name")))
            .Replace("{{PROJ_DESCRIPTION}}", FormatDescription(Str(proj, "description")))
            .Replace("{{PROJ_PERIOD}}", H(period))
            .Replace("{{PROJ_LINK}}", H(link));
    }

    /// <summary>Processes a single <!--IF:TOKEN-->…<!--/IF:TOKEN--> block inside an item template.</summary>
    private static string ApplyItemConditional(string tpl, string token, string value)
    {
        var open = $"<!--IF:{token}-->";
        var close = $"<!--/IF:{token}-->";
        int idx;
        while ((idx = tpl.IndexOf(open, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            int closeIdx = tpl.IndexOf(close, idx + open.Length, StringComparison.OrdinalIgnoreCase);
            if (closeIdx < 0) break;
            int afterClose = closeIdx + close.Length;
            tpl = string.IsNullOrWhiteSpace(value)
                ? tpl[..idx] + tpl[afterClose..]
                : tpl[..idx] + tpl[(idx + open.Length)..closeIdx] + tpl[afterClose..];
        }
        return tpl;
    }

    // ── Helpers ─────────────────────────────────────────────────────────────
    private static string Str(JsonElement el, string key)
    {
        if (el.ValueKind != JsonValueKind.Object) return string.Empty;
        return el.TryGetProperty(key, out var v) ? v.GetString() ?? string.Empty : string.Empty;
    }

    /// <summary>HTML-encodes a plain-text value to prevent XSS in rendered HTML.</summary>
    private static string H(string value) => System.Web.HttpUtility.HtmlEncode(value);

    /// <summary>
    /// Converts description text with bullet markers ("• line\n• line") into an HTML unordered list.
    /// Falls back to &lt;p&gt; when no bullets are detected.
    /// </summary>
    private static string FormatDescription(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        bool hasBullets = lines.Any(l => l.TrimStart().StartsWith("•") || l.TrimStart().StartsWith("-"));

        if (hasBullets)
        {
            var sb = new StringBuilder("<ul style=\"margin:0;padding-left:16px;\">");
            foreach (var line in lines)
            {
                var clean = Regex.Replace(line.TrimStart(), @"^[•\-]\s*", string.Empty);
                if (!string.IsNullOrWhiteSpace(clean))
                    sb.Append($"<li>{H(clean)}</li>");
            }
            sb.Append("</ul>");
            return sb.ToString();
        }

        return $"<p style=\"margin:0;\">{H(text.Replace("\n", "<br>"))}</p>";
    }

    private static string BuildSkillsList(JsonElement content)
    {
        if (content.ValueKind != JsonValueKind.Object) return string.Empty;
        if (!content.TryGetProperty("skills", out var arr) || arr.ValueKind != JsonValueKind.Array)
            return string.Empty;
        return string.Join(", ", arr.EnumerateArray().Select(s => s.GetString()).Where(s => s != null));
    }

    private static string FormatDate(string? raw, string fallback = "")
    {
        if (string.IsNullOrWhiteSpace(raw)) return fallback;
        if (DateTime.TryParse(raw, out var dt))
            return dt.ToString("MMM yyyy");
        return raw;
    }

    private static string BuildPeriod(string start, string end)
    {
        var s = FormatDate(start);
        var e = FormatDate(end, "Present");
        if (string.IsNullOrEmpty(s)) return e;
        if (string.IsNullOrEmpty(e)) return s;
        return $"{s} – {e}";
    }
}
