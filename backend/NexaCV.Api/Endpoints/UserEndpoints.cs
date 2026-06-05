using NexaCV.Api.DTOs.Users;
using NexaCV.Api.Services;

namespace NexaCV.Api.Endpoints;

public static class UserEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users").RequireAuthorization();

        group.MapGet("/me", async (ICurrentUserContext currentUser, IUserService userService) =>
        {
            var profile = await userService.GetProfileAsync(currentUser.UserId);
            return Results.Ok(profile);
        })
        .WithName("GetMyProfile")
        .WithSummary("Get the current user's profile")
        .WithDescription(
            "Returns the full profile of the authenticated user. " +
            "The response **never** includes the password hash. " +
            "Requires a valid `Bearer` token.")
        .Produces<UserProfileDto>(200)
        .ProducesProblem(401);

        group.MapPut("/me", async (UpdateUserRequest req, ICurrentUserContext currentUser, IUserService userService) =>
        {
            var profile = await userService.UpdateProfileAsync(currentUser.UserId, req);
            return Results.Ok(profile);
        })
        .WithName("UpdateMyProfile")
        .WithSummary("Update the current user's profile")
        .WithDescription(
            "Partial update: only non-null fields in the request body are applied. " +
            "Sending `password` re-hashes it with BCrypt and logs a `PASSWORD_UPDATED` movement. " +
            "Returns the updated profile. Requires a valid `Bearer` token.")
        .Produces<UserProfileDto>(200)
        .ProducesProblem(401);
    }
}
