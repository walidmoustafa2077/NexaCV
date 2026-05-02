using Microsoft.AspNetCore.Http;

namespace NexaCV.Api.Services.Payment;

public class PaymentGatewayFactory
{
    private readonly IEnumerable<IPaymentGateway> _gateways;

    public PaymentGatewayFactory(IEnumerable<IPaymentGateway> gateways)
    {
        _gateways = gateways;
    }

    public IPaymentGateway Resolve(string currency)
    {
        var gateway = _gateways.FirstOrDefault(g =>
            g.SupportedCurrency == currency || g.SupportedCurrency == "*");

        return gateway ?? throw new InvalidOperationException(
            $"No payment gateway registered for currency '{currency}'.");
    }

    public IPaymentGateway ResolveByRequest(HttpRequest request)
    {
        foreach (var gateway in _gateways)
        {
            if (gateway.VerifyWebhookSignature(request, out _, out _))
                return gateway;
        }

        throw new InvalidOperationException("No payment gateway matched the incoming webhook request.");
    }
}
