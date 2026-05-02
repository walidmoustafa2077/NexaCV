namespace NexaCV.Api.Models;

public class Template
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IndustryCategory { get; set; }
    public decimal BasePriceUsd { get; set; }
    public bool SupportsWord { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
}
