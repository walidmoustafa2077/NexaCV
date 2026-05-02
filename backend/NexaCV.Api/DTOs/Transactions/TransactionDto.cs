namespace NexaCV.Api.DTOs.Transactions;

/// <summary>Full detail view of a payment transaction.</summary>
public class TransactionDto
{
    /// <summary>Transaction unique identifier.</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>The resume this transaction is for.</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid ResumeId { get; set; }

    /// <summary>Total amount charged.</summary>
    /// <example>140.00</example>
    public decimal TotalAmount { get; set; }

    /// <summary>Currency of the transaction (ISO 4217 code).</summary>
    /// <example>EGP</example>
    public string Currency { get; set; } = string.Empty;

    /// <summary>USD → target currency rate that was used. Stored for dispute resolution and reporting.</summary>
    /// <example>50.00</example>
    public decimal ExchangeRateUsed { get; set; }

    /// <summary>Payment gateway status. One of: <c>PENDING</c>, <c>SUCCESS</c>, <c>FAILED</c>.</summary>
    /// <example>SUCCESS</example>
    public string PaymentStatus { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the transaction was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC timestamp when the payment was confirmed by the gateway webhook. Null if still pending.</summary>
    public DateTime? CompletedAt { get; set; }
}
