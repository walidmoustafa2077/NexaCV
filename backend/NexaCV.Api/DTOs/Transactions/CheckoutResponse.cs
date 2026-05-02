namespace NexaCV.Api.DTOs.Transactions;

/// <summary>Result of a successful checkout initiation. Redirect the user to <c>PaymentUrl</c> to complete payment.</summary>
public class CheckoutResponse
{
    /// <summary>Internal transaction ID. Use to poll <c>GET /api/transactions/{id}</c>.</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid TransactionId { get; set; }

    /// <summary>Gateway-provided payment URL. Redirect the customer here to complete the transaction.</summary>
    /// <example>https://stub.payment/session/3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public string PaymentUrl { get; set; } = string.Empty;

    /// <summary>Template base price component (in the requested currency).</summary>
    /// <example>120.00</example>
    public decimal BaseAmount { get; set; }

    /// <summary>Sum of all regeneration costs for this resume (in the requested currency).</summary>
    /// <example>20.00</example>
    public decimal RegenAmount { get; set; }

    /// <summary>Total charge: <c>BaseAmount + RegenAmount</c>.</summary>
    /// <example>140.00</example>
    public decimal TotalAmount { get; set; }

    /// <summary>Currency used for this transaction (ISO 4217, e.g. <c>EGP</c>, <c>USD</c>, <c>EUR</c>).</summary>
    /// <example>EGP</example>
    public string Currency { get; set; } = string.Empty;

    /// <summary>USD → target currency exchange rate applied at checkout. Stored for financial auditing.</summary>
    /// <example>50.00</example>
    public decimal ExchangeRateUsed { get; set; }
}
