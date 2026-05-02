namespace NexaCV.Api.Models;

public class Download
{
    public Guid Id { get; set; }
    public Guid ResumeId { get; set; }
    public string FormatType { get; set; } = string.Empty;
    public DateTime DownloadedAt { get; set; }
    public string? IpAddress { get; set; }

    public Resume Resume { get; set; } = null!;
}
