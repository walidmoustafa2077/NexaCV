namespace NexaCV.Api.Models;

/// <summary>Immutable audit snapshot of a resume's FinalData at a point in time.</summary>
public class ResumeHistory
{
    public Guid Id { get; set; }

    /// <summary>FK to the resume this snapshot belongs to.</summary>
    public Guid ResumeId { get; set; }

    /// <summary>Full FinalData JSON captured at the moment the event occurred.</summary>
    public string SnapshotData { get; set; } = string.Empty;

    /// <summary>
    /// Why this snapshot was created.
    /// Known values: <c>INITIAL_AI_GEN</c>, <c>REGEN_{SectionIdentifier}</c>, <c>MANUAL_EDIT</c>.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public Resume Resume { get; set; } = null!;
}
