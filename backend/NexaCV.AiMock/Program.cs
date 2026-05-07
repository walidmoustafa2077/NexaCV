using System.Text.Json;
using System.Text.Json.Nodes;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// ══════════════════════════════════════════════════════════════
// Data banks — declared first so helper closures can capture them
// ══════════════════════════════════════════════════════════════

// 50-skill master pool
string[] masterSkillPool =
[
    "C#", ".NET 9", "Java", "Python", "Go", "TypeScript", "JavaScript", "Rust", "Kotlin", "Ruby",
    "ASP.NET Core", "Minimal APIs", "FastAPI", "Django", "Spring Boot", "Node.js", "Express.js",
    "GraphQL", "REST API Design", "gRPC",
    "React", "Next.js", "Angular", "Vue.js", "Tailwind CSS", "Redux Toolkit", "React Query",
    "SQL Server", "PostgreSQL", "MySQL", "MongoDB", "Redis", "Elasticsearch", "Dapper",
    "Entity Framework Core",
    "Azure", "AWS", "GCP", "Docker", "Kubernetes", "Terraform", "Helm", "GitHub Actions",
    "Azure DevOps", "ArgoCD",
    "Microservices", "CQRS", "Domain-Driven Design", "Event Sourcing", "Clean Architecture",
    "Kafka", "RabbitMQ", "Azure Service Bus", "SignalR", "OpenTelemetry",
    "xUnit", "NUnit", "Jest", "k6", "FluentValidation", "MediatR",
];

// 5 polished professional summaries
string[] summaryMocks =
[
    "Results-driven Software Engineer with 6+ years designing and shipping high-availability .NET microservices on Azure. " +
    "Led cross-functional teams of up to 8 engineers, improved system uptime to 99.9%, and reduced average API latency by 35% through targeted optimisations. " +
    "Passionate about clean architecture, test-driven development, and delivering measurable business impact.",

    "Senior Backend Engineer specialising in distributed systems and cloud-native architecture. " +
    "Track record of scaling services to handle 10M+ daily requests, cutting infrastructure costs by 28% through right-sizing and intelligent caching, " +
    "and mentoring junior engineers to build resilient, maintainable codebases. " +
    "Committed to engineering excellence through rigorous code reviews and continuous delivery practices.",

    "Full-Stack Developer with 5 years of experience delivering responsive, accessible web applications using React and ASP.NET Core. " +
    "Shipped 12 production features end-to-end, improved Core Web Vitals scores by 40%, and drove adoption of component-design standards across three engineering teams. " +
    "Thrives in collaborative, fast-paced environments where product quality and developer experience are treated equally.",

    "Versatile Software Engineer with a strong foundation in object-oriented design, domain-driven development, and Agile delivery. " +
    "Contributed to three greenfield projects from architecture through go-live, consistently shipping on schedule while maintaining above-90% unit-test coverage. " +
    "Eager to apply expertise in .NET, SQL Server, and containerised infrastructure to solve complex, real-world engineering challenges.",

    "Experienced engineering leader with 8 years building and scaling software products across fintech and e-commerce domains. " +
    "Defined technical roadmaps adopted by four product squads, introduced trunk-based development that halved mean time to recovery, " +
    "and grew a high-performing team from 3 to 14 engineers. " +
    "Combines deep technical depth with strong stakeholder communication to align engineering effort with strategic business goals.",
];

// 15 distinct experience descriptions (paragraph form; bullets applied on-the-fly)
string[] experienceDescriptionMocks =
[
    "Architected and delivered a high-throughput order-processing microservice handling 50 000 requests per minute on Azure Service Bus. " +
    "Reduced end-to-end latency from 820 ms to 210 ms by introducing Redis caching and targeted query optimisation. " +
    "Mentored four junior engineers through weekly code reviews and pair-programming sessions.",

    "Designed RESTful APIs consumed by three client applications with a combined 200 000 monthly active users. " +
    "Migrated a legacy monolith to six independent ASP.NET Core services, cutting deployment lead time from three days to 45 minutes via GitHub Actions pipelines. " +
    "Achieved 99.95% uptime across all services over 18 months post-migration.",

    "Built and maintained a React and .NET 9 SaaS dashboard serving 1 200 enterprise clients worldwide. " +
    "Introduced component-level lazy loading that reduced the initial JavaScript bundle size by 42%. " +
    "Collaborated with the UX team to redesign the onboarding flow, increasing the 7-day activation rate from 54% to 73%.",

    "Led end-to-end delivery of a real-time notification platform processing 2 million events per day using Azure Event Hubs and SignalR. " +
    "Defined coding and testing standards adopted by three product teams. " +
    "Reduced the critical-bug escape rate by 45% by introducing mandatory integration-test gates in CI.",

    "Spearheaded migration of a monolithic e-commerce platform to a microservices architecture, enabling independent deployment of 12 bounded contexts. " +
    "Improved checkout conversion by 8% after optimising database query plans and introducing read replicas. " +
    "Reduced cloud spend by 31% through instance right-sizing and Spot VM adoption.",

    "Developed and owned a data-ingestion pipeline processing 500 GB of clickstream data daily using Apache Kafka and Azure Data Factory. " +
    "Reduced pipeline failure rate from 12% to under 0.5% by implementing idempotent consumers and dead-letter queuing. " +
    "Delivered weekly performance reports to stakeholders, accelerating data-driven product decisions.",

    "Implemented a CI/CD platform with GitHub Actions and ArgoCD, reducing average release cycle from two weeks to same-day deployments for eight engineering teams. " +
    "Introduced infrastructure-as-code with Terraform, eliminating manual provisioning errors and cutting environment setup time from four hours to 12 minutes. " +
    "Presented tooling improvements at company-wide engineering all-hands.",

    "Owned the full lifecycle of a payment-gateway integration supporting four currencies and three payment providers, processing over 2 M USD in monthly transactions. " +
    "Ensured PCI-DSS compliance by implementing end-to-end encryption and detailed audit logging. " +
    "Cut payment failure rate from 3.2% to 0.8% through robust retry logic and provider fallback routing.",

    "Designed and implemented a GraphQL API layer that unified five disparate REST services into a single developer-friendly schema. " +
    "Reduced average client-side data-fetching round trips by 60%, resulting in a 25% improvement in perceived page load speed. " +
    "Wrote comprehensive schema documentation that cut new-team onboarding time from three days to half a day.",

    "Rebuilt the reporting engine using CQRS and event sourcing, enabling point-in-time state reconstruction and audit trails required by regulatory compliance. " +
    "Improved report generation time from 45 seconds to under 3 seconds for the 95th-percentile query. " +
    "Collaborated with legal and compliance teams to enforce GDPR data-retention policies at the data-model level.",

    "Led a team of five engineers to deliver a mobile-first Progressive Web App that replaced native iOS and Android apps, reducing maintenance overhead by 60%. " +
    "Achieved a Lighthouse performance score of 97 and a first contentful paint under 1.2 seconds on 4G connections. " +
    "Coordinated release rollout across 30 regional markets with zero critical incidents.",

    "Introduced automated load and chaos testing using k6, uncovering and resolving seven latent reliability issues before they reached production. " +
    "Established SLO dashboards in Grafana, giving the engineering team real-time visibility into error budgets. " +
    "Reduced mean time to detect outages from 18 minutes to under 2 minutes.",

    "Delivered a multi-tenant identity platform using ASP.NET Core Identity and OpenID Connect, supporting SSO for 40 enterprise clients. " +
    "Implemented fine-grained RBAC that reduced unauthorised access incidents to zero over a 24-month period. " +
    "Authored integration guides adopted by six partner companies during onboarding.",

    "Optimised a high-traffic SQL Server database serving 5 000 concurrent users by redesigning indexes, partitioning large tables, and eliminating N+1 query patterns. " +
    "Reduced average query execution time by 68% and eliminated deadlock incidents that had previously caused weekly escalations. " +
    "Produced a query-optimisation runbook used as the database chapter of the team engineering handbook.",

    "Built an internal developer platform with a self-service portal letting product engineers provision fully configured cloud environments in under five minutes. " +
    "Reduced infra-request ticket volume by 80%, freeing the platform team to focus on reliability improvements. " +
    "Presented the platform at an internal tech summit, receiving the Engineering Excellence award.",
];

string[] educationDescriptionMocks =
[
    "Graduated with honours. Final-year project: distributed key-value store achieving 120 000 ops/sec on commodity hardware.",
    "Dean's List all four years. Capstone: real-time collaborative document editor built with SignalR and React.",
    "Completed specialisation in Software Architecture and Cloud Computing. Academic excellence award recipient.",
];

string[] experienceMocks =
[
    "[{\"id\":\"exp_001\",\"title\":\"Senior Software Engineer\",\"company\":\"TechFlow Systems\"," +
    "\"startDate\":\"2021-03\",\"current\":true," +
    "\"description\":\"Architected and delivered a high-throughput order-processing microservice handling 50 000 requests/min on Azure Service Bus. " +
    "Reduced end-to-end latency from 820 ms to 210 ms by introducing Redis caching and query optimisation. " +
    "Mentored four junior engineers through weekly code reviews and pair-programming sessions.\"}]",

    "[{\"id\":\"exp_001\",\"title\":\"Backend Developer\",\"company\":\"Nexora Ltd\"," +
    "\"startDate\":\"2020-06\",\"current\":true," +
    "\"description\":\"Designed RESTful APIs consumed by three client applications and a mobile app with a combined 200 000 MAU. " +
    "Migrated legacy monolith to six independent ASP.NET Core services, cutting deployment lead time from 3 days to 45 minutes via GitHub Actions pipelines. " +
    "Achieved 99.95% uptime over 18 months post-migration.\"}]",

    "[{\"id\":\"exp_001\",\"title\":\"Full-Stack Engineer\",\"company\":\"BrightLayer\"," +
    "\"startDate\":\"2019-09\",\"current\":true," +
    "\"description\":\"Built and maintained a React and .NET 9 SaaS dashboard used by 1 200 enterprise clients. " +
    "Introduced component-level lazy loading that cut initial bundle size by 42%. " +
    "Collaborated with UX to redesign the onboarding flow, increasing 7-day activation rate from 54% to 73%.\"}]",
];

string[] skillsMocks =
[
    "[\"C#\",\".NET 9\",\"ASP.NET Core\",\"Entity Framework Core\",\"MediatR\",\"CQRS\",\"SQL Server\",\"Azure\",\"Docker\",\"Kubernetes\",\"React\",\"TypeScript\",\"GitHub Actions\",\"Redis\",\"xUnit\"]",
    "[\"Python\",\"FastAPI\",\"PostgreSQL\",\"Redis\",\"Elasticsearch\",\"AWS\",\"Docker\",\"Terraform\",\"Kafka\",\"React\",\"GraphQL\",\"Jest\",\"OpenTelemetry\",\"Domain-Driven Design\",\"Clean Architecture\"]",
    "[\"Java\",\"Spring Boot\",\"Hibernate\",\"MySQL\",\"Kafka\",\"Kubernetes\",\"Angular\",\"TypeScript\",\"gRPC\",\"RabbitMQ\",\"GCP\",\"ArgoCD\",\"OpenTelemetry\",\"Clean Architecture\",\"Event Sourcing\"]",
    "[\"Go\",\"Rust\",\"gRPC\",\"PostgreSQL\",\"Redis\",\"Kafka\",\"Docker\",\"Kubernetes\",\"Terraform\",\"GCP\",\"OpenTelemetry\",\"GitHub Actions\",\"Clean Architecture\",\"Microservices\",\"Azure Service Bus\"]",
    "[\"TypeScript\",\"Next.js\",\"React\",\"Tailwind CSS\",\"Redux Toolkit\",\"React Query\",\"Node.js\",\"GraphQL\",\"PostgreSQL\",\"Docker\",\"Azure\",\"Jest\",\"Vite\",\"REST API Design\",\"SignalR\"]",
];

string[] educationMocks =
[
    "[{\"id\":\"edu_001\",\"degree\":\"Bachelor of Science in Computer Science\"," +
    "\"institution\":\"Cairo University\",\"startDate\":\"2015-09\",\"endDate\":\"2019-06\"," +
    "\"grade\":\"Very Good\",\"description\":\"Graduated with honours. Final-year project: distributed key-value store achieving 120 000 ops/sec on commodity hardware.\"}]",

    "[{\"id\":\"edu_001\",\"degree\":\"Bachelor of Engineering in Software Engineering\"," +
    "\"institution\":\"Alexandria University\",\"startDate\":\"2016-09\",\"endDate\":\"2020-06\"," +
    "\"grade\":\"Excellent\",\"description\":\"Dean's List all four years. Capstone: real-time collaborative document editor built with SignalR and React.\"}]",
];

var titleBoostMap = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
{
    ["Software Engineer"] = ["C#", "Java", "Python", "Go", "TypeScript", ".NET 9"],
    ["Senior Software Engineer"] = ["C#", "Java", "Python", "Microservices", "Domain-Driven Design", ".NET 9"],
    ["Backend Developer"] = ["C#", ".NET 9", "ASP.NET Core", "Java", "Node.js", "PostgreSQL"],
    ["Senior Backend Developer"] = ["C#", ".NET 9", "ASP.NET Core", "Redis", "Kafka", "Microservices"],
    ["Full Stack Developer"] = ["React", "Angular", "Vue.js", "TypeScript", "Node.js", "C#"],
    ["Senior Full Stack Developer"] = ["React", "TypeScript", "Node.js", "C#", "GraphQL", "Next.js"],
    ["Frontend Developer"] = ["React", "Angular", "Vue.js", "TypeScript", "JavaScript", "Next.js"],
    ["Cloud Engineer"] = ["Azure", "AWS", "GCP", "Terraform", "Kubernetes", "Docker"],
    ["DevOps Engineer"] = ["Docker", "Kubernetes", "GitHub Actions", "Azure DevOps", "Terraform", "Helm"],
    ["Solutions Architect"] = ["Azure", "AWS", "Microservices", "Docker", "Kubernetes", "CQRS"],
    ["Software Architect"] = ["C#", ".NET 9", "Microservices", "Domain-Driven Design", "CQRS", "Clean Architecture"],
    ["Technical Lead"] = ["C#", ".NET 9", "Java", "Microservices", "Domain-Driven Design"],
    ["Data Engineer"] = ["Python", "SQL Server", "Kafka", "Elasticsearch", "MongoDB"],
    ["Machine Learning Engineer"] = ["Python", "PostgreSQL", "Redis"],
    ["Security Engineer"] = ["Azure", "AWS", "Docker"],
};

var skillClusters = new[]
{
    (Triggers: new[] { "C#", ".NET", ".NET 9", "ASP.NET Core" },
     Suggestions: new[] { "MediatR", "CQRS", "Domain-Driven Design", "xUnit", "FluentValidation", "Entity Framework Core", "SignalR", "Minimal APIs" }),
    (Triggers: new[] { "Azure" },
     Suggestions: new[] { "Azure Service Bus", "Azure DevOps", "ArgoCD", "OpenTelemetry" }),
    (Triggers: new[] { "AWS" },
     Suggestions: new[] { "Terraform", "Kafka", "Docker", "Kubernetes" }),
    (Triggers: new[] { "React", "Next.js" },
     Suggestions: new[] { "TypeScript", "Redux Toolkit", "React Query", "Tailwind CSS", "Jest", "Vite" }),
    (Triggers: new[] { "Docker" },
     Suggestions: new[] { "Kubernetes", "Helm", "GitHub Actions", "ArgoCD" }),
    (Triggers: new[] { "SQL Server", "PostgreSQL", "MySQL" },
     Suggestions: new[] { "Redis", "Elasticsearch", "MongoDB", "Dapper", "Entity Framework Core" }),
    (Triggers: new[] { "Python" },
     Suggestions: new[] { "FastAPI", "Django", "Kafka", "PostgreSQL" }),
    (Triggers: new[] { "Microservices" },
     Suggestions: new[] { "gRPC", "RabbitMQ", "Kafka", "OpenTelemetry" }),
};

// ══════════════════════════════════════════════════════════════
// Helper functions
// ══════════════════════════════════════════════════════════════

string FormatDescription(string text, string descFormat)
{
    var isBulleted = descFormat.Equals("BULLET", StringComparison.OrdinalIgnoreCase)
                  || descFormat.Equals("Bulleted", StringComparison.OrdinalIgnoreCase);
    if (!isBulleted) return text;
    var bullets = text.Split(". ", StringSplitOptions.RemoveEmptyEntries)
        .Select(s => s.Trim().TrimEnd('.'))
        .Where(s => !string.IsNullOrWhiteSpace(s))
        .Select(s => $"• {s}.")
        .ToList();
    return bullets.Count > 0 ? string.Join("\n", bullets) : text;
}

JsonNode? ApplyAiPolish(JsonNode? node, string? parentKey = null, string descFormat = "PARAGRAPH")
{
    return node switch
    {
        JsonObject obj => ApplyToObject(obj, descFormat),
        JsonArray arr => ApplyToArray(arr, descFormat),
        JsonValue val when val.TryGetValue<string>(out var s) && !string.IsNullOrWhiteSpace(s) =>
            parentKey?.ToLowerInvariant() switch
            {
                "summary" => JsonValue.Create(summaryMocks[Math.Abs(s.GetHashCode()) % summaryMocks.Length]),
                "description" => JsonValue.Create(FormatDescription(
                                     experienceDescriptionMocks[Math.Abs(s.GetHashCode()) % experienceDescriptionMocks.Length],
                                     descFormat)),
                _ => node?.DeepClone()
            },
        _ => node?.DeepClone()
    };
}

JsonObject ApplyToObject(JsonObject obj, string descFormat = "PARAGRAPH")
{
    var result = new JsonObject();
    foreach (var (key, value) in obj)
        result[key] = ApplyAiPolish(value, key, descFormat);
    return result;
}

JsonArray ApplyToArray(JsonArray arr, string descFormat = "PARAGRAPH")
{
    var result = new JsonArray();
    foreach (var item in arr)
        result.Add(ApplyAiPolish(item, null, descFormat));
    return result;
}

HashSet<string> ExtractSkills(JsonNode? content)
{
    var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    if (content?["skills"] is not JsonArray arr) return result;
    foreach (var s in arr)
    {
        var v = s?.GetValue<string>();
        if (!string.IsNullOrWhiteSpace(v)) result.Add(v);
    }
    return result;
}

List<object> SuggestJobTitles(HashSet<string> existingSkills)
{
    var scored = new List<(string Title, int Score)>();
    foreach (var (title, boostSkills) in titleBoostMap)
    {
        var score = 5 + boostSkills.Count(s => existingSkills.Contains(s));
        scored.Add((title, Math.Min(score, 10)));
    }
    return [.. scored
        .OrderByDescending(x => x.Score)
        .Take(10)
        .Select(x => (object)new { title = x.Title, score = x.Score })];
}

List<string> SuggestSkills(HashSet<string> existingSkills)
{
    var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var suggestions = new List<string>();
    foreach (var cluster in skillClusters)
    {
        if (!cluster.Triggers.Any(t => existingSkills.Contains(t))) continue;
        foreach (var candidate in cluster.Suggestions)
        {
            if (!existingSkills.Contains(candidate) && seen.Add(candidate))
                suggestions.Add(candidate);
        }
        if (suggestions.Count >= 10) break;
    }
    foreach (var skill in masterSkillPool)
    {
        if (suggestions.Count >= 10) break;
        if (!existingSkills.Contains(skill) && seen.Add(skill))
            suggestions.Add(skill);
    }
    return suggestions.Take(10).ToList();
}

// ══════════════════════════════════════════════════════════════
// Endpoints
// ══════════════════════════════════════════════════════════════

// POST /api/ai/generate
// finalData visibly differs from rawData: summary & every experience description
// are replaced with polished mocked text; up to 5 extra skills are injected.
app.MapPost("/api/ai/generate", (JsonElement body) =>
{
    JsonNode rawNode;
    try { rawNode = JsonNode.Parse(body.GetRawText()) ?? new JsonObject(); }
    catch { return Results.BadRequest(new { error = "Invalid JSON payload." }); }

    var descFormat = rawNode["settings"]?["descriptionFormat"]?.GetValue<string>() ?? "PARAGRAPH";
    var existingSkills = ExtractSkills(rawNode["content"]);
    var skillSuggestions = SuggestSkills(existingSkills);

    var polished = ApplyAiPolish(rawNode, null, descFormat) as JsonObject;

    // Ensure settings contains all three canonical keys so finalData is consistent
    // with what LocalGenerate (the no-mock fallback) produces.
    if (polished?["settings"] is JsonObject settingsNode)
    {
        // Preserve existing summaryType / descriptionFormat; inject skillsFormat if absent.
        if (settingsNode["skillsFormat"] is null)
            settingsNode["skillsFormat"] = "GRID";
    }

    if (polished?["content"] is JsonObject contentNode)
    {
        var enrichedSkills = new JsonArray();
        foreach (var s in existingSkills) enrichedSkills.Add(JsonValue.Create(s));
        foreach (var s in skillSuggestions.Take(5))
            if (!existingSkills.Contains(s)) enrichedSkills.Add(JsonValue.Create(s));
        contentNode["skills"] = enrichedSkills;
    }

    return Results.Ok(new
    {
        FinalDataJson = polished!.ToJsonString(),
        AiAvailable = true,
        JobTitleSuggestions = SuggestJobTitles(existingSkills),
        SkillSuggestions = skillSuggestions
    });
});

// POST /api/ai/regenerate
app.MapPost("/api/ai/regenerate", (JsonElement body) =>
{
    static string? TryGet(JsonElement el, string camel, string pascal) =>
        el.TryGetProperty(camel, out var v1) ? v1.GetString() :
        el.TryGetProperty(pascal, out var v2) ? v2.GetString() : null;

    var section = (TryGet(body, "sectionIdentifier", "SectionIdentifier") ?? "section").ToLowerInvariant();
    var prompt = TryGet(body, "userPrompt", "UserPrompt") ?? string.Empty;
    var seed = prompt.Length;

    var updatedContent = section switch
    {
        "summary" => summaryMocks[seed % summaryMocks.Length],
        "experience" => experienceMocks[seed % experienceMocks.Length],
        "skills" => skillsMocks[seed % skillsMocks.Length],
        "education" => educationMocks[seed % educationMocks.Length],
        _ when section.StartsWith("exp_") => experienceDescriptionMocks[seed % experienceDescriptionMocks.Length],
        _ when section.StartsWith("edu_") => educationDescriptionMocks[seed % educationDescriptionMocks.Length],
        _ => "Professionally rewritten content tailored for your target role and industry."
    };

    return Results.Ok(new { UpdatedContent = updatedContent, AiAvailable = true });
});

app.Run();
