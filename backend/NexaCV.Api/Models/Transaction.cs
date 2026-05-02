using NexaCV.Api.Enums;

namespace NexaCV.Api.Models;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ResumeId { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal RegenAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    /// <summary>USD → Currency rate captured at checkout time. Used for financial auditing.</summary>
    public decimal ExchangeRateUsed { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? GatewayRefId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public User User { get; set; } = null!;
    public Resume Resume { get; set; } = null!;
}
