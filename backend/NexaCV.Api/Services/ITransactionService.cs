using NexaCV.Api.DTOs.Transactions;

namespace NexaCV.Api.Services;

public interface ITransactionService
{
    Task<CheckoutResponse> CheckoutAsync(Guid resumeId, Guid userId, string currency);
    Task<TransactionDto> GetByIdAsync(Guid txId, Guid userId);
    Task FulfillAsync(string gatewayRefId);
}
