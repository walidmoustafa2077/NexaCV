using FluentValidation;
using NexaCV.Api.DTOs.Auth;
using NexaCV.Api.Services;

namespace NexaCV.Api.Endpoints;

public static class AuthEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (
            RegisterRequest req,
            IValidator<RegisterRequest> validator,
            IAuthService authService,
            HttpContext ctx) =>
        {
            await validator.ValidateAndThrowAsync(req);
            var ip = ctx.Connection.RemoteIpAddress?.ToString();
            var ua = ctx.Request.Headers.UserAgent.ToString();

            var response = await authService.RegisterAsync(req, ip, ua);
            return Results.Created(string.Empty, response);
        })
        .WithName("Register")
        .WithSummary("Register a new user")
        .WithDescription(
            "Creates a new user account and returns a signed JWT on success. " +
            "All fields are required except `dateOfBirth`. " +
            "Password must be at least 8 characters and contain one special character. " +
            "Returns **409** if the email or username is already taken. " +
            "Returns **422** with a `details` array if validation fails.")
        .Produces<AuthResponse>(201)
        .ProducesProblem(409)
        .ProducesProblem(422);

        group.MapPost("/login", async (
            LoginRequest req,
            IValidator<LoginRequest> validator,
            IAuthService authService,
            HttpContext ctx) =>
        {
            await validator.ValidateAndThrowAsync(req);
            var ip = ctx.Connection.RemoteIpAddress?.ToString();
            var ua = ctx.Request.Headers.UserAgent.ToString();

            var response = await authService.LoginAsync(req, ip, ua);
            return Results.Ok(response);
        })
        .WithName("Login")
        .WithSummary("Login with email and password")
        .WithDescription(
            "Validates credentials, updates `lastLogin` on the user record, " +
            "logs a `LOGIN` movement, and returns a signed 24-hour JWT. " +
            "Returns **401** for invalid email or password (message is deliberately ambiguous to prevent user enumeration).")
        .Produces<AuthResponse>(200)
        .ProducesProblem(401)
        .ProducesProblem(422);

        group.MapPost("/logout", async (ICurrentUserContext currentUser, IAuthService authService) =>
        {
            await authService.LogoutAsync(currentUser.UserId);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("Logout")
        .WithSummary("Logout the current user")
        .WithDescription(
            "Logs a `LOGOUT` movement for audit purposes. " +
            "The JWT is **not** invalidated server-side (stateless). " +
            "Clients should discard the token after calling this endpoint. " +
            "Requires a valid `Bearer` token.")
        .Produces(204)
        .ProducesProblem(401);
    }
}
