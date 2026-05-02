namespace NexaCV.Api.DTOs.Transactions;

/// <summary>Initiates a payment session for a <c>COMPLETED</c> resume.</summary>
public class CheckoutRequest
{
    /// <summary>ID of the resume to purchase. Must belong to the authenticated user and have status <c>COMPLETED</c>.</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid ResumeId { get; set; }

    /// <summary>Currency for this transaction. Accepted values: <c>EGP</c> or <c>USD</c>.</summary>
    /// <example>EGP</example>
    public string Currency { get; set; } = string.Empty;
}
