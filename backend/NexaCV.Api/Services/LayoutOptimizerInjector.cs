namespace NexaCV.Api.Services;

/// <summary>
/// Injects the Layout Optimizer CSS hooks and JavaScript engine into a rendered
/// HTML resume document before it is sent to the client.
///
/// The CSS block contains ONLY print-safe rules (page-break-inside: avoid).
/// No !important overrides, no :root variable pre-declarations.
/// The JS engine measures the template's pure CSS height first, then sets
/// CSS custom properties (--lopt-*) on :root only when adjustment is needed.
/// </summary>
internal static class LayoutOptimizerInjector
{
    // ── CSS: empty — no rules injected so the template renders exactly as authored ─
    // Previously contained page-break-inside and @media print overrides.
    // Cleared to let the diagnostic script measure the pure template output.

    private const string CssBlock = "";

    // ── Lazy-load the JS engine from the Templates directory once ─────────────

    private static readonly Lazy<string> _scriptContent = new(LoadScript);

    private static string LoadScript()
    {
        // The JS file is copied to the output directory alongside the HTML templates.
        var path = Path.Combine(AppContext.BaseDirectory, "Templates", "resumeLayoutOptimizer.js");

        if (!File.Exists(path))
        {
            // Graceful degradation: if the file is missing in production, emit nothing.
            return string.Empty;
        }

        return File.ReadAllText(path);
    }

    // ── Public injection API ──────────────────────────────────────────────────

    /// <summary>
    /// Returns a new HTML string with the optimizer CSS injected before
    /// <c>&lt;/head&gt;</c> and the optimizer JS injected before <c>&lt;/body&gt;</c>.
    /// If either marker is absent the original HTML is returned unchanged.
    /// </summary>
    public static string InjectInto(string html)
    {
        // 1. Skip CSS injection — CssBlock is empty in diagnostic mode.
        //    No rules are added so the template renders exactly as authored.

        // 2. Build inline <script> from the cached JS file content
        var js = _scriptContent.Value;
        if (string.IsNullOrWhiteSpace(js)) return html;

        var scriptTag = $"\n<script>\n{js}\n</script>\n";

        // 3. Inject JS just before </body>
        const string bodyClose = "</body>";
        var bodyIdx = html.IndexOf(bodyClose, StringComparison.OrdinalIgnoreCase);
        if (bodyIdx < 0) return html;

        html = string.Concat(html.AsSpan(0, bodyIdx), scriptTag, html.AsSpan(bodyIdx));

        return html;
    }
}
