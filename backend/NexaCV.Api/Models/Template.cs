namespace NexaCV.Api.Models;

public class Template
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IndustryCategory { get; set; }
    /// <summary>Design category: Executive | Creative | ModernTech</summary>
    public string? StyleCategory { get; set; }
    public decimal BasePriceUsd { get; set; }
    public bool SupportsWord { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    /// <summary>Full HTML content of the template with {{PLACEHOLDER}} tokens.</summary>
    public string? HtmlContent { get; set; }

    public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
}
