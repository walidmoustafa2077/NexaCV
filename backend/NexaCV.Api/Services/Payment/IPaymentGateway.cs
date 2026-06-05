using Microsoft.AspNetCore.Http;

namespace NexaCV.Api.Services.Payment;

public record PaymentRequest(Guid TransactionId, decimal Amount, string Currency, Guid ResumeId);
public record PaymentSessionResult(string PaymentUrl, string GatewayRefId);

/// <summary>
/// Handles creating a payment checkout session.
/// Segregated from <see cref="IWebhookVerifier"/> so that callers that only initiate
/// payments (e.g. <c>TransactionService</c>) do not depend on webhook verification (ISP).
/// </summary>
public interface IPaymentSessionCreator
{
    string GatewayName { get; }
    string SupportedCurrency { get; }
    Task<PaymentSessionResult> CreateSessionAsync(PaymentRequest request);
}

/// <summary>
/// Handles verifying inbound payment webhook callbacks.
/// Segregated from <see cref="IPaymentSessionCreator"/> so that webhook-only gateways
/// do not need to implement session creation (ISP).
/// <para>
/// <see cref="CanHandleRequest"/> is a lightweight probe used by <see cref="PaymentGatewayFactory"/>
/// to identify the correct gateway <em>without side effects</em>, avoiding the LSP violation
/// of calling <see cref="VerifyWebhookSignature"/> on every gateway just to find a match.
/// </para>
/// </summary>
public interface IWebhookVerifier
{
    string GatewayName { get; }

    /// <summary>Lightweight, side-effect-free check to determine whether this gateway owns the request.</summary>
    bool CanHandleRequest(HttpRequest request);

    bool VerifyWebhookSignature(
        HttpRequest request,
        out string eventType,
        out string gatewayRefId);
}

/// <summary>
/// Composite marker interface for gateways that handle both payment session creation
/// and webhook verification. Implement this on concrete gateway classes; inject
/// <see cref="IPaymentSessionCreator"/> or <see cref="IWebhookVerifier"/> at call sites.
/// </summary>
public interface IPaymentGateway : IPaymentSessionCreator, IWebhookVerifier { }
