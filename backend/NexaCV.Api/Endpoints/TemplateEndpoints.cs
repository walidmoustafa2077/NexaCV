using NexaCV.Api.DTOs.Templates;
using NexaCV.Api.Services;

namespace NexaCV.Api.Endpoints;

public static class TemplateEndpoints
{
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
            "No authentication required. " +
            "Templates are seeded on startup: **Modern Minimalist** (Corporate), **Creative**, **Executive** (Corporate).")
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
    }
}
