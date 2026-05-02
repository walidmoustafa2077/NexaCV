using System.Net;
using System.Text.Json;
using FluentValidation;
using NexaCV.Api.Services;

namespace NexaCV.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(ctx, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext ctx, Exception ex)
    {
        ctx.Response.ContentType = "application/json";

        object response;

        switch (ex)
        {
            case ValidationException ve:
                ctx.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                response = new
                {
                    status = 422,
                    error = "Validation failed",
                    details = ve.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
                };
                break;

            case UnauthorizedAccessException:
                ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response = new { status = 401, error = ex.Message };
                break;

            case ForbiddenException:
                ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                response = new { status = 403, error = ex.Message };
                break;

            case KeyNotFoundException:
                ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = new { status = 404, error = ex.Message };
                break;

            case ConflictException:
                ctx.Response.StatusCode = (int)HttpStatusCode.Conflict;
                response = new { status = 409, error = ex.Message };
                break;

            case TooManyRegenerationsException:
                ctx.Response.StatusCode = 429;
                response = new { status = 429, error = ex.Message };
                break;

            case InvalidOperationException:
                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new { status = 400, error = ex.Message };
                break;

            case Microsoft.AspNetCore.Http.BadHttpRequestException:
            case System.Text.Json.JsonException:
                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new { status = 400, error = "Invalid request: malformed JSON or missing Content-Type header." };
                break;

            default:
                _logger.LogError(ex, "Unhandled exception");
                ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response = new { status = 500, error = "Internal server error" };
                break;
        }

        await ctx.Response.WriteAsync(JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
