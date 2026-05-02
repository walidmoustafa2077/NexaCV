using Microsoft.AspNetCore.Http;

namespace NexaCV.Api.Services.Payment;

public record PaymentRequest(Guid TransactionId, decimal Amount, string Currency, Guid ResumeId);
public record PaymentSessionResult(string PaymentUrl, string GatewayRefId);

public interface IPaymentGateway
{
    string GatewayName { get; }
    string SupportedCurrency { get; }

    Task<PaymentSessionResult> CreateSessionAsync(PaymentRequest request);

    bool VerifyWebhookSignature(
        HttpRequest request,
        out string eventType,
        out string gatewayRefId);
}
