using NexaCV.Identity.DTOs;
using NexaCV.Identity.Services;
using System.ComponentModel.DataAnnotations;

namespace NexaCV.Identity.Endpoints;

public static class AuthEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/register", async (
            RegisterRequest req,
            IAuthService authService,
            HttpContext ctx) =>
        {
            ValidateRequest(req);
            var ip = GetIpAddress(ctx);
            var userAgent = GetUserAgent(ctx);
            var response = await authService.RegisterAsync(req, ip, userAgent);
            return Results.Created(string.Empty, response);
        })
        .WithName("Register")
        .WithSummary("Register a new user account")
        .WithDescription(
            "Creates a new user account, hashes the password, and immediately returns a signed " +
            "Access Token (15 min) + Refresh Token (7 days) pair. " +
            "Returns **409** if a user with that email already exists. " +
            "All fields are required.")
        .Produces<AuthResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .AllowAnonymous();

        group.MapPost("/login", async (
            LoginRequest req,
            IAuthService authService,
            HttpContext ctx) =>
        {
            ValidateRequest(req);
            var ip = GetIpAddress(ctx);
            var userAgent = GetUserAgent(ctx);
            var response = await authService.LoginAsync(req, ip, userAgent);
            return Results.Ok(response);
        })
        .WithName("Login")
        .WithSummary("Authenticate with email and password")
        .WithDescription(
            "Validates credentials and returns a new Access Token (15 min) + Refresh Token (7 days) pair. " +
            "Updates `lastLogin` on the user record. " +
            "Returns **401** for invalid email or password " +
            "(message is deliberately ambiguous to prevent user enumeration).")
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .AllowAnonymous();

        group.MapPost("/refresh", async (
            TokenRequest req,
            IAuthService authService,
            HttpContext ctx) =>
        {
            ValidateRequest(req);
            var ip = GetIpAddress(ctx);
            var userAgent = GetUserAgent(ctx);
            var response = await authService.RefreshTokenAsync(req.RefreshToken, ip, userAgent);
            return Results.Ok(response);
        })
        .WithName("RefreshToken")
        .WithSummary("Exchange a Refresh Token for a new token pair")
        .WithDescription(
            "Accepts a valid Refresh Token and performs **token rotation**: the supplied token is " +
            "immediately revoked and a fresh Access Token + Refresh Token pair is issued. " +
            "**Reuse detection**: if a revoked token is replayed, all active sessions for that user " +
            "are terminated as a security precaution. " +
            "Returns **401** if the token is invalid, expired, or already revoked.")
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .AllowAnonymous();

        group.MapPost("/revoke", async (
            RevokeTokenRequest req,
            IAuthService authService,
            HttpContext ctx) =>
        {
            ValidateRequest(req);
            var ip = GetIpAddress(ctx);
            var userAgent = GetUserAgent(ctx);
            await authService.RevokeTokenAsync(req.RefreshToken, ip, userAgent);
            return Results.NoContent();
        })
        .WithName("RevokeToken")
        .WithSummary("Revoke a Refresh Token (logout from a device)")
        .WithDescription(
            "Explicitly marks a Refresh Token as revoked. " +
            "Use this on logout or when a device is deregistered. " +
            "The short-lived Access Token remains valid until it naturally expires — " +
            "clients should discard it locally. " +
            "Returns **400** if the token is already inactive.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .AllowAnonymous();
    }

    private static string? GetIpAddress(HttpContext ctx)
    {
        if (ctx.Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
            return forwarded.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();

        return ctx.Connection.RemoteIpAddress?.MapToIPv4().ToString();
    }

    private static string? GetUserAgent(HttpContext ctx)
    {
        if (ctx.Request.Headers.TryGetValue("User-Agent", out var userAgent))
            return userAgent.FirstOrDefault();

        return null;
    }

    private static void ValidateRequest<T>(T? request)
    {
        if (request is null)
            throw new ValidationException("Request body is required.");
            
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        
        if (!Validator.TryValidateObject(request, context, results, validateAllProperties: true))
        {
            var errors = string.Join("; ", results.Select(r => r.ErrorMessage));
            throw new ValidationException($"Request validation failed: {errors}");
        }
    }
}
