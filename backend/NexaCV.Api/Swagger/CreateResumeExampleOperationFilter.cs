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
            ["schemaVersion"] = new OpenApiString("1.2"),
            ["settings"] = new OpenApiObject
            {
                ["summaryType"] = new OpenApiString("Summary"),
                ["descriptionFormat"] = new OpenApiString("Bulleted"),
                ["tone"] = new OpenApiString("Executive"),
                ["aiFocus"] = new OpenApiString("MetricsDriven"),
                ["narrativeVoice"] = new OpenApiString("ThirdPerson"),
                ["skillsLayout"] = new OpenApiString("Mixed"),
                ["constraints"] = new OpenApiObject
                {
                    ["summaryMaxWords"] = new OpenApiInteger(50),
                    ["experienceItemMaxWords"] = new OpenApiInteger(100),
                    ["projectMaxWords"] = new OpenApiInteger(75)
                }
            },
            ["content"] = new OpenApiObject
            {
                ["targetJobTitle"] = new OpenApiString("Senior Software Engineer"),
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
                            "System worked better and had less downtime."),
                        ["wordCount"]   = new OpenApiInteger(85)
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
                ["projects"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["id"]          = new OpenApiString("prj_001"),
                        ["name"]        = new OpenApiString("NexaCV Engine"),
                        ["role"]        = new OpenApiString("Lead Developer"),
                        ["description"] = new OpenApiString("Developed a .NET 9 based resume generation platform."),
                        ["link"]        = new OpenApiString("github.com/nexacv"),
                        ["technologies"] = new OpenApiArray
                        {
                            new OpenApiString("C#"),
                            new OpenApiString("React")
                        },
                        ["wordCount"] = new OpenApiInteger(45)
                    }
                },
                ["skills"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["name"]     = new OpenApiString("C#"),
                        ["type"]     = new OpenApiString("Technical"),
                        ["category"] = new OpenApiString("Backend")
                    },
                    new OpenApiObject
                    {
                        ["name"]     = new OpenApiString("Team Leadership"),
                        ["type"]     = new OpenApiString("Soft"),
                        ["category"] = new OpenApiString("Management")
                    },
                    new OpenApiObject
                    {
                        ["name"]     = new OpenApiString("System Architecture"),
                        ["type"]     = new OpenApiString("Technical"),
                        ["category"] = new OpenApiString("Design")
                    }
                },
                ["languages"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["language"] = new OpenApiString("Arabic"),
                        ["level"]    = new OpenApiString("Native")
                    },
                    new OpenApiObject
                    {
                        ["language"] = new OpenApiString("English"),
                        ["level"]    = new OpenApiString("Professional")
                    }
                },
                ["hobbies"] = new OpenApiArray
                {
                    new OpenApiString("Photography"),
                    new OpenApiString("Open Source"),
                    new OpenApiString("Marathon Running")
                },
                ["other"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["label"] = new OpenApiString("Military Status"),
                        ["value"] = new OpenApiString("Completed")
                    },
                    new OpenApiObject
                    {
                        ["label"] = new OpenApiString("Driving License"),
                        ["value"] = new OpenApiString("Valid")
                    }
                }
            }
        }
    };
}
