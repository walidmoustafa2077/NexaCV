using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NexaCV.Identity.Swagger;

/// <summary>
/// Appends human-readable descriptions to every Swagger tag (API group).
/// </summary>
public sealed class TagDescriptionsDocumentFilter : IDocumentFilter
{
    private static readonly Dictionary<string, string> TagDescriptions = new()
    {
        ["Auth"] =
            "All identity operations: registration, login, token refresh, and revocation.\n\n" +
            "**Token lifecycle:**\n" +
            "- `register` / `login` → returns an **Access Token** (15 min) + **Refresh Token** (7 days).\n" +
            "- `refresh` → rotates the Refresh Token; old token is immediately revoked.\n" +
            "- `revoke` → explicitly invalidates a Refresh Token (logout from a device).\n\n" +
            "**Reuse detection:** replaying a previously revoked Refresh Token causes all active sessions " +
            "for that user to be terminated instantly."
    };

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Tags ??= new List<OpenApiTag>();

        foreach (var (tag, description) in TagDescriptions)
        {
            var existing = swaggerDoc.Tags.FirstOrDefault(t => t.Name == tag);
            if (existing is not null)
                existing.Description = description;
            else
                swaggerDoc.Tags.Add(new OpenApiTag { Name = tag, Description = description });
        }
    }
}
