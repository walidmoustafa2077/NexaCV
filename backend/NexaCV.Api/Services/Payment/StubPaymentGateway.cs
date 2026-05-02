using Microsoft.AspNetCore.Http;

namespace NexaCV.Api.Services.Payment;

public class StubPaymentGateway : IPaymentGateway
{
    public string GatewayName => "Stub";
    public string SupportedCurrency => "*";

    public Task<PaymentSessionResult> CreateSessionAsync(PaymentRequest request)
    {
        var url = $"https://stub.payment/session/{request.TransactionId}";
        var refId = request.TransactionId.ToString();
        return Task.FromResult(new PaymentSessionResult(url, refId));
    }

    public bool VerifyWebhookSignature(HttpRequest request, out string eventType, out string gatewayRefId)
    {
        eventType = "checkout.completed";
        gatewayRefId = request.Headers.TryGetValue("X-Stub-Ref", out var val)
            ? val.ToString()
            : string.Empty;

        return !string.IsNullOrEmpty(gatewayRefId);
    }
}
