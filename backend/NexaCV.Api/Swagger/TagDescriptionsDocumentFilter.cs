using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NexaCV.Api.Swagger;

/// <summary>
/// Appends human-readable descriptions to every Swagger tag (API group).
/// </summary>
public class TagDescriptionsDocumentFilter : IDocumentFilter
{
    private static readonly Dictionary<string, string> TagDescriptions = new()
    {
        ["Profile"] =
            "Manage the profile of the currently authenticated user. " +
            "All routes require a valid `Bearer` token.",

        ["Templates"] =
            "Read-only catalogue of resume templates available for purchase. " +
            "No authentication required. " +
            "Filter by `industryCategory` (e.g. `Corporate`, `Creative`) to narrow results.",

        ["Resumes"] =
            "Full lifecycle management of a user's resumes. " +
            "Creating a resume runs it through the AI pipeline (stub in this build) and transitions status to `COMPLETED`. " +
            "Regeneration is capped at **3 calls per section** and incurs a cost (EGP 10 / USD 0.25 each). " +
            "Resumes are **soft-deleted** — paid resumes cannot be deleted. " +
            "Download is scaffolded (returns 501) until a PDF renderer is wired in.",

        ["Transactions"] =
            "Payment flow for a completed resume. " +
            "Checkout calculates `base price + total regeneration cost`, creates a `PENDING` transaction, " +
            "and delegates to the active `IPaymentGateway` for a payment URL. " +
            "The stub gateway returns a local URL immediately.",

        ["Webhooks"] =
            "Inbound callbacks from payment gateways. " +
            "No JWT required — gateway identity is verified by `VerifyWebhookSignature()`. " +
            "On success the transaction is marked `SUCCESS` and the resume status transitions to `PAID`."
    };

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Tags ??= new List<OpenApiTag>();

        foreach (var (name, description) in TagDescriptions)
        {
            var existing = swaggerDoc.Tags.FirstOrDefault(t => t.Name == name);
            if (existing is not null)
                existing.Description = description;
            else
                swaggerDoc.Tags.Add(new OpenApiTag { Name = name, Description = description });
        }

        // Sort tags alphabetically for consistent display
        swaggerDoc.Tags = swaggerDoc.Tags.OrderBy(t => t.Name).ToList();
    }
}
