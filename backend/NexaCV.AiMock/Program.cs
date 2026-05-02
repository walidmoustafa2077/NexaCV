using System.Text.Json;
using System.Text.Json.Nodes;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// ── Helpers ───────────────────────────────────────────────────

// Fields whose values are identifiers, dates, proper nouns, or codes —
// the AI should only rewrite free-form text (summary, description).
var skipFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    // Identity / contact
    "id", "email", "phone", "firstName", "middleName", "lastName",
    "linkedinUrl", "siteUrl", "dateOfBirth",
    // Location
    "location", "zipCode", "city", "country",
    // Enum / format settings
    "summaryType", "descriptionFormat", "skillsFormat",
    // Experience / education metadata (proper nouns, dates, codes)
    "title", "company", "startDate", "endDate",
    "degree", "institution", "fieldOfStudy", "grade",
    // Course metadata
    "name", "provider", "date",
    // Skill level tags
    "level", "type"
};

JsonNode? ApplyAiPolish(JsonNode? node, string? parentKey = null)
{
    return node switch
    {
        JsonObject obj => ApplyToObject(obj),
        JsonArray arr => ApplyToArray(arr),
        JsonValue val
            when val.TryGetValue<string>(out var s)
            && !string.IsNullOrWhiteSpace(s)
            && (parentKey == null || !skipFields.Contains(parentKey))
            => JsonValue.Create("AI-Polished: " + s),
        _ => node?.DeepClone()
    };
}

JsonObject ApplyToObject(JsonObject obj)
{
    var result = new JsonObject();
    foreach (var (key, value) in obj)
        result[key] = ApplyAiPolish(value, key);
    return result;
}

JsonArray ApplyToArray(JsonArray arr)
{
    var result = new JsonArray();
    foreach (var item in arr)
        result.Add(ApplyAiPolish(item));
    return result;
}

// ── Suggestion data ───────────────────────────────────────────

// Job title → skills that each boost its relevance by +1 (base score = 5, capped at 10).
var titleBoostMap = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
{
    ["Software Engineer"] = ["C#", "Java", "Python", "Go", "TypeScript", ".NET"],
    ["Senior Software Engineer"] = ["C#", "Java", "Python", "Microservices", "System Design", ".NET"],
    ["Backend Developer"] = ["C#", ".NET", "ASP.NET Core", "Java", "Node.js", "SQL", "PostgreSQL"],
    ["Senior Backend Developer"] = ["C#", ".NET", "ASP.NET Core", "Redis", "Kafka", "Microservices", "SQL"],
    ["Full Stack Developer"] = ["React", "Angular", "Vue", "TypeScript", "Node.js", "C#", "JavaScript"],
    ["Senior Full Stack Developer"] = ["React", "TypeScript", "Node.js", "C#", "GraphQL", "Next.js"],
    ["Frontend Developer"] = ["React", "Angular", "Vue", "TypeScript", "JavaScript", "CSS", "Next.js"],
    ["Cloud Engineer"] = ["Azure", "AWS", "GCP", "Terraform", "Kubernetes", "Docker"],
    ["DevOps Engineer"] = ["Docker", "Kubernetes", "GitHub Actions", "Azure DevOps", "Terraform", "Helm"],
    ["Solutions Architect"] = ["Azure", "AWS", "Microservices", "System Design", "Docker", "Kubernetes"],
    ["Software Architect"] = ["C#", ".NET", "Microservices", "DDD", "CQRS", "System Design"],
    ["Technical Lead"] = ["C#", ".NET", "Java", "Agile", "System Design", "Microservices"],
    ["Data Engineer"] = ["Python", "SQL", "Spark", "Kafka", "Airflow", "BigQuery"],
    ["Machine Learning Engineer"] = ["Python", "TensorFlow", "PyTorch", "scikit-learn", "SQL"],
    ["Security Engineer"] = ["Penetration Testing", "OAuth2", "JWT", "Azure", "AWS", "Docker"],
};

// Skill clusters: having any trigger skill unlocks complementary suggestions.
var skillClusters = new[]
{
    (
        Triggers:     new[] { "C#", ".NET", ".NET 9", "ASP.NET Core" },
        Suggestions:  new[] { "MediatR", "CQRS", "Domain-Driven Design", "xUnit", "FluentValidation",
                               "Entity Framework Core", "SignalR", "Minimal APIs" }
    ),
    (
        Triggers:     new[] { "Azure" },
        Suggestions:  new[] { "Azure Functions", "Azure Service Bus", "Azure DevOps",
                               "Azure Container Apps", "Bicep", "Azure Cosmos DB" }
    ),
    (
        Triggers:     new[] { "AWS" },
        Suggestions:  new[] { "AWS Lambda", "Amazon SQS", "Amazon S3", "Amazon EKS",
                               "CloudFormation", "AWS CDK" }
    ),
    (
        Triggers:     new[] { "React", "Next.js" },
        Suggestions:  new[] { "TypeScript", "Redux Toolkit", "React Query", "Tailwind CSS",
                               "Jest", "Storybook", "Vite" }
    ),
    (
        Triggers:     new[] { "Docker" },
        Suggestions:  new[] { "Kubernetes", "Helm", "Docker Compose", "GitHub Actions", "ArgoCD" }
    ),
    (
        Triggers:     new[] { "SQL Server", "SQL", "PostgreSQL", "MySQL" },
        Suggestions:  new[] { "Query Optimisation", "Database Design", "Redis",
                               "Elasticsearch", "MongoDB", "Dapper" }
    ),
    (
        Triggers:     new[] { "Python" },
        Suggestions:  new[] { "FastAPI", "Django", "Pandas", "NumPy", "Celery", "Pydantic" }
    ),
    (
        Triggers:     new[] { "Microservices", "System Design" },
        Suggestions:  new[] { "gRPC", "RabbitMQ", "Kafka", "OpenTelemetry", "Prometheus", "Grafana" }
    ),
};

// ── Suggestion helpers ────────────────────────────────────────

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
    return suggestions.Take(10).ToList();
}

// ── POST /api/ai/generate ─────────────────────────────────────
// Accepts the wizard's rawData JSON. Returns a polished FinalData JSON,
// up to 10 job title suggestions (scored 1–10), and up to 10 skill suggestions.
app.MapPost("/api/ai/generate", (JsonElement body) =>
{
    JsonNode rawNode;
    try
    {
        rawNode = JsonNode.Parse(body.GetRawText()) ?? new JsonObject();
    }
    catch
    {
        return Results.BadRequest(new { error = "Invalid JSON payload." });
    }

    var existingSkills = ExtractSkills(rawNode["content"]);
    var polished = ApplyAiPolish(rawNode);

    return Results.Ok(new
    {
        FinalDataJson = polished!.ToJsonString(),
        AiAvailable = true,
        JobTitleSuggestions = SuggestJobTitles(existingSkills),
        SkillSuggestions = SuggestSkills(existingSkills)
    });
});

// ── Mock content banks ────────────────────────────────────────

string[] summaryMocks =
[
    "Results-driven Software Engineer with 6+ years designing and shipping high-availability .NET microservices on Azure. " +
    "Led cross-functional teams of up to 8 engineers, improved system uptime to 99.9%, and reduced average API latency by 35% through targeted optimisations. " +
    "Passionate about clean architecture, test-driven development, and delivering measurable business impact.",

    "Senior Backend Engineer specialising in distributed systems and cloud-native architecture. " +
    "Track record of scaling services to handle 10M+ daily requests on AWS, cutting infrastructure costs by 28% through right-sizing and caching strategies, " +
    "and mentoring junior engineers to build resilient, maintainable codebases.",

    "Full-Stack Developer with 5 years of experience building responsive web applications using React and ASP.NET Core. " +
    "Delivered 12 production features end-to-end, improved Core Web Vitals scores by 40%, and championed accessibility standards across a three-team engineering org.",

    "Versatile Software Engineer with a strong foundation in object-oriented design and Agile delivery. " +
    "Contributed to three greenfield projects from architecture through go-live, consistently shipping on schedule while maintaining >90% unit-test coverage. " +
    "Eager to leverage expertise in .NET, SQL Server, and Docker to drive product excellence.",
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
    "\"description\":\"Built and maintained a React + .NET 7 SaaS dashboard used by 1 200 enterprise clients. " +
    "Introduced component-level lazy loading that cut initial bundle size by 42%. " +
    "Collaborated with UX to redesign the onboarding flow, increasing 7-day activation rate from 54% to 73%.\"}]",
];

string[] skillsMocks =
[
    "[\"C#\",\".NET 9\",\"ASP.NET Core\",\"Entity Framework Core\",\"SQL Server\",\"Azure\",\"Docker\",\"Kubernetes\",\"React\",\"TypeScript\"]",
    "[\"Python\",\"FastAPI\",\"PostgreSQL\",\"Redis\",\"AWS Lambda\",\"Amazon SQS\",\"Docker\",\"Terraform\",\"React\",\"GraphQL\"]",
    "[\"Java\",\"Spring Boot\",\"Hibernate\",\"MySQL\",\"Kafka\",\"Kubernetes\",\"Jenkins\",\"Angular\",\"TypeScript\",\"Gradle\"]",
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

// Single-entry description mocks — used when sectionIdentifier is an entry ID (exp_*, edu_*, etc.)
string[] experienceDescriptionMocks =
[
    "Architected and delivered a high-throughput order-processing microservice handling 50 000 requests/min on Azure Service Bus. " +
    "Reduced end-to-end latency from 820 ms to 210 ms by introducing Redis caching and targeted query optimisation. " +
    "Mentored four junior engineers through weekly code reviews and pair-programming sessions.",

    "Designed RESTful APIs consumed by three client applications with a combined 200 000 MAU. " +
    "Migrated a legacy monolith to six independent ASP.NET Core services, cutting deployment lead time from 3 days to 45 minutes via GitHub Actions pipelines. " +
    "Achieved 99.95% uptime over 18 months post-migration.",

    "Built and maintained a React + .NET SaaS dashboard used by 1 200 enterprise clients. " +
    "Introduced component-level lazy loading that reduced the initial bundle size by 42%. " +
    "Collaborated with UX to redesign the onboarding flow, increasing the 7-day activation rate from 54% to 73%.",

    "Led end-to-end delivery of a real-time notification platform processing 2M events/day using Azure Event Hubs and SignalR. " +
    "Defined engineering standards adopted across three product teams and reduced critical-bug rate by 45% through mandatory integration testing.",
];

string[] educationDescriptionMocks =
[
    "Graduated with honours. Final-year project: distributed key-value store achieving 120 000 ops/sec on commodity hardware.",
    "Dean's List all four years. Capstone: real-time collaborative document editor built with SignalR and React.",
    "Completed specialisation in Software Architecture and Cloud Computing. Academic excellence award recipient.",
];

// ── POST /api/ai/regenerate ───────────────────────────────────
// Accepts an AiRegenerateContext payload, returns updated section content.
app.MapPost("/api/ai/regenerate", (JsonElement body) =>
{
    static string? TryGet(JsonElement el, string camel, string pascal) =>
        el.TryGetProperty(camel, out var v1) ? v1.GetString() :
        el.TryGetProperty(pascal, out var v2) ? v2.GetString() : null;

    var section = (TryGet(body, "sectionIdentifier", "SectionIdentifier") ?? "section").ToLowerInvariant();
    var prompt = TryGet(body, "userPrompt", "UserPrompt") ?? string.Empty;

    // Use prompt length as a lightweight seed so repeated calls with different
    // prompts cycle through the mock bank, giving variety without statefulness.
    var seed = prompt.Length;

    var updatedContent = section switch
    {
        "summary" => summaryMocks[seed % summaryMocks.Length],
        "experience" => experienceMocks[seed % experienceMocks.Length],
        "skills" => skillsMocks[seed % skillsMocks.Length],
        "education" => educationMocks[seed % educationMocks.Length],
        _ when section.StartsWith("exp_") => experienceDescriptionMocks[seed % experienceDescriptionMocks.Length],
        _ when section.StartsWith("edu_") => educationDescriptionMocks[seed % educationDescriptionMocks.Length],
        _ => $"Professionally rewritten content tailored for your target role and industry."
    };

    return Results.Ok(new
    {
        UpdatedContent = updatedContent,
        AiAvailable = true
    });
});

app.Run();
