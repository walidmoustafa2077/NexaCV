using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NexaCV.Api.Swagger;

/// <summary>
/// Replaces the default schema-derived example on <c>POST /api/resumes</c> with a
/// realistic payload that mirrors the <c>Mocks/SampleRawData.json</c> test fixture,
/// so Swagger UI shows real data instead of <c>"string"</c> / <c>0</c> placeholders.
/// </summary>
public class CreateResumeExampleOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Summary != "Create a new resume") return;
        if (operation.RequestBody?.Content?.TryGetValue("application/json", out var media) != true) return;
        if (media is null) return;

        media.Example = BuildExample();
    }

    private static OpenApiObject BuildExample() => new()
    {
        ["templateId"] = new OpenApiInteger(1),
        ["rawData"] = new OpenApiObject
        {
            ["settings"] = new OpenApiObject
            {
                ["summaryType"] = new OpenApiString("Summary"),
                ["descriptionFormat"] = new OpenApiString("Bulleted")
            },
            ["content"] = new OpenApiObject
            {
                ["personal"] = new OpenApiObject
                {
                    ["firstName"] = new OpenApiString("John"),
                    ["middleName"] = new OpenApiString("Alexander"),
                    ["lastName"] = new OpenApiString("Doe"),
                    ["email"] = new OpenApiString("john.doe@example.com"),
                    ["phone"] = new OpenApiString("+201012345678"),
                    ["location"] = new OpenApiString("Cairo, Egypt"),
                    ["zipCode"] = new OpenApiString("11511"),
                    ["dateOfBirth"] = new OpenApiString("1995-05-15"),
                    ["linkedinUrl"] = new OpenApiString("linkedin.com/in/johndoe")
                },
                ["summary"] = new OpenApiString(
                    "Software engineer with 5 years of experience. Worked on web apps and APIs. " +
                    "Good at solving problems and working in teams."),
                ["experience"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["id"]          = new OpenApiString("exp_001"),
                        ["title"]       = new OpenApiString("Senior Software Engineer"),
                        ["company"]     = new OpenApiString("TechFlow Systems"),
                        ["startDate"]   = new OpenApiString("2021-03"),
                        ["endDate"]     = new OpenApiString("2023-10"),
                        ["description"] = new OpenApiString(
                            "Led backend team to migrate legacy APIs to microservices. " +
                            "System worked better and had less downtime.")
                    },
                    new OpenApiObject
                    {
                        ["id"]          = new OpenApiString("exp_002"),
                        ["title"]       = new OpenApiString("Software Developer"),
                        ["company"]     = new OpenApiString("Nexus Digital"),
                        ["startDate"]   = new OpenApiString("2019-07"),
                        ["endDate"]     = new OpenApiString("2021-02"),
                        ["description"] = new OpenApiString(
                            "Built REST APIs and worked on database queries. " +
                            "Also fixed bugs and helped with code reviews.")
                    }
                },
                ["education"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["id"]           = new OpenApiString("edu_001"),
                        ["institution"]  = new OpenApiString("Cairo University"),
                        ["degree"]       = new OpenApiString("B.Sc. in Computer Science"),
                        ["fieldOfStudy"] = new OpenApiString("Computer Science"),
                        ["grade"]        = new OpenApiString("3.7 / 4.0"),
                        ["startDate"]    = new OpenApiString("2015-09"),
                        ["endDate"]      = new OpenApiString("2019-06")
                    }
                },
                ["courses"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["id"]       = new OpenApiString("crs_001"),
                        ["name"]     = new OpenApiString("Cloud Computing Architecture"),
                        ["provider"] = new OpenApiString("Coursera"),
                        ["date"]     = new OpenApiString("2023-01")
                    },
                    new OpenApiObject
                    {
                        ["id"]       = new OpenApiString("crs_002"),
                        ["name"]     = new OpenApiString("Advanced React Patterns"),
                        ["provider"] = new OpenApiString("Frontend Masters"),
                        ["date"]     = new OpenApiString("2022-08")
                    }
                },
                ["skills"] = new OpenApiArray
                {
                    new OpenApiString("C#"),
                    new OpenApiString(".NET 9"),
                    new OpenApiString("ASP.NET Core"),
                    new OpenApiString("React"),
                    new OpenApiString("SQL Server"),
                    new OpenApiString("Azure"),
                    new OpenApiString("Docker")
                }
            }
        }
    };
}
