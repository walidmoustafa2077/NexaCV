using Microsoft.AspNetCore.Http;

namespace NexaCV.Api.Services.Payment;

public class StubPaymentGateway : IPaymentGateway
{
    public string GatewayName => "Stub";
    public string SupportedCurrency => "*";

    /// <inheritdoc/>
    /// <remarks>Identifies stub webhook requests by the presence of the <c>X-Stub-Ref</c> header.</remarks>
    public bool CanHandleRequest(HttpRequest request)
        => request.Headers.ContainsKey("X-Stub-Ref");

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
