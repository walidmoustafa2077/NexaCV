using NexaCV.Api.DTOs.Templates;
using NexaCV.Api.Services;

namespace NexaCV.Api.Endpoints;

public static class TemplateEndpoints
{
    // Sample resume data used to populate template previews in the gallery.
    private const string SampleResumeJson = """
        {
          "settings": { "summaryType": "Paragraph", "descriptionFormat": "BulletPoints" },
          "content": {
            "personal": {
              "firstName": "Alexandra",
              "lastName": "Harrison",
              "jobTitle": "Senior Product Manager",
              "email": "alex.harrison@email.com",
              "phone": "+1 (555) 234-5678",
              "location": "San Francisco, CA",
              "linkedinUrl": "linkedin.com/in/alexharrison",
              "siteUrl": "alexharrison.dev"
            },
            "summary": "Accomplished product leader with 8+ years of experience driving cross-functional teams to deliver innovative, user-centric products. Proven track record in agile environments, data-driven decision-making, and scaling B2B SaaS platforms from concept to market.",
            "experience": [
              {
                "id": "exp_001",
                "title": "Senior Product Manager",
                "company": "Nexus Technologies",
                "startDate": "2021-03",
                "endDate": null,
                "description": "Led a 12-person cross-functional team to launch three major product lines, achieving 40% YoY revenue growth. Drove OKR planning and quarterly roadmaps aligning engineering, design, and marketing."
              },
              {
                "id": "exp_002",
                "title": "Product Manager",
                "company": "Brightwave Analytics",
                "startDate": "2018-06",
                "endDate": "2021-02",
                "description": "Owned the full product lifecycle for an enterprise analytics dashboard serving 200+ clients. Reduced churn by 25% through targeted feature development informed by user research and NPS data."
              },
              {
                "id": "exp_003",
                "title": "Associate Product Manager",
                "company": "LaunchPad Ventures",
                "startDate": "2016-09",
                "endDate": "2018-05",
                "description": "Collaborated with engineering and design to ship 15 product iterations in 18 months. Conducted A/B testing and user interviews to validate feature hypotheses."
              }
            ],
            "education": [
              {
                "id": "edu_001",
                "institution": "University of California, Berkeley",
                "degree": "Bachelor of Science",
                "fieldOfStudy": "Computer Science",
                "grade": "3.8 GPA",
                "startDate": "2012-09",
                "endDate": "2016-05"
              }
            ],
            "courses": [
              {
                "id": "crs_001",
                "name": "Certified Scrum Product Owner",
                "provider": "Scrum Alliance",
                "date": "2020-04"
              },
              {
                "id": "crs_002",
                "name": "AWS Certified Solutions Architect",
                "provider": "Amazon Web Services",
                "date": "2022-11"
              }
            ],
            "skills": ["Product Strategy", "Agile / Scrum", "Data Analytics", "Stakeholder Management", "SQL", "Figma", "Roadmap Planning", "User Research", "A/B Testing", "Python"]
          }
        }
        """;

    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/templates").WithTags("Templates");

        group.MapGet("/", async (ITemplateService templateService, string? industryCategory) =>
        {
            var templates = await templateService.GetAllAsync(industryCategory);
            return Results.Ok(templates);
        })
        .WithName("GetTemplates")
        .WithSummary("List all active templates")
        .WithDescription(
            "Returns all active resume templates. " +
            "Optionally filter by `industryCategory` query parameter (e.g. `Corporate`, `Creative`). " +
            "No authentication required.")
        .Produces<List<TemplateDto>>(200);

        group.MapGet("/{id:int}", async (int id, ITemplateService templateService) =>
        {
            var template = await templateService.GetByIdAsync(id);
            return Results.Ok(template);
        })
        .WithName("GetTemplateById")
        .WithSummary("Get a single template by ID")
        .WithDescription(
            "Returns the full detail of a single active template. " +
            "Returns **404** if the template does not exist or is inactive. " +
            "No authentication required.")
        .Produces<TemplateDto>(200)
        .ProducesProblem(404);

        group.MapGet("/{id:int}/preview", async (
            int id,
            ITemplateService templateService,
            ITemplateRendererService renderer) =>
        {
            var html = await templateService.GetHtmlContentAsync(id);
            if (html is null or "") return Results.NotFound();
            var rendered = renderer.Render(html, SampleResumeJson);
            return Results.Content(rendered, "text/html; charset=utf-8");
        })
        .WithName("GetTemplatePreview")
        .WithSummary("Render a template with sample data")
        .WithDescription(
            "Returns a fully rendered HTML page for the specified template, " +
            "populated with sample resume data. Intended for use in thumbnail previews. " +
            "No authentication required.")
        .Produces<string>(200, "text/html")
        .ProducesProblem(404);
    }
}

