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
      ("template_branded_expert_a4_1.html", "Branded Expert — Periwinkle A4", "Executive", "Corporate", 14.99m, true),
      ("template_clean_tech_minimalist_03.html", "Clean Tech — Modern Minimalist", "ModernTech", "Technology", 11.99m, true),
      ("template_editorial_serif_creative_04.html", "Editorial Serif — Creative Professional", "Creative", "Creative", 13.99m, false),
      ("template_traditional_legal_finance_05.html", "Traditional Legal & Finance", "Executive", "Legal", 14.99m, true),
      ("template_dark_accent_executive_06.html", "Dark Accent — Executive", "Executive", "Executive", 13.99m, true),
      ("template_warm_contemporary_craft_07.html", "Warm Contemporary — Craft & Design", "Creative", "Design", 12.99m, false),
      ("template_minimalist_academic_swiss_08.html", "Minimalist Academic — Swiss", "Minimalist", "Academic", 11.99m, false),
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

    // ── Default user profile ───────────────────────────────────────────────
    // Fixed GUID matching the seed user in NexaCV.Identity's IdentitySeeder.
    // This profile corresponds to `walidmoustafa1215@gmail.com` / `testuser`.
    var seedUserId = Guid.Parse("A1B2C3D4-E5F6-7890-ABCD-EF1234567890");

    if (!await db.Profiles.AnyAsync())
    {
      var seedProfile = new NexaCvUserProfile
      {
        UserId = seedUserId,
        FirstName = "Walid",
        LastName = "Mostafa",
        Username = "testuser",
        Email = "walidmoustafa1215@gmail.com",
        DateOfBirth = new DateOnly(1995, 6, 15),
        CreatedAt = utcNow
      };

      db.Profiles.Add(seedProfile);
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
        UserId = seedUserId,
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

      // ── Second seeded resume — all sections filled ─────────────────────
      const string resumeJson2 = """
                {
                  "schemaVersion": "1.2",
                  "content": {
                    "targetJobTitle": "Full-Stack Software Engineer",
                    "personal": {
                      "firstName": "Sara",
                      "middleName": "Khaled",
                      "lastName": "Hassan",
                      "jobTitle": "Full-Stack Software Engineer",
                      "email": "sara.hassan@example.com",
                      "phone": "+201001234567",
                      "location": "Cairo, Egypt",
                      "zipCode": "11511",
                      "dateOfBirth": "1996-03-14",
                      "linkedinUrl": "https://linkedin.com/in/sara-hassan-dev",
                      "siteUrl": "https://sarahassan.dev",
                      "photoUrl": null
                    },
                    "summary": "Full-Stack Software Engineer with 6+ years building scalable web applications from database to UI. Specialises in React, Node.js, and cloud-native services on AWS. Passionate about developer experience, accessibility, and delivering features that real users love. Proven ability to lead cross-functional squads, ship under pressure, and drive 30%+ performance improvements through thoughtful architecture decisions.",
                    "experience": [
                      {
                        "id": "exp_001",
                        "title": "Senior Full-Stack Engineer",
                        "company": "Instabug",
                        "location": "Cairo, Egypt (Remote)",
                        "startDate": "2022-02",
                        "endDate": null,
                        "description": "Architected and shipped a real-time crash-analytics dashboard used by 4,000+ enterprise customers, reducing mean-time-to-resolution by 35%.\nMigrated legacy jQuery front-end to a component-driven React 18 system, cutting bundle size by 42% with code-splitting and lazy loading.\nDesigned a multi-tenant role-based access control layer using PostgreSQL row-level security and JWT claims."
                      },
                      {
                        "id": "exp_002",
                        "title": "Full-Stack Developer",
                        "company": "Sary",
                        "location": "Cairo, Egypt",
                        "startDate": "2019-07",
                        "endDate": "2022-01",
                        "description": "Built the B2B supplier portal (Next.js + GraphQL + Node.js) enabling 10,000+ SME orders per day with sub-200 ms p95 latency.\nIntegrated Stripe and Paymob payment gateways, handling PCI-DSS compliance requirements and webhook retry logic.\nEstablished the front-end testing culture; raised unit and E2E coverage from 12% to 78% using Jest and Playwright."
                      },
                      {
                        "id": "exp_003",
                        "title": "Junior Web Developer",
                        "company": "Raisa Energy",
                        "location": "Alexandria, Egypt",
                        "startDate": "2018-01",
                        "endDate": "2019-06",
                        "description": "Developed internal energy-monitoring dashboards with React and D3.js, visualising live SCADA data for 20 field engineers.\nImplemented REST API endpoints in Express.js backed by MongoDB, supporting offline-first sync via service workers.\nCollaborated with UX designers to redesign the mobile experience, improving task-completion rate by 22% in usability testing."
                      }
                    ],
                    "education": [
                      {
                        "id": "edu_001",
                        "institution": "Alexandria University",
                        "degree": "B.Sc. in Computer Engineering",
                        "fieldOfStudy": "Computer Engineering",
                        "grade": "3.82 / 4.0",
                        "startDate": "2013-09",
                        "endDate": "2017-06"
                      },
                      {
                        "id": "edu_002",
                        "institution": "Coursera / Google",
                        "degree": "Professional Certificate in UX Design",
                        "fieldOfStudy": "UX Design",
                        "grade": null,
                        "startDate": "2021-01",
                        "endDate": "2021-06"
                      }
                    ],
                    "courses": [
                      {
                        "id": "crs_001",
                        "name": "AWS Certified Developer — Associate",
                        "provider": "Amazon Web Services",
                        "date": "2023-05"
                      },
                      {
                        "id": "crs_002",
                        "name": "Fullstack Open",
                        "provider": "University of Helsinki",
                        "date": "2022-11"
                      },
                      {
                        "id": "crs_003",
                        "name": "System Design for Interviews and Beyond",
                        "provider": "Exponent",
                        "date": "2022-04"
                      }
                    ],
                    "projects": [
                      {
                        "id": "prj_001",
                        "name": "OpenBudget",
                        "role": "Creator & Maintainer",
                        "description": "Open-source personal finance tracker built with Next.js 14 App Router, Prisma, and Supabase. Supports multi-currency, recurring transactions, and AI-powered spending forecasts. 1,200+ GitHub stars.",
                        "link": "https://github.com/sara-hassan/openbudget",
                        "technologies": ["Next.js", "TypeScript", "Prisma", "Supabase", "OpenAI API"]
                      },
                      {
                        "id": "prj_002",
                        "name": "AccessAudit CLI",
                        "role": "Lead Developer",
                        "description": "Node.js CLI tool that crawls a website and generates a WCAG 2.1 AA compliance report with actionable fixes. Used internally at Instabug to audit 50+ product pages automatically on every release.",
                        "link": "https://github.com/sara-hassan/access-audit",
                        "technologies": ["Node.js", "Playwright", "axe-core", "TypeScript"]
                      },
                      {
                        "id": "prj_003",
                        "name": "NexaChat",
                        "role": "Full-Stack Developer",
                        "description": "Real-time chat platform supporting rooms, file sharing, and message reactions, built as a learning exercise for WebSocket scaling patterns. Handles 500+ concurrent connections on a single t3.micro instance.",
                        "link": "https://nexachat.sarahassan.dev",
                        "technologies": ["React", "Socket.IO", "Express.js", "Redis", "PostgreSQL"]
                      }
                    ],
                    "skills": [
                      { "name": "React", "category": "Frontend" },
                      { "name": "Next.js", "category": "Frontend" },
                      { "name": "TypeScript", "category": "Frontend" },
                      { "name": "Tailwind CSS", "category": "Frontend" },
                      { "name": "Node.js", "category": "Backend" },
                      { "name": "Express.js", "category": "Backend" },
                      { "name": "GraphQL", "category": "Backend" },
                      { "name": "PostgreSQL", "category": "Databases" },
                      { "name": "MongoDB", "category": "Databases" },
                      { "name": "Redis", "category": "Databases" },
                      { "name": "AWS (EC2, S3, Lambda)", "category": "Cloud & DevOps" },
                      { "name": "Docker", "category": "Cloud & DevOps" },
                      { "name": "GitHub Actions", "category": "Cloud & DevOps" },
                      { "name": "Jest", "category": "Testing" },
                      { "name": "Playwright", "category": "Testing" }
                    ],
                    "languages": [
                      { "language": "Arabic", "level": "Native" },
                      { "language": "English", "level": "Fluent" },
                      { "language": "French", "level": "Intermediate" }
                    ],
                    "volunteers": [
                      {
                        "id": "vol_001",
                        "organization": "GirlsWhoCode Egypt",
                        "role": "Coding Mentor",
                        "startDate": "2021-03",
                        "endDate": null,
                        "description": "Mentor a cohort of 15 university students through weekly coding sessions, code reviews, and mock technical interviews."
                      }
                    ],
                    "hobbies": ["Open-source contribution", "Technical blogging", "Rock climbing", "Film photography"],
                    "other": [
                      { "label": "GitHub", "value": "github.com/sara-hassan" },
                      { "label": "Clearance", "value": "Open to relocation — EU / UK" },
                      { "label": "Availability", "value": "2 weeks notice" }
                    ]
                  }
                }
                """;

      var seededResume2 = new Resume
      {
        Id = Guid.NewGuid(),
        UserId = seedUserId,
        TemplateId = seededTemplate.Id,
        Status = ResumeStatus.Completed,
        Name = "Full-Stack Engineer — Sara Hassan",
        RawData = resumeJson2,
        FinalData = resumeJson2,
        AiAvailable = false,
        JobTitleSuggestionsJson = """[{"title":"Full-Stack Software Engineer","score":10},{"title":"Senior Frontend Engineer","score":9},{"title":"Tech Lead","score":8}]""",
        SkillSuggestionsJson = """["Kubernetes","Terraform","React Native","tRPC","Vitest"]""",
        CreatedAt = utcNow,
        UpdatedAt = utcNow,
      };

      db.Resumes.Add(seededResume2);

      // ── Third seeded resume — Walid Mostafa, .NET Software Engineer variant ──
      const string resumeJson3 = """
                {
                  "schemaVersion": "1.2",
                  "content": {
                    "targetJobTitle": ".NET Software Engineer",
                    "personal": {
                      "firstName": "Walid",
                      "lastName": "Mostafa",
                      "jobTitle": ".NET Software Engineer",
                      "email": "walidmoustafa1215@gmail.com",
                      "phone": "+201272102273",
                      "location": "Alexandria, Egypt",
                      "linkedinUrl": "https://linkedin.com/in/walidmostafa"
                    },
                    "summary": "Results-driven Software Engineer with 6+ years designing and shipping high-availability .NET microservices on Azure. Led cross-functional teams of up to 8 engineers, improved system uptime to 99.9%, and reduced average API latency by 35% through targeted optimisations. Passionate about clean architecture, test-driven development, and delivering measurable business impact.",
                    "experience": [
                      {
                        "id": "exp_001",
                        "title": "Senior Software Engineer",
                        "company": "TechFlow Systems",
                        "location": "Alexandria, Egypt",
                        "startDate": "2023-04",
                        "endDate": null,
                        "description": "Built a developer platform using Angular framework and ASP.NET Core backed with a Terraform-powered provisioning engine.\nIntegrated Azure Active Directory for SSO and fine-grained RBAC using Microsoft Identity Web.\nImplemented approval workflows with Azure Service Bus end-to-end and a real-time notification engine built on the Serverless .NET library."
                      },
                      {
                        "id": "exp_002",
                        "title": "Backend Engineer",
                        "company": "Nexus Digital",
                        "location": "Cairo, Egypt",
                        "startDate": "2020-06",
                        "endDate": "2023-03",
                        "description": "Implemented CI/CD pipelines using GitHub Actions, ArgoCD, and Terraform across 8 engineering teams.\nBuilt reusable GitHub Actions containerisation templates that standardised deployment across the organisation.\nConfigured Kubernetes namespaces, resource quotas, horizontal pod autoscalers, and network policies for multi-tenant environment isolation."
                      },
                      {
                        "id": "exp_003",
                        "title": "Junior .NET Developer",
                        "company": "CodeBase Solutions",
                        "location": "Alexandria, Egypt",
                        "startDate": "2018-09",
                        "endDate": "2020-05",
                        "description": "Built a full-stack SaaS dashboard using React, TypeScript, and .NET 5 with Entity Framework Core and PostgreSQL.\nImplemented real-time updates via SignalR and server-state management with React Query and Zustand.\nDesigned a RESTful API with role-based access control, refresh-token rotation, and OpenAPI documentation."
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
                      },
                      {
                        "id": "crs_003",
                        "name": "Designing Distributed Systems",
                        "provider": "O'Reilly Learning",
                        "date": "2021-05"
                      }
                    ],
                    "projects": [
                      {
                        "id": "prj_001",
                        "name": "OpenBudget",
                        "role": "Creator & Maintainer",
                        "description": "Open-source personal finance tracker built with Next.js App Router, Prisma, and Supabase. Supports multi-currency, recurring transactions, and AI-powered spending forecasts. 1,200+ GitHub stars.",
                        "link": "https://github.com/walid-mostafa/openbudget",
                        "technologies": ["Next.js", "TypeScript", "Prisma", "Supabase", "OpenAI API"]
                      }
                    ],
                    "skills": [
                      { "name": "C#", "category": "Backend" },
                      { "name": ".NET 9", "category": "Backend" },
                      { "name": "ASP.NET Core", "category": "Backend" },
                      { "name": "gRPC", "category": "Backend" },
                      { "name": "FluentValidation", "category": "Backend" },
                      { "name": "SQL Server", "category": "Databases" },
                      { "name": "Redis", "category": "Databases" },
                      { "name": "PostgreSQL", "category": "Databases" },
                      { "name": "Docker", "category": "DevOps" },
                      { "name": "Kubernetes", "category": "DevOps" },
                      { "name": "Azure", "category": "DevOps" },
                      { "name": "GitHub Actions", "category": "DevOps" },
                      { "name": "React", "category": "Frontend" },
                      { "name": "TypeScript", "category": "Frontend" },
                      { "name": "GraphQL", "category": "Architecture" },
                      { "name": "CQRS", "category": "Architecture" },
                      { "name": "Domain-Driven Design", "category": "Architecture" }
                    ],
                    "languages": [
                      { "language": "Arabic", "level": "Native" },
                      { "language": "English", "level": "Fluent" }
                    ],
                    "volunteers": [
                      {
                        "id": "vol_001",
                        "organization": "Team One Co",
                        "role": "Lead Dev",
                        "startDate": "2022-01",
                        "endDate": null,
                        "description": "Open-source personal finance tracker built with Next.js App Router, Prisma, and Supabase. Supports multi-currency, recurring transactions, and AI-powered spending forecasts. 1,200+ GitHub stars."
                      }
                    ],
                    "hobbies": ["Open-source contribution", "Technical writing", "Chess"],
                    "other": [
                      { "label": "GitHub", "value": "github.com/walid-mostafa" },
                      { "label": "Availability", "value": "Open to remote opportunities" }
                    ]
                  }
                }
                """;

      var seededResume3 = new Resume
      {
        Id = Guid.NewGuid(),
        UserId = seedUserId,
        TemplateId = seededTemplate.Id,
        Status = ResumeStatus.Completed,
        Name = ".NET Software Engineer — Walid Mostafa",
        RawData = resumeJson3,
        FinalData = resumeJson3,
        AiAvailable = false,
        JobTitleSuggestionsJson = """[{"title":".NET Software Engineer","score":10},{"title":"Senior Backend Engineer","score":9},{"title":"Cloud Engineer","score":8}]""",
        SkillSuggestionsJson = """["Terraform","ArgoCD","MediatR","SignalR","OpenTelemetry"]""",
        CreatedAt = utcNow,
        UpdatedAt = utcNow,
      };

      db.Resumes.Add(seededResume3);

      // ── Fourth seeded resume — Product Manager with extensive experience ────
      const string resumeJson4 = """
                {
                  "schemaVersion": "1.2",
                  "content": {
                    "targetJobTitle": "Senior Product Manager",
                    "personal": {
                      "firstName": "Ahmed",
                      "lastName": "Khalil",
                      "jobTitle": "Senior Product Manager",
                      "email": "ahmed.khalil@example.com",
                      "phone": "+201001111111",
                      "location": "Cairo, Egypt",
                      "linkedinUrl": "https://linkedin.com/in/ahmadkhalil"
                    },
                    "summary": "Results-driven Senior Product Manager with 9+ years leading cross-functional teams to deliver products serving 5M+ users across mobile and web. Expert in defining OKRs, roadmap prioritisation, and driving product adoption through data-driven decision-making. Proven ability to scale from early stage to Series C with 40%+ YoY growth.",
                    "experience": [
                      {
                        "id": "exp_001",
                        "title": "Senior Product Manager",
                        "company": "Instabug",
                        "location": "Cairo, Egypt",
                        "startDate": "2022-06",
                        "endDate": null,
                        "description": "Led product strategy for crash analytics suite serving 4,000+ enterprise customers.\nIncreased ARPU by 32% through tier-based pricing and feature packaging strategy.\nManaged cross-functional team of 12 engineers, 2 designers, and data analysts to deliver biweekly releases."
                      },
                      {
                        "id": "exp_002",
                        "title": "Product Manager",
                        "company": "Sary",
                        "location": "Cairo, Egypt",
                        "startDate": "2019-08",
                        "endDate": "2022-05",
                        "description": "Scaled the supplier marketplace from 500 to 10,000+ vendors through targeted partnership campaigns.\nRedesigned onboarding flow reducing drop-off by 28% and improving activation from 15% to 52%.\nDefined and implemented data analytics framework tracking 50+ key metrics across user journey."
                      },
                      {
                        "id": "exp_003",
                        "title": "Associate Product Manager",
                        "company": "Raisa Energy",
                        "location": "Alexandria, Egypt",
                        "startDate": "2017-02",
                        "endDate": "2019-07",
                        "description": "Launched IoT monitoring platform for 100+ industrial customers with $2M ARR.\nConducted user research via 150+ interviews to validate product-market fit and inform roadmap.\nManaged vendor partnerships with hardware suppliers and cloud providers to reduce COGS by 22%."
                      },
                      {
                        "id": "exp_004",
                        "title": "Business Development Manager",
                        "company": "TechFlow Systems",
                        "location": "Alexandria, Egypt",
                        "startDate": "2015-09",
                        "endDate": "2017-01",
                        "description": "Closed 25+ enterprise contracts generating $1.5M in new ARR through consultative selling.\nBuilt partner ecosystem with 8 integrations increasing product stickiness and reducing churn by 15%.\nEstablished product advisory board with 12 key customers for quarterly feedback sessions."
                      },
                      {
                        "id": "exp_005",
                        "title": "Sales Executive",
                        "company": "Nexus Digital",
                        "location": "Cairo, Egypt",
                        "startDate": "2013-11",
                        "endDate": "2015-08",
                        "description": "Built and led sales team from 1 to 5 people, generating $800K ARR in first year.\nEstablished account-based marketing strategy targeting Fortune 500 companies.\nAchieved 95% customer retention and 40% net revenue retention through proactive support."
                      },
                      {
                        "id": "exp_006",
                        "title": "Junior Sales Representative",
                        "company": "CodeBase Solutions",
                        "location": "Alexandria, Egypt",
                        "startDate": "2012-01",
                        "endDate": "2013-10",
                        "description": "Exceeded quarterly targets by 35% on average, consistently ranking #1 in sales team.\nPurposefully developed consultative selling skills and built a network of 200+ qualified leads.\nTrained incoming sales hires on best practices and product positioning."
                      }
                    ],
                    "education": [
                      {
                        "id": "edu_001",
                        "institution": "American University in Cairo",
                        "degree": "B.A. in Business Administration",
                        "fieldOfStudy": "Business & Entrepreneurship",
                        "grade": "3.6 / 4.0",
                        "startDate": "2008-09",
                        "endDate": "2012-06"
                      },
                      {
                        "id": "edu_002",
                        "institution": "General Assembly",
                        "degree": "Certificate in Product Management",
                        "fieldOfStudy": "Product Management",
                        "grade": null,
                        "startDate": "2018-01",
                        "endDate": "2018-04"
                      }
                    ],
                    "courses": [
                      {
                        "id": "crs_001",
                        "name": "Reforge: Product Strategy",
                        "provider": "Reforge",
                        "date": "2023-03"
                      },
                      {
                        "id": "crs_002",
                        "name": "Mind the Product: Product Metrics",
                        "provider": "MindTheProduct",
                        "date": "2022-06"
                      }
                    ],
                    "projects": [
                      {
                        "id": "prj_001",
                        "name": "Marketplace Launch",
                        "role": "Product Lead",
                        "description": "Led end-to-end marketplace platform launch for Sary connecting 10,000+ suppliers to retailers, generating $5M GMV.",
                        "link": null,
                        "technologies": ["Product Strategy", "Marketplace", "Analytics"]
                      },
                      {
                        "id": "prj_002",
                        "name": "Mobile App Redesign",
                        "role": "Senior PM",
                        "description": "Spearheaded complete iOS and Android redesign increasing daily active users by 45% and session duration by 60%.",
                        "link": null,
                        "technologies": ["Mobile", "UX", "Growth"]
                      },
                      {
                        "id": "prj_003",
                        "name": "Enterprise Tier Launch",
                        "role": "Product Manager",
                        "description": "Defined and shipped enterprise tier with advanced features like SSO, audit logs, and dedicated support, adding $3M ARR.",
                        "link": null,
                        "technologies": ["Enterprise", "Monetization", "Security"]
                      },
                      {
                        "id": "prj_004",
                        "name": "AI-Powered Recommendations",
                        "role": "Product Lead",
                        "description": "Designed and launched ML-powered recommendation engine increasing conversion by 22% and ARPU by 18%.",
                        "link": null,
                        "technologies": ["AI/ML", "Recommendations", "Personalization"]
                      }
                    ],
                    "skills": [
                      { "name": "Product Strategy", "category": "Core" },
                      { "name": "OKR Planning", "category": "Core" },
                      { "name": "Roadmap Planning", "category": "Core" },
                      { "name": "User Research", "category": "Research" },
                      { "name": "Analytics & Data Interpretation", "category": "Research" },
                      { "name": "SQL", "category": "Technical" },
                      { "name": "Figma", "category": "Design" },
                      { "name": "Jira", "category": "Collaboration" },
                      { "name": "Mixpanel", "category": "Analytics" },
                      { "name": "Intercom", "category": "CRM" }
                    ],
                    "languages": [
                      { "language": "Arabic", "level": "Native" },
                      { "language": "English", "level": "Fluent" }
                    ],
                    "volunteers": [
                      {
                        "id": "vol_001",
                        "organization": "Startup Grind Cairo",
                        "role": "Mentor & Speaker",
                        "startDate": "2019-01",
                        "endDate": null,
                        "description": "Mentor 10+ early-stage founders on product-market fit, fundraising, and go-to-market strategy."
                      },
                      {
                        "id": "vol_002",
                        "organization": "Google Startup Ecosystem",
                        "role": "Product Expert Panelist",
                        "startDate": "2020-06",
                        "endDate": null,
                        "description": "Participate in quarterly panels and workshops educating 500+ founders on scaling product teams."
                      }
                    ],
                    "hobbies": ["Product blogging", "Angel investing", "Hiking", "Reading business books"],
                    "other": [
                      { "label": "Speaking", "value": "5+ conferences (Web Summit, Expansion Summit, TC Disrupt)" },
                      { "label": "Publications", "value": "Featured in TechCrunch, Forbes, Arab News" }
                    ]
                  }
                }
                """;

      var seededResume4 = new Resume
      {
        Id = Guid.NewGuid(),
        UserId = seedUserId,
        TemplateId = seededTemplate.Id,
        Status = ResumeStatus.Completed,
        Name = "Senior Product Manager — Ahmed Khalil",
        RawData = resumeJson4,
        FinalData = resumeJson4,
        AiAvailable = false,
        JobTitleSuggestionsJson = """[{"title":"Senior Product Manager","score":10},{"title":"Director of Product","score":9},{"title":"VP Product","score":8}]""",
        SkillSuggestionsJson = """["Amplitude","LaunchDarkly","Notion","Maze","ProductBoard"]""",
        CreatedAt = utcNow,
        UpdatedAt = utcNow,
      };

      db.Resumes.Add(seededResume4);

      // ── Fifth seeded resume — Design Lead with rich project portfolio ────────
      const string resumeJson5 = """
                {
                  "schemaVersion": "1.2",
                  "content": {
                    "targetJobTitle": "Design Lead",
                    "personal": {
                      "firstName": "Noor",
                      "lastName": "Ahmed",
                      "jobTitle": "Design Lead",
                      "email": "noor.ahmed@example.com",
                      "phone": "+201002222222",
                      "location": "Cairo, Egypt",
                      "linkedinUrl": "https://linkedin.com/in/noorahmed",
                      "siteUrl": "https://noorahmed.design"
                    },
                    "summary": "Award-winning Design Lead with 8+ years crafting intuitive digital experiences for 10M+ users globally. Led cross-functional teams at scale, establishing design systems that reduced time-to-market by 40% and improved accessibility compliance to WCAG AAA. Passionate about human-centered design, inclusive practices, and mentoring emerging design talent.",
                    "experience": [
                      {
                        "id": "exp_001",
                        "title": "Design Lead",
                        "company": "Instabug",
                        "location": "Cairo, Egypt (Remote)",
                        "startDate": "2022-03",
                        "endDate": null,
                        "description": "Leading design strategy for crash analytics dashboard serving 4,000+ enterprise customers across 50+ countries.\nEstablished comprehensive design system in Figma reducing component recreation time by 60%.\nMentored team of 5 designers and conducted design reviews for product quality assurance."
                      },
                      {
                        "id": "exp_002",
                        "title": "Senior Product Designer",
                        "company": "Sary",
                        "location": "Cairo, Egypt",
                        "startDate": "2019-09",
                        "endDate": "2022-02",
                        "description": "Led redesign of B2B supplier portal (used by 10,000+ vendors) improving task completion rate by 45%.\nConducted 200+ user interviews and usability tests informing design decisions across 3 product lines.\nDesigned and launched mobile app achieving 4.8-star rating on App Store and Play Store."
                      },
                      {
                        "id": "exp_003",
                        "title": "UX/UI Designer",
                        "company": "Raisa Energy",
                        "location": "Alexandria, Egypt",
                        "startDate": "2017-05",
                        "endDate": "2019-08",
                        "description": "Designed industrial IoT monitoring dashboards with real-time data visualisation for 100+ enterprise customers.\nImplemented responsive design principles ensuring seamless experience across desktop, tablet, and mobile.\nConducted accessibility audit and remediated 150+ issues achieving WCAG AA compliance."
                      },
                      {
                        "id": "exp_004",
                        "title": "UI Designer",
                        "company": "TechFlow Systems",
                        "location": "Alexandria, Egypt",
                        "startDate": "2016-01",
                        "endDate": "2017-04",
                        "description": "Designed user interfaces for SaaS platform serving 5,000+ SME customers across the Middle East.\nCreated comprehensive icon library and style guide enabling consistent branding across touchpoints.\nOptimised design-to-development handoff using Zeplin, reducing back-and-forth iterations by 35%."
                      },
                      {
                        "id": "exp_005",
                        "title": "Junior UX Designer",
                        "company": "Nexus Digital",
                        "location": "Cairo, Egypt",
                        "startDate": "2015-03",
                        "endDate": "2015-12",
                        "description": "Assisted in redesigning company website and customer portal, improving bounce rate by 25%.\nConducted user testing sessions with 50+ customers providing qualitative insights on design decisions.\nDesigned wireframes and high-fidelity mockups for 10+ new features and product lines."
                      },
                      {
                        "id": "exp_006",
                        "title": "Graphic Designer",
                        "company": "CodeBase Solutions",
                        "location": "Alexandria, Egypt",
                        "startDate": "2014-06",
                        "endDate": "2015-02",
                        "description": "Designed marketing collateral, web assets, and promotional materials for growing tech startup.\nDeveloped brand guidelines ensuring consistency across all customer-facing materials.\nCreated data visualisations for reports and presentations enhancing stakeholder communication."
                      }
                    ],
                    "education": [
                      {
                        "id": "edu_001",
                        "institution": "Helwan University",
                        "degree": "B.A. in Graphic Design",
                        "fieldOfStudy": "Graphic Design & Visual Communication",
                        "grade": "3.8 / 4.0",
                        "startDate": "2010-09",
                        "endDate": "2014-06"
                      },
                      {
                        "id": "edu_002",
                        "institution": "Interaction Design Foundation",
                        "degree": "Certificate in UX Design",
                        "fieldOfStudy": "User Experience Design",
                        "grade": null,
                        "startDate": "2017-01",
                        "endDate": "2017-08"
                      }
                    ],
                    "courses": [
                      {
                        "id": "crs_001",
                        "name": "Advanced Prototyping in Figma",
                        "provider": "Interaction Design Foundation",
                        "date": "2022-09"
                      },
                      {
                        "id": "crs_002",
                        "name": "Accessibility for UX Designers",
                        "provider": "Nielsen Norman Group",
                        "date": "2021-11"
                      }
                    ],
                    "projects": [
                      {
                        "id": "prj_001",
                        "name": "Enterprise Dashboard Redesign",
                        "role": "Design Lead",
                        "description": "Led complete redesign of Instabug analytics dashboard for enterprise customers, incorporating AI-powered insights and real-time alerts. Reduced cognitive load by 40% through information hierarchy and progressive disclosure.",
                        "link": "https://instabug.com",
                        "technologies": ["Figma", "Design Systems", "Data Visualization"]
                      },
                      {
                        "id": "prj_002",
                        "name": "Mobile App Design System",
                        "role": "Senior Designer",
                        "description": "Designed comprehensive mobile design system for iOS and Android with 80+ reusable components. Enabled teams to ship features 3x faster while maintaining consistency and quality.",
                        "link": null,
                        "technologies": ["Figma", "Design Systems", "Mobile Design"]
                      },
                      {
                        "id": "prj_003",
                        "name": "Accessibility Initiative",
                        "role": "UX Lead",
                        "description": "Spearheaded company-wide accessibility initiative, auditing all product surfaces and remediating 500+ issues. Achieved WCAG AAA compliance across web and mobile products.",
                        "link": null,
                        "technologies": ["Accessibility", "WCAG", "Audit"]
                      },
                      {
                        "id": "prj_004",
                        "name": "Supplier Onboarding Flow",
                        "role": "Product Designer",
                        "description": "Redesigned 8-step vendor onboarding flow reducing abandonment by 45% and time-to-complete by 60%. Conducted 30+ user interviews and 5 rounds of usability testing.",
                        "link": null,
                        "technologies": ["User Research", "Prototyping", "Usability Testing"]
                      }
                    ],
                    "skills": [
                      { "name": "Figma", "category": "Design Tools" },
                      { "name": "Prototyping", "category": "Design Tools" },
                      { "name": "User Research", "category": "UX" },
                      { "name": "Usability Testing", "category": "UX" },
                      { "name": "Information Architecture", "category": "UX" },
                      { "name": "Wireframing", "category": "Design" },
                      { "name": "Visual Design", "category": "Design" },
                      { "name": "Design Systems", "category": "Design" },
                      { "name": "Accessibility (WCAG)", "category": "UX" },
                      { "name": "Data Visualization", "category": "Specialisation" }
                    ],
                    "languages": [
                      { "language": "Arabic", "level": "Native" },
                      { "language": "English", "level": "Fluent" }
                    ],
                    "volunteers": [
                      {
                        "id": "vol_001",
                        "organization": "Design & Code Egypt",
                        "role": "Design Workshop Facilitator",
                        "startDate": "2020-01",
                        "endDate": null,
                        "description": "Lead monthly design workshops for 50+ aspiring UX/UI designers covering design principles, Figma, and career guidance."
                      },
                      {
                        "id": "vol_002",
                        "organization": "Women in Tech Cairo",
                        "role": "Career Mentor",
                        "startDate": "2019-06",
                        "endDate": null,
                        "description": "Mentor 8 women in tech through career transitions into design roles, providing portfolio reviews and interview prep."
                      }
                    ],
                    "hobbies": ["Illustration", "Photography", "Travel", "Design blogging"],
                    "other": [
                      { "label": "Awards", "value": "Dribbble Best Shot (2021, 2022, 2023), MENA Design Excellence" },
                      { "label": "Speaking", "value": "Spoke at 12+ design conferences on accessibility and design systems" }
                    ]
                  }
                }
                """;

      var seededResume5 = new Resume
      {
        Id = Guid.NewGuid(),
        UserId = seedUserId,
        TemplateId = seededTemplate.Id,
        Status = ResumeStatus.Completed,
        Name = "Design Lead — Noor Ahmed",
        RawData = resumeJson5,
        FinalData = resumeJson5,
        AiAvailable = false,
        JobTitleSuggestionsJson = """[{"title":"Design Lead","score":10},{"title":"Senior Product Designer","score":9},{"title":"Design Manager","score":8}]""",
        SkillSuggestionsJson = """["Penpot","Adobe XD","Sketch","InVision","Zeplin"]""",
        CreatedAt = utcNow,
        UpdatedAt = utcNow,
      };

      db.Resumes.Add(seededResume5);

      // ── Sixth seeded resume — QA Engineer with comprehensive test automation ──
      const string resumeJson6 = """
                {
                  "schemaVersion": "1.2",
                  "content": {
                    "targetJobTitle": "QA Automation Engineer",
                    "personal": {
                      "firstName": "Mona",
                      "lastName": "Ibrahim",
                      "jobTitle": "QA Automation Engineer",
                      "email": "mona.ibrahim@example.com",
                      "phone": "+201003333333",
                      "location": "Alexandria, Egypt",
                      "linkedinUrl": "https://linkedin.com/in/monaibrahim"
                    },
                    "summary": "Quality Assurance Automation Engineer with 7+ years experience designing and implementing robust test automation frameworks. Expert in E2E testing, API testing, and performance testing across web and mobile platforms. Reduced test execution time by 65% through intelligent test parallelisation and flaky test elimination. Passionate about test-driven development and shifting quality left.",
                    "experience": [
                      {
                        "id": "exp_001",
                        "title": "Senior QA Automation Engineer",
                        "company": "Instabug",
                        "location": "Cairo, Egypt",
                        "startDate": "2023-02",
                        "endDate": null,
                        "description": "Architected enterprise-grade test automation framework for crash analytics platform using Playwright and Node.js.\nImplemented CI/CD pipeline in GitHub Actions with parallel test execution reducing time from 45m to 12m.\nMentored team of 3 QA engineers and established test strategy documentation and best practices."
                      },
                      {
                        "id": "exp_002",
                        "title": "QA Automation Engineer",
                        "company": "Sary",
                        "location": "Cairo, Egypt",
                        "startDate": "2020-08",
                        "endDate": "2023-01",
                        "description": "Designed and implemented end-to-end test suite covering 10,000+ user flows across web and mobile apps using Cypress and Detox.\nBuilt API test framework using RestAssured and Postman for 50+ microservices endpoints.\nReduced production bugs by 58% through comprehensive regression testing and exploratory testing strategies."
                      },
                      {
                        "id": "exp_003",
                        "title": "QA Engineer",
                        "company": "Raisa Energy",
                        "location": "Alexandria, Egypt",
                        "startDate": "2018-06",
                        "endDate": "2020-07",
                        "description": "Automated 1,000+ manual test cases using Selenium and Java, increasing test coverage from 35% to 82%.\nImplemented continuous testing in Azure DevOps pipelines enabling shift-left quality practices.\nConducted performance testing on IoT platform identifying and resolving critical bottlenecks."
                      },
                      {
                        "id": "exp_004",
                        "title": "Junior QA Engineer",
                        "company": "TechFlow Systems",
                        "location": "Alexandria, Egypt",
                        "startDate": "2017-03",
                        "endDate": "2018-05",
                        "description": "Performed manual and automated testing for web and mobile applications across multiple browsers and devices.\nDeveloped Python scripts for test data generation and test environment setup automation.\nLogged and tracked 500+ bugs using JIRA, coordinating resolution with development teams."
                      },
                      {
                        "id": "exp_005",
                        "title": "Test Analyst",
                        "company": "Nexus Digital",
                        "location": "Cairo, Egypt",
                        "startDate": "2015-11",
                        "endDate": "2017-02",
                        "description": "Created comprehensive test plans and test cases for 15+ product releases affecting 2M+ users.\nExecuted manual regression testing and conducted exploratory testing sessions identifying critical issues.\nParticipated in UAT coordination with business stakeholders ensuring readiness for production release."
                      },
                      {
                        "id": "exp_006",
                        "title": "Quality Assurance Trainee",
                        "company": "CodeBase Solutions",
                        "location": "Alexandria, Egypt",
                        "startDate": "2015-01",
                        "endDate": "2015-10",
                        "description": "Assisted in manual testing of ERP and HR management systems ensuring quality and regulatory compliance.\nLearned ISTQB testing methodologies and participated in testing knowledge transfer sessions.\nDocumented test results and provided detailed bug reports supporting rapid issue resolution."
                      }
                    ],
                    "education": [
                      {
                        "id": "edu_001",
                        "institution": "Alexandria University",
                        "degree": "B.Sc. in Computer Science",
                        "fieldOfStudy": "Computer Science",
                        "grade": "3.5 / 4.0",
                        "startDate": "2011-09",
                        "endDate": "2015-06"
                      },
                      {
                        "id": "edu_002",
                        "institution": "ISTQB",
                        "degree": "Certified Tester - CTFL",
                        "fieldOfStudy": "Software Testing",
                        "grade": null,
                        "startDate": "2018-01",
                        "endDate": "2018-03"
                      }
                    ],
                    "courses": [
                      {
                        "id": "crs_001",
                        "name": "Advanced Selenium WebDriver",
                        "provider": "Udemy",
                        "date": "2022-08"
                      },
                      {
                        "id": "crs_002",
                        "name": "Playwright End-to-End Testing",
                        "provider": "Scrimba",
                        "date": "2023-03"
                      }
                    ],
                    "projects": [
                      {
                        "id": "prj_001",
                        "name": "E2E Test Framework Implementation",
                        "role": "Lead QA Engineer",
                        "description": "Architected enterprise Playwright framework with 2,000+ test cases for Instabug analytics platform. Implemented intelligent waits, custom reporters, and parallel execution reducing suite runtime from 2h to 20m.",
                        "link": null,
                        "technologies": ["Playwright", "Node.js", "GitHub Actions"]
                      },
                      {
                        "id": "prj_002",
                        "name": "Microservices API Testing Suite",
                        "role": "QA Automation Engineer",
                        "description": "Designed RestAssured-based API test framework covering 50+ endpoints across 8 microservices with contract testing and performance assertions.",
                        "link": null,
                        "technologies": ["RestAssured", "Java", "Postman", "Docker"]
                      },
                      {
                        "id": "prj_003",
                        "name": "Mobile Automation Automation",
                        "role": "Senior QA Engineer",
                        "description": "Implemented Detox framework for iOS and Android automation testing 10,000+ user flows. Integrated with CI/CD achieving 15-minute test cycle.",
                        "link": null,
                        "technologies": ["Detox", "Appium", "React Native"]
                      },
                      {
                        "id": "prj_004",
                        "name": "Performance Testing & Monitoring",
                        "role": "QA Lead",
                        "description": "Conducted load testing and stress testing on IoT platform identifying infrastructure bottlenecks. Implemented continuous performance monitoring in DataDog.",
                        "link": null,
                        "technologies": ["JMeter", "k6", "DataDog"]
                      }
                    ],
                    "skills": [
                      { "name": "Playwright", "category": "Automation" },
                      { "name": "Selenium WebDriver", "category": "Automation" },
                      { "name": "Cypress", "category": "Automation" },
                      { "name": "Java", "category": "Programming" },
                      { "name": "Python", "category": "Programming" },
                      { "name": "JavaScript", "category": "Programming" },
                      { "name": "RestAssured", "category": "API Testing" },
                      { "name": "Postman", "category": "API Testing" },
                      { "name": "Docker", "category": "DevOps" },
                      { "name": "GitHub Actions", "category": "CI/CD" }
                    ],
                    "languages": [
                      { "language": "Arabic", "level": "Native" },
                      { "language": "English", "level": "Professional" }
                    ],
                    "volunteers": [
                      {
                        "id": "vol_001",
                        "organization": "QA Academy Egypt",
                        "role": "Test Automation Instructor",
                        "startDate": "2020-06",
                        "endDate": null,
                        "description": "Teach advanced Selenium and Playwright courses to 30+ aspiring QA engineers every quarter."
                      },
                      {
                        "id": "vol_002",
                        "organization": "Tech Community Egypt",
                        "role": "Workshop Organizer",
                        "startDate": "2021-01",
                        "endDate": null,
                        "description": "Organize and facilitate monthly QA and testing workshops attracting 100+ participants on emerging testing trends."
                      }
                    ],
                    "hobbies": ["Test automation blogging", "Contributing to Playwright", "Coding challenges", "Mentoring junior QAs"],
                    "other": [
                      { "label": "Open Source", "value": "Active contributor to Playwright and TestCafe projects" },
                      { "label": "Publications", "value": "15+ articles on test automation published on Medium and Dev.to" }
                    ]
                  }
                }
                """;

      var seededResume6 = new Resume
      {
        Id = Guid.NewGuid(),
        UserId = seedUserId,
        TemplateId = seededTemplate.Id,
        Status = ResumeStatus.Completed,
        Name = "QA Automation Engineer — Mona Ibrahim",
        RawData = resumeJson6,
        FinalData = resumeJson6,
        AiAvailable = false,
        JobTitleSuggestionsJson = """[{"title":"QA Automation Engineer","score":10},{"title":"Senior QA Engineer","score":9},{"title":"Test Automation Architect","score":8}]""",
        SkillSuggestionsJson = """["k6","Appium","JMeter","DataDog","BrowserStack"]""",
        CreatedAt = utcNow,
        UpdatedAt = utcNow,
      };

      db.Resumes.Add(seededResume6);
      await db.SaveChangesAsync();
    }
  }
}
