/**
 * Client-side template preview utilities.
 *
 * Substitutes the {{Placeholder}} tokens that all NexaCV HTML templates use
 * so a live preview can be rendered in a sandboxed iframe via `srcdoc`,
 * with no backend `/preview` call required.
 */

// ─── Sample / mock resume data ────────────────────────────────────────────────

const MOCK: Record<string, string> = {
    // Personal
    FullName: "Alexandra Harrison",
    FirstName: "Alexandra",
    LastName: "Harrison",
    TargetTitle: "Senior Product Manager",
    Email: "alex.harrison@email.com",
    Phone: "+1 (555) 234-5678",
    Location: "San Francisco, CA",
    LinkedIn: "linkedin.com/in/alexharrison",
    GitHub: "github.com/alexharrison",
    Website: "alexharrison.dev",
    Initials: "AH",

    // Summary
    Summary:
        "Accomplished product leader with 8+ years of experience driving cross-functional teams to deliver innovative, user-centric products. Proven track record in agile environments, data-driven decision-making, and scaling B2B SaaS platforms from concept to market.",

    // Experience (single-entry placeholders — loops are stripped to one pass)
    JobTitle: "Senior Product Manager",
    CompanyName: "Nexus Technologies",
    CompanyLocation: "Nexus Technologies \u2014 San Francisco, CA",
    StartDate: "Mar 2021",
    EndDate: "Present",
    Responsibility:
        "Led a 12-person cross-functional team to launch three major product lines, achieving 40% YoY revenue growth.",

    // Education
    Degree: "Bachelor of Science",
    FieldOfStudy: "Computer Science",
    Institution: "University of California, Berkeley",
    EducationLocation: "Berkeley, CA",
    GradYear: "2016",

    // Skills
    SkillName: "Product Strategy",
    SkillLevel: "85",
    CategoryName: "Technical Skills",
    CategorySkills: "SQL, Python, Figma, Tableau, JIRA",

    // Certifications
    CertName: "Certified Scrum Product Owner",
    CertIssuer: "Scrum Alliance",
    CertYear: "2020",

    // Languages
    Language: "English",
    LanguageLevel: "Native",

    // Projects
    ProjectName: "NexaInsight Dashboard",
    ProjectDate: "Jan 2023",
    ProjectTechStack: "React · Node.js · PostgreSQL",
    ProjectBullet: "Reduced average page load time by 60% through lazy loading and API caching.",

    // Achievements
    Achievement: "Grew ARR from $2M to $8M in 18 months by launching 3 new product tiers.",

    // Misc
    Interest: "Open Source",

    // Additional tokens for branded expert template
    PhotoUrl: "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='200' height='250' viewBox='0 0 200 250'%3E%3Crect width='200' height='250' fill='%23e2e2e9'/%3E%3Ccircle cx='100' cy='90' r='45' fill='%233a59a1'/%3E%3Cpath d='M30 250 Q100 180 170 250Z' fill='%233a59a1'/%3E%3C/svg%3E",
    VolunteerRole: "Tech Mentor",
    VolunteerDate: "2019 – Present",
    VolunteerOrganization: "CodeFirst Youth",
    VolunteerDescription: "Mentor aspiring software developers from underrepresented backgrounds through a 12-week full-stack bootcamp.",
    OtherLabel: "Volunteer",
    OtherValue: "Code mentor, hackathon judge",
};

// ─── Conditional & loop block handling ─────────────────────────────────

const CONDITIONAL_IF =
    /<!--\s*IF:\s*(\w+)\s*-->([\s\S]*?)<!--\s*\/\s*IF:\s*\1\s*-->/g;

const SKIP_IF_EMPTY_OPEN =
    /<!--\s*SKIP_IF_EMPTY:\s*(\w+)\s*-->/g;

const SKIP_IF_EMPTY_CLOSE =
    /<!--\s*\/\s*SKIP_IF_EMPTY:\s*\w+\s*-->/g;

/**
 * Collapses loops, IF conditionals, and SKIP_IF_EMPTY markers.
 *
 * - <!-- START Section --> ... <!-- END Section --> → content kept (single pass)
 * - <!-- IF:Token --> ... <!-- /IF:Token --> → content kept (all tokens present in mock)
 * - <!-- SKIP_IF_EMPTY:Token --> ... <!-- /SKIP_IF_EMPTY:Token --> → content kept
 */
function collapseLoops(html: string): string {
    // Step 1: Collapse START/END loops to one iteration
    let result = html.replace(
        /<!--\s*START\s+\S+\s*-->([\s\S]*?)<!--\s*END\s+\S+\s*-->/g,
        (_match, inner: string) => inner,
    );

    // Step 2: Remove IF markers, keep the content inside
    result = result.replace(CONDITIONAL_IF, (_match, _key: string, inner: string) => inner);

    // Step 3: Strip SKIP_IF_EMPTY open/close markers, keep content
    result = result.replace(SKIP_IF_EMPTY_OPEN, '');
    result = result.replace(SKIP_IF_EMPTY_CLOSE, '');

    return result;
}

// ─── Scalar substitution ──────────────────────────────────────────────────────

function substituteTokens(html: string, data: Record<string, string> = MOCK): string {
    // Replace every {{Token}} with the corresponding mock value (case-insensitive key match)
    return html.replace(/\{\{(\w+)\}\}/g, (_match, key: string) => {
        const value = data[key] ?? data[key.charAt(0).toUpperCase() + key.slice(1)];
        return value ?? "";
    });
}

// ─── Public API ───────────────────────────────────────────────────────────────

/**
 * Takes a raw template HTML string (with `{{Placeholder}}` tokens and
 * `<!-- START/END SECTION -->` loop markers) and returns a fully rendered
 * HTML string using the built-in mock resume data.
 */
export function renderTemplatePreview(rawHtml: string): string {
    const withLoops = collapseLoops(rawHtml);
    return substituteTokens(withLoops);
}
