namespace NexaCV.Api.Models;

public class Regeneration
{
    public Guid Id { get; set; }
    public Guid ResumeId { get; set; }
    public string SectionIdentifier { get; set; } = string.Empty;
    public string? UserPrompt { get; set; }
    public decimal CostUsd { get; set; }
    public DateTime CreatedAt { get; set; }

    public Resume Resume { get; set; } = null!;
}
