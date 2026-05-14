using System.Text.Json;
using NexaCV.Api.DTOs.Resumes;
using NexaCV.Api.Enums;
using NexaCV.Api.Extensions;
using NexaCV.Api.Models;
using NexaCV.Api.Repositories;

namespace NexaCV.Api.Services;

public class ResumeService : IResumeService
{
    private readonly IResumeRepository _resumes;
    private readonly IDownloadRepository _downloads;
    private readonly IResumeHistoryRepository _history;
    private readonly IAiService _ai;
    private readonly ITemplateRendererService _renderer;

    public ResumeService(
        IResumeRepository resumes,
        IDownloadRepository downloads,
        IResumeHistoryRepository history,
        IAiService ai,
        ITemplateRendererService renderer)
    {
        _resumes = resumes;
        _downloads = downloads;
        _history = history;
        _ai = ai;
        _renderer = renderer;
    }

    public async Task<ResumeDetailDto> CreateAsync(Guid userId, CreateResumeRequest req)
    {
        var resume = req.ToResume(userId);
        await _resumes.AddAsync(resume);

        var result = await _ai.GenerateAsync(resume.RawData ?? string.Empty);

        resume.FinalData = result.FinalDataJson;
        resume.AiAvailable = result.AiAvailable;
        resume.Status = ResumeStatus.Completed;
        resume.UpdatedAt = DateTime.UtcNow;
        resume.JobTitleSuggestionsJson = result.JobTitleSuggestions is { Count: > 0 }
            ? JsonSerializer.Serialize(result.JobTitleSuggestions)
            : null;
        resume.SkillSuggestionsJson = result.SkillSuggestions is { Count: > 0 }
            ? JsonSerializer.Serialize(result.SkillSuggestions)
            : null;

        await _resumes.UpdateAsync(resume);

        await _history.AddAsync(new ResumeHistory
        {
            Id = Guid.NewGuid(),
            ResumeId = resume.Id,
            SnapshotData = resume.FinalData,
            Reason = "INITIAL_AI_GEN",
            CreatedAt = DateTime.UtcNow
        });
        await _history.PruneAsync(resume.Id);

        // Re-fetch with Template navigation loaded for mapping
        var withTemplate = await _resumes.GetWithTemplateAsync(resume.Id)
            ?? resume;

        return withTemplate.ToDetailDto(result.AiAvailable, result.JobTitleSuggestions, result.SkillSuggestions);
    }

    public async Task<List<ResumeSummaryDto>> GetAllByUserAsync(Guid userId)
    {
        var resumes = await _resumes.GetByUserIdAsync(userId);
        return resumes.Select(r => r.ToSummaryDto()).ToList();
    }

    public async Task<ResumeSummaryDto> RenameAsync(Guid resumeId, Guid userId, string name)
    {
        var resume = await _resumes.GetWithTemplateAsync(resumeId)
            ?? throw new KeyNotFoundException("Resume not found.");

        if (resume.UserId != userId)
            throw new ForbiddenException("Access denied.");

        resume.Name = name.Trim();
        resume.UpdatedAt = DateTime.UtcNow;
        await _resumes.UpdateAsync(resume);

        return resume.ToSummaryDto();
    }

    public async Task<ResumeDetailDto> GetByIdAsync(Guid resumeId, Guid userId)
    {
        var resume = await _resumes.GetWithTemplateAsync(resumeId)
            ?? throw new KeyNotFoundException("Resume not found.");

        if (resume.UserId != userId)
            throw new ForbiddenException("Access denied.");

        var jobTitles = resume.JobTitleSuggestionsJson is not null
            ? JsonSerializer.Deserialize<IReadOnlyList<AiJobTitleSuggestion>>(resume.JobTitleSuggestionsJson)
            : null;
        var skills = resume.SkillSuggestionsJson is not null
            ? JsonSerializer.Deserialize<IReadOnlyList<string>>(resume.SkillSuggestionsJson)
            : null;

        return resume.ToDetailDto(resume.AiAvailable, jobTitles, skills);
    }

    public async Task<ResumeDetailDto> UpdateFinalDataAsync(Guid resumeId, Guid userId, string finalData)
    {
        var resume = await _resumes.GetWithTemplateAsync(resumeId)
            ?? throw new KeyNotFoundException("Resume not found.");

        if (resume.UserId != userId)
            throw new ForbiddenException("Access denied.");

        resume.FinalData = finalData;
        resume.UpdatedAt = DateTime.UtcNow;

        await _resumes.UpdateAsync(resume);

        await _history.AddAsync(new ResumeHistory
        {
            Id = Guid.NewGuid(),
            ResumeId = resume.Id,
            SnapshotData = finalData,
            Reason = "MANUAL_EDIT",
            CreatedAt = DateTime.UtcNow
        });
        await _history.PruneAsync(resume.Id);

        var jobTitles = resume.JobTitleSuggestionsJson is not null
            ? JsonSerializer.Deserialize<IReadOnlyList<AiJobTitleSuggestion>>(resume.JobTitleSuggestionsJson)
            : null;
        var skills = resume.SkillSuggestionsJson is not null
            ? JsonSerializer.Deserialize<IReadOnlyList<string>>(resume.SkillSuggestionsJson)
            : null;

        return resume.ToDetailDto(resume.AiAvailable, jobTitles, skills);
    }

    public async Task DeleteAsync(Guid resumeId, Guid userId)
    {
        var resume = await _resumes.GetWithTemplateAsync(resumeId)
            ?? throw new KeyNotFoundException("Resume not found.");

        if (resume.UserId != userId)
            throw new ForbiddenException("Access denied.");

        if (resume.Status == ResumeStatus.Paid)
            throw new InvalidOperationException("Cannot delete a paid resume.");

        resume.IsDeleted = true;
        resume.UpdatedAt = DateTime.UtcNow;

        await _resumes.UpdateAsync(resume);
    }

    public async Task<Resume> GetForDownloadAsync(Guid resumeId, Guid userId, string format, string? ipAddress)
    {
        var resume = await _resumes.GetWithTemplateAsync(resumeId)
            ?? throw new KeyNotFoundException("Resume not found.");

        if (resume.UserId != userId)
            throw new ForbiddenException("Access denied.");

        if (resume.Status != ResumeStatus.Paid)
            throw new UnauthorizedAccessException("Resume must be paid before downloading.");

        if (format.Equals("docx", StringComparison.OrdinalIgnoreCase) && !resume.Template.SupportsWord)
            throw new InvalidOperationException("This template does not support DOCX format.");

        await _downloads.AddAsync(new Download
        {
            Id = Guid.NewGuid(),
            ResumeId = resumeId,
            FormatType = format.ToUpperInvariant(),
            DownloadedAt = DateTime.UtcNow,
            IpAddress = ipAddress
        });

        return resume;
    }

    public async Task<string> RenderHtmlAsync(Guid resumeId, Guid userId)
    {
        var resume = await _resumes.GetWithTemplateAsync(resumeId)
            ?? throw new KeyNotFoundException("Resume not found.");

        if (resume.UserId != userId)
            throw new ForbiddenException("Access denied.");

        var html = resume.Template.HtmlContent;
        if (string.IsNullOrWhiteSpace(html))
            return "<html><body><p>This template has no HTML content.</p></body></html>";

        var data = resume.FinalData ?? resume.RawData ?? "{}";
        return _renderer.Render(html, data);
    }
}
