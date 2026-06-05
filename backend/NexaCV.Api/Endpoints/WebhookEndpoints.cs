using NexaCV.Api.Services;
using NexaCV.Api.Services.Payment;

namespace NexaCV.Api.Endpoints;

public static class WebhookEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/webhooks/payment", async (
            PaymentGatewayFactory gatewayFactory,
            ITransactionService txService,
            HttpContext ctx) =>
        {
            // TryResolveWebhook probes CanHandleRequest (side-effect-free) before calling
            // VerifyWebhookSignature exactly once on the matching gateway (LSP fix).
            var resolved = gatewayFactory.TryResolveWebhook(ctx.Request);

            if (resolved is null)
                return Results.BadRequest(new { error = "No matching payment gateway for this webhook." });

            await txService.FulfillAsync(resolved.Value.GatewayRefId);

            return Results.Ok();
        })
        .WithName("PaymentWebhook")
        .WithSummary("Inbound payment gateway webhook")
        .WithDescription(
            "Receives callbacks from payment gateways after a payment attempt. " +
            "No JWT required — authentication is performed by each gateway's own VerifyWebhookSignature() implementation. " +
            "On a verified success event the transaction status transitions to SUCCESS, " +
            "completedAt is set, and the associated resume transitions to PAID. " +
            "Stub testing: POST with header X-Stub-Ref: <gatewayRefId> where gatewayRefId is the value returned in CheckoutResponse.transactionId.")
        .Produces(200)
        .ProducesProblem(400)
        .WithTags("Webhooks");
    }
}
