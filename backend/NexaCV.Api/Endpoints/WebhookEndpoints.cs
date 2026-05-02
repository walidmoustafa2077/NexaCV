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
            IPaymentGateway gateway;
            try
            {
                gateway = gatewayFactory.ResolveByRequest(ctx.Request);
            }
            catch (InvalidOperationException)
            {
                return Results.BadRequest(new { error = "No matching payment gateway for this webhook." });
            }

            if (!gateway.VerifyWebhookSignature(ctx.Request, out _, out var gatewayRefId))
                return Results.BadRequest(new { error = "Invalid webhook signature." });

            await txService.FulfillAsync(gatewayRefId);

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
