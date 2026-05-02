using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using NexaCV.Api.DTOs.Errors;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NexaCV.Api.Swagger;

/// <summary>
/// Replaces the generic ProblemDetails schema (with additionalProp1/2/3) on all 4xx/5xx
/// responses with the actual error shapes returned by ExceptionMiddleware:
///   - 422 → <see cref="ValidationErrorResponse"/> (includes per-field details array)
///   - All other 4xx/5xx → <see cref="ApiErrorResponse"/> ({ status, error })
/// Also normalises the content type from application/problem+json to application/json
/// to match what the middleware actually writes, and attaches a per-status-code example.
/// </summary>
public class ErrorSchemaOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        foreach (var (statusCode, response) in operation.Responses)
        {
            if (!int.TryParse(statusCode, out var code) || code < 400)
                continue;

            var errorType = code == 422
                ? typeof(ValidationErrorResponse)
                : typeof(ApiErrorResponse);

            var schema = context.SchemaGenerator.GenerateSchema(errorType, context.SchemaRepository);
            var mediaType = new OpenApiMediaType
            {
                Schema = schema,
                Example = BuildExample(code)
            };

            response.Content.Remove("application/problem+json");
            response.Content["application/json"] = mediaType;
        }
    }

    private static IOpenApiAny BuildExample(int code) => code switch
    {
        400 => Error(400, "Bad request."),
        401 => Error(401, "Unauthorized."),
        403 => Error(403, "Access denied."),
        404 => Error(404, "Resource not found."),
        409 => Error(409, "Conflict — resource already exists."),
        422 => new OpenApiObject
        {
            ["status"]  = new OpenApiInteger(422),
            ["error"]   = new OpenApiString("Validation failed"),
            ["details"] = new OpenApiArray
            {
                new OpenApiObject
                {
                    ["field"]   = new OpenApiString("email"),
                    ["message"] = new OpenApiString("'Email' must not be empty.")
                }
            }
        },
        429 => Error(429, "Section regeneration limit reached (max 3 per section)."),
        500 => Error(500, "Internal server error."),
        _   => Error(code, "Error.")
    };

    private static OpenApiObject Error(int status, string message) => new()
    {
        ["status"] = new OpenApiInteger(status),
        ["error"]  = new OpenApiString(message)
    };
}
