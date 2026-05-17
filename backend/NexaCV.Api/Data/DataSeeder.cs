using Microsoft.EntityFrameworkCore;
using NexaCV.Api.Enums;
using NexaCV.Api.Models;

namespace NexaCV.Api.Data;

public class DataSeeder
{
    // ── Template metadata ─────────────────────────────────────────────────────
    // Each entry: (FileName, DisplayName, StyleCategory, IndustryCategory, BasePriceUsd, SupportsWord)
    private static readonly (string File, string Name, string Style, string Industry, decimal Price, bool Word)[] TemplateMetadata =
    [
        ("template_executive_corporate_centered_01.html", "Executive — Centered Classic", "Executive", "Corporate", 12.99m, true),
    ];

    public async Task SeedAsync(AppDbContext db)
    {
        var utcNow = DateTime.UtcNow;

        // ── Wipe & re-seed templates every startup (in-memory DB) ──────────────
        var existingTemplates = await db.Templates.ToListAsync();
        if (existingTemplates.Any())
        {
            db.Templates.RemoveRange(existingTemplates);
            await db.SaveChangesAsync();
        }

        var templatesDir = Path.Combine(AppContext.BaseDirectory, "Templates");

        // Fallback: if running directly from source (e.g. dotnet watch), look
        // relative to the executable's ancestor directories for a Templates folder.
        if (!Directory.Exists(templatesDir))
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir is not null)
            {
                var candidate = Path.Combine(dir.FullName, "Templates");
                if (Directory.Exists(candidate)) { templatesDir = candidate; break; }
                dir = dir.Parent;
            }
        }

        var templates = new List<Template>();

        foreach (var (file, name, style, industry, price, word) in TemplateMetadata)
        {
            var filePath = Path.Combine(templatesDir, file);
            var html = File.Exists(filePath) ? await File.ReadAllTextAsync(filePath) : null;

            templates.Add(new Template
            {
                Name = name,
                StyleCategory = style,
                IndustryCategory = industry,
                BasePriceUsd = price,
                SupportsWord = word,
                IsActive = true,
                CreatedAt = utcNow,
                HtmlContent = html,
            });
        }

        db.Templates.AddRange(templates);
        await db.SaveChangesAsync();

        // ── Default user ───────────────────────────────────────────────────────
        if (!db.Users.Any())
        {
            var defaultUser = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "User",
                Username = "testuser",
                Email = "walidmoustafa1215@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("nMxT..iwREcYJ2Y"),
                CreatedAt = utcNow
            };

            db.Users.Add(defaultUser);
            await db.SaveChangesAsync();

            // ── Seed resume for default user ───────────────────────────────────
            var seededTemplate = await db.Templates.FirstAsync();
            const string resumeJson = """
                {
                  "content": {
                    "targetJobTitle": "Senior Backend Engineer",
                    "personal": {
                      "firstName": "Walid",
                      "lastName": "Mostafa",
                      "email": "walidmoustafa1215@gmail.com",
                      "phone": "+201272102273",
                      "location": "Alexandria, Egypt",
                      "linkedinUrl": "https://linkedin.com/in/walidmostafa"
                    },
                    "summary": "Senior Backend Engineer specialising in distributed systems and cloud-native architecture. Track record of scaling services to handle 10M+ daily requests, cutting infrastructure costs by 28% through right-sizing and intelligent caching, and mentoring junior engineers to build resilient, maintainable codebases. Committed to engineering excellence through rigorous code reviews and continuous delivery practices.",
                    "experience": [
                      {
                        "id": "exp_001",
                        "title": "Senior Software Engineer",
                        "company": "TechFlow Systems",
                        "location": "Alexandria, Egypt",
                        "startDate": "2023-04",
                        "endDate": null,
                        "description": "Optimised SQL Server database by redesigning covering indexes, filtered indexes, and table partitioning strategies using execution plan analysis.\nResolved N+1 query patterns using Dapper projections and batch loading via EF Core split queries.\nImplemented read replicas with automatic failover using SQL Server Always On Availability Groups."
                      },
                      {
                        "id": "exp_002",
                        "title": "Backend Engineer",
                        "company": "Nexus Digital",
                        "location": "Cairo, Egypt",
                        "startDate": "2020-06",
                        "endDate": "2023-03",
                        "description": "Built and maintained REST APIs serving 2M+ users across 12 microservices using ASP.NET Core and Docker.\nReduced average API response time by 40% through Redis caching and connection pooling optimisations.\nLed migration from monolith to event-driven architecture using Azure Service Bus."
                      },
                      {
                        "id": "exp_003",
                        "title": "Junior .NET Developer",
                        "company": "CodeBase Solutions",
                        "location": "Alexandria, Egypt",
                        "startDate": "2018-09",
                        "endDate": "2020-05",
                        "description": "Developed internal tooling for HR and payroll management using ASP.NET MVC and SQL Server.\nIntegrated third-party payment gateways (Fawry, PayTabs) and handled webhook verification.\nWrote unit and integration tests achieving 85% code coverage across core modules."
                      }
                    ],
                    "education": [
                      {
                        "id": "edu_001",
                        "institution": "Cairo University",
                        "degree": "B.Sc. in Computer Science",
                        "fieldOfStudy": "Computer Science",
                        "grade": "3.7 / 4.0",
                        "startDate": "2014-09",
                        "endDate": "2018-06"
                      }
                    ],
                    "courses": [
                      {
                        "id": "crs_001",
                        "name": "Cloud Computing Architecture",
                        "provider": "Coursera",
                        "date": "2023-01"
                      },
                      {
                        "id": "crs_002",
                        "name": "Advanced React Patterns",
                        "provider": "Frontend Masters",
                        "date": "2022-08"
                      }
                    ],
                    "skills": [
                      { "name": "C#", "category": "Backend" },
                      { "name": ".NET 9", "category": "Backend" },
                      { "name": "ASP.NET Core", "category": "Backend" },
                      { "name": "SQL Server", "category": "Databases" },
                      { "name": "Redis", "category": "Databases" },
                      { "name": "PostgreSQL", "category": "Databases" },
                      { "name": "Docker", "category": "DevOps" },
                      { "name": "Azure", "category": "DevOps" },
                      { "name": "React", "category": "Frontend" },
                      { "name": "TypeScript", "category": "Frontend" }
                    ],
                    "languages": [
                      { "language": "Arabic", "level": "Native" },
                      { "language": "English", "level": "Professional" }
                    ]
                  }
                }
                """;

            var seededResume = new Resume
            {
                Id = Guid.NewGuid(),
                UserId = defaultUser.Id,
                TemplateId = seededTemplate.Id,
                Status = ResumeStatus.Completed,
                Name = "Senior Backend Engineer — Walid Mostafa",
                RawData = resumeJson,
                FinalData = resumeJson,
                AiAvailable = false,
                JobTitleSuggestionsJson = """[{"title":"Senior Backend Engineer","score":10},{"title":"Software Architect","score":9},{"title":"Lead .NET Developer","score":8}]""",
                SkillSuggestionsJson = """["Kubernetes","Terraform","gRPC","RabbitMQ","SignalR"]""",
                CreatedAt = utcNow,
                UpdatedAt = utcNow,
            };

            db.Resumes.Add(seededResume);
            await db.SaveChangesAsync();
        }
    }
}
