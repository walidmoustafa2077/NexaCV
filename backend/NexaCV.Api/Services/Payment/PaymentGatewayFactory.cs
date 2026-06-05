using Microsoft.AspNetCore.Http;

namespace NexaCV.Api.Services.Payment;

public class PaymentGatewayFactory
{
    private readonly IEnumerable<IPaymentSessionCreator> _creators;
    private readonly IEnumerable<IWebhookVerifier> _verifiers;

    public PaymentGatewayFactory(
        IEnumerable<IPaymentSessionCreator> creators,
        IEnumerable<IWebhookVerifier> verifiers)
    {
        _creators = creators;
        _verifiers = verifiers;
    }

    /// <summary>Resolves a payment session creator for the given currency.</summary>
    public IPaymentSessionCreator Resolve(string currency)
    {
        var creator = _creators.FirstOrDefault(g =>
            g.SupportedCurrency == currency || g.SupportedCurrency == "*");

        return creator ?? throw new InvalidOperationException(
            $"No payment gateway registered for currency '{currency}'.");
    }

    /// <summary>
    /// Identifies the correct verifier for an inbound webhook and verifies the signature
    /// in a single pass. Each gateway is first probed with the side-effect-free
    /// <see cref="IWebhookVerifier.CanHandleRequest"/> before the real verification call,
    /// eliminating the LSP violation of invoking <c>VerifyWebhookSignature</c> on every
    /// registered gateway just to find a match.
    /// Returns <c>null</c> when no gateway claims the request or signature is invalid.
    /// </summary>
    public (IWebhookVerifier Verifier, string EventType, string GatewayRefId)? TryResolveWebhook(HttpRequest request)
    {
        foreach (var verifier in _verifiers)
        {
            if (!verifier.CanHandleRequest(request))
                continue;

            if (verifier.VerifyWebhookSignature(request, out var eventType, out var gatewayRefId))
                return (verifier, eventType, gatewayRefId);
        }

        return null;
    }
}
