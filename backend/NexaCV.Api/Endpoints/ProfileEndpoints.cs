using System.Security.Claims;
using FluentValidation;
using NexaCV.Api.DTOs.Profile;
using NexaCV.Api.Services;

namespace NexaCV.Api.Endpoints;

public static class ProfileEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/profile")
            .WithTags("Profile")
            .RequireAuthorization();

        // ── GET /api/profile/me ────────────────────────────────────────
        group.MapGet("/me", async (
            ICurrentUserContext currentUser,
            IProfileService profileService) =>
        {
            var userId = currentUser.UserId;
            var profile = await profileService.GetProfileAsync(userId);

            if (profile is null)
                return Results.Ok(new { profileExists = false });

            return Results.Ok(profile);
        })
        .WithName("GetMyProfile")
        .WithSummary("Get the current user's NexaCV profile")
        .WithDescription(
            "Returns the profile for the authenticated user. " +
            "If no profile has been created yet (JIT onboarding), returns `{ profileExists: false }`.")
        .Produces<ProfileDto>(200)
        .Produces(200)
        .ProducesProblem(401);

        // ── POST /api/profile ──────────────────────────────────────────
        group.MapPost("/", async (
            CreateProfileRequest req,
            IValidator<CreateProfileRequest> validator,
            ICurrentUserContext currentUser,
            IProfileService profileService) =>
        {
            await validator.ValidateAndThrowAsync(req);
            var profile = await profileService.CreateProfileAsync(currentUser.UserId, req);
            return Results.Created($"/api/profile/me", profile);
        })
        .WithName("CreateProfile")
        .WithSummary("Create a NexaCV profile (JIT onboarding)")
        .WithDescription(
            "Called after a user first authenticates with NexaCV.Identity to create " +
            "their NexaCV-specific profile. The `userId` is extracted from the JWT.")
        .Produces<ProfileDto>(201)
        .ProducesProblem(401)
        .ProducesProblem(422);

        // ── PUT /api/profile/me ────────────────────────────────────────
        group.MapPut("/me", async (
            UpdateProfileRequest req,
            IValidator<UpdateProfileRequest> validator,
            ICurrentUserContext currentUser,
            IProfileService profileService) =>
        {
            await validator.ValidateAndThrowAsync(req);
            var profile = await profileService.UpdateProfileAsync(currentUser.UserId, req);
            return Results.Ok(profile);
        })
        .WithName("UpdateMyProfile")
        .WithSummary("Partially update the current user's profile")
        .WithDescription(
            "Only non-null fields are applied. Null/omitted fields are left unchanged. " +
            "Returns the updated profile.")
        .Produces<ProfileDto>(200)
        .ProducesProblem(401)
        .ProducesProblem(422);
    }
}
