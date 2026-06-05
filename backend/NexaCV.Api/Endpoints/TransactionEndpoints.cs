using NexaCV.Api.DTOs.Transactions;
using NexaCV.Api.Services;

namespace NexaCV.Api.Endpoints;

public static class TransactionEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/transactions").WithTags("Transactions").RequireAuthorization();

        group.MapPost("/checkout", async (
            CheckoutRequest req,
            ICurrentUserContext currentUser,
            ITransactionService txService) =>
        {
            var response = await txService.CheckoutAsync(req.ResumeId, currentUser.UserId, req.Currency);
            return Results.Ok(response);
        })
        .WithName("Checkout")
        .WithSummary("Initiate payment checkout for a resume")
        .WithDescription(
            "Creates a PENDING transaction and delegates to the active payment gateway to produce a paymentUrl. " +
            "The total is calculated as: base template price + sum of all regeneration costs for the resume. " +
            "Resume must be in COMPLETED status — DRAFT or PAID resumes return 400. " +
            "Supply currency as EGP or USD to select pricing. " +
            "The stub gateway returns an immediate local URL for testing.")
        .Produces<CheckoutResponse>(200)
        .ProducesProblem(400)
        .ProducesProblem(401)
        .ProducesProblem(403);

        group.MapGet("/{id:guid}", async (
            Guid id,
            ICurrentUserContext currentUser,
            ITransactionService txService) =>
        {
            var tx = await txService.GetByIdAsync(id, currentUser.UserId);
            return Results.Ok(tx);
        })
        .WithName("GetTransactionById")
        .WithSummary("Get a transaction by ID")
        .WithDescription(
            "Returns the full detail of a payment transaction. " +
            "Poll this endpoint to check if paymentStatus has transitioned to SUCCESS. " +
            "Returns 403 if the transaction belongs to a different user. " +
            "Returns 404 if the transaction does not exist.")
        .Produces<TransactionDto>(200)
        .ProducesProblem(401)
        .ProducesProblem(403)
        .ProducesProblem(404);
    }
}
