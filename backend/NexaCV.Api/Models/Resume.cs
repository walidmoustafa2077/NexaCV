using NexaCV.Api.Enums;

namespace NexaCV.Api.Models;

public class Resume
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int TemplateId { get; set; }
    public ResumeStatus Status { get; set; }
    public string? Name { get; set; }
    public string? RawData { get; set; }
    public string? FinalData { get; set; }
    public string? JobTitleSuggestionsJson { get; set; }
    public string? SkillSuggestionsJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public bool AiAvailable { get; set; }

    public User User { get; set; } = null!;
    public Template Template { get; set; } = null!;
    public ICollection<Regeneration> Regenerations { get; set; } = new List<Regeneration>();
    public Transaction? Transaction { get; set; }
    public ICollection<Download> Downloads { get; set; } = new List<Download>();
    public ICollection<ResumeHistory> History { get; set; } = new List<ResumeHistory>();
}
