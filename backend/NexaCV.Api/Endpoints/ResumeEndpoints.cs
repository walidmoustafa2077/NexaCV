using FluentValidation;
using NexaCV.Api.DTOs.Resumes;
using NexaCV.Api.Services;

namespace NexaCV.Api.Endpoints;

public static class ResumeEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/resumes").WithTags("Resumes").RequireAuthorization();

        group.MapPost("/", async (
            CreateResumeRequest req,
            IValidator<CreateResumeRequest> validator,
            JwtService jwt,
            IResumeService resumeService,
            HttpContext ctx) =>
        {
            await validator.ValidateAndThrowAsync(req);
            var userId = jwt.GetUserIdFromClaims(ctx.User);
            var result = await resumeService.CreateAsync(userId, req);
            return Results.Created($"/api/resumes/{result.Id}", result);
        })
        .WithName("CreateResume")
        .WithSummary("Create a new resume")
        .WithDescription(
            "Accepts wizard form data, passes it through the AI generation pipeline " +
            "(stub in this build — `finalData` mirrors `rawData`, `aiAvailable: false`), " +
            "and transitions the resume from `DRAFT` to `COMPLETED` atomically. " +
            "Returns the full detail DTO including both `rawData` and the AI-produced `finalData`.")
        .Produces<ResumeDetailDto>(201)
        .ProducesProblem(401)
        .ProducesProblem(404)
        .ProducesProblem(422);

        group.MapGet("/", async (JwtService jwt, IResumeService resumeService, HttpContext ctx) =>
        {
            var userId = jwt.GetUserIdFromClaims(ctx.User);
            var resumes = await resumeService.GetAllByUserAsync(userId);
            return Results.Ok(resumes);
        })
        .WithName("GetMyResumes")
        .WithSummary("List all resumes for the current user")
        .WithDescription(
            "Returns a summary list of all non-deleted resumes belonging to the authenticated user. " +
            "Soft-deleted resumes are excluded automatically via the global EF query filter. " +
            "Use `GET /api/resumes/{id}` to retrieve full detail including `finalData`.")
        .Produces<List<ResumeSummaryDto>>(200)
        .ProducesProblem(401);

        group.MapGet("/{id:guid}", async (
            Guid id,
            JwtService jwt,
            IResumeService resumeService,
            HttpContext ctx) =>
        {
            var userId = jwt.GetUserIdFromClaims(ctx.User);
            var resume = await resumeService.GetByIdAsync(id, userId);
            return Results.Ok(resume);
        })
        .WithName("GetResumeById")
        .WithSummary("Get a single resume by ID")
        .WithDescription(
            "Returns full resume detail including `rawData`, `finalData`, and `aiAvailable`. " +
            "Returns **403** if the resume belongs to a different user. " +
            "Returns **404** if the resume does not exist or has been soft-deleted.")
        .Produces<ResumeDetailDto>(200)
        .ProducesProblem(401)
        .ProducesProblem(403)
        .ProducesProblem(404);

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateFinalDataRequest req,
            JwtService jwt,
            IResumeService resumeService,
            HttpContext ctx) =>
        {
            var userId = jwt.GetUserIdFromClaims(ctx.User);
            var resume = await resumeService.UpdateFinalDataAsync(id, userId, req.FinalData.GetRawText());
            return Results.Ok(resume);
        })
        .WithName("UpdateResumeFinalData")
        .WithSummary("Replace the resume's final data JSON")
        .WithDescription(
            "Allows the user to manually edit the AI-produced `finalData` JSON. " +
            "Sets `updatedAt` to UTC now. " +
            "Returns **403** if the resume belongs to a different user.")
        .Produces<ResumeDetailDto>(200)
        .ProducesProblem(401)
        .ProducesProblem(403);

        group.MapDelete("/{id:guid}", async (
            Guid id,
            JwtService jwt,
            IResumeService resumeService,
            HttpContext ctx) =>
        {
            var userId = jwt.GetUserIdFromClaims(ctx.User);
            await resumeService.DeleteAsync(id, userId);
            return Results.NoContent();
        })
        .WithName("DeleteResume")
        .WithSummary("Soft-delete a resume")
        .WithDescription(
            "Marks the resume as deleted (`isDeleted = true`). Hard deletes are forbidden to preserve " +
            "transaction history for accounting and chargebacks. " +
            "Returns **400** if the resume status is `PAID` — paid resumes cannot be deleted. " +
            "Returns **403** if the resume belongs to a different user.")
        .Produces(204)
        .ProducesProblem(400)
        .ProducesProblem(401)
        .ProducesProblem(403);

        group.MapPost("/{id:guid}/regenerate", async (
            Guid id,
            RegenerateRequest req,
            IValidator<RegenerateRequest> validator,
            JwtService jwt,
            IRegenerationService regenService,
            HttpContext ctx) =>
        {
            await validator.ValidateAndThrowAsync(req);
            var userId = jwt.GetUserIdFromClaims(ctx.User);
            var result = await regenService.RegenerateAsync(id, userId, req);
            return Results.Ok(result);
        })
        .WithName("RegenerateSection")
        .WithSummary("Regenerate a single resume section with AI")
        .WithDescription(
            "Calls the AI service to rewrite one section of `finalData`. " +
            "The section is identified by `sectionIdentifier` (a key in the `finalData` JSON, e.g. `SUMMARY`). " +
            "Each call costs **EGP 10 / USD 0.25** and is recorded as a `Regeneration` row. " +
            "A maximum of **3 regenerations per section** is enforced — the 4th call returns **429**. " +
            "Returns the new content, updated counters, and added cost. " +
            "`aiAvailable: false` while the stub AI is active (content = echo of the user prompt).")
        .Produces<RegenerateResponse>(200)
        .ProducesProblem(401)
        .ProducesProblem(403)
        .ProducesProblem(429);

        group.MapGet("/{id:guid}/download", async (
            Guid id,
            string? format,
            JwtService jwt,
            IResumeService resumeService,
            HttpContext ctx) =>
        {
            var userId = jwt.GetUserIdFromClaims(ctx.User);
            var fmt = format ?? "pdf";
            var ip = ctx.Connection.RemoteIpAddress?.ToString();

            // Validate ownership and paid status — records download attempt
            _ = await resumeService.GetForDownloadAsync(id, userId, fmt, ip);

            // PDF/DOCX rendering deferred
            return Results.StatusCode(501);
        })
        .WithName("DownloadResume")
        .WithSummary("Download a paid resume as PDF or DOCX (not yet implemented)")
        .WithDescription(
            "**Status: 501 Not Implemented.** " +
            "This endpoint validates ownership and payment status, records the download attempt, " +
            "and will stream the rendered file once a PDF renderer (QuestPDF) is wired in. " +
            "Use the `format` query parameter: `pdf` (default) or `docx`. " +
            "Returns **400** if `docx` is requested for a template that does not support Word export. " +
            "Returns **403** if the resume is not `PAID` or belongs to another user.")
        .Produces(501)
        .ProducesProblem(400)
        .ProducesProblem(401)
        .ProducesProblem(403);
    }
}


