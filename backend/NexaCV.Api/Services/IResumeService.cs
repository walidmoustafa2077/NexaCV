using NexaCV.Api.DTOs.Resumes;
using NexaCV.Api.Models;

namespace NexaCV.Api.Services;

public interface IResumeService
{
    Task<ResumeDetailDto> CreateAsync(Guid userId, CreateResumeRequest req);
    Task<List<ResumeSummaryDto>> GetAllByUserAsync(Guid userId);
    Task<ResumeSummaryDto> RenameAsync(Guid resumeId, Guid userId, string name);
    Task<ResumeDetailDto> GetByIdAsync(Guid resumeId, Guid userId);
    Task<ResumeDetailDto> UpdateFinalDataAsync(Guid resumeId, Guid userId, string finalData);
    Task DeleteAsync(Guid resumeId, Guid userId);
    Task<Resume> GetForDownloadAsync(Guid resumeId, Guid userId, string format, string? ipAddress);
    Task<string> RenderHtmlAsync(Guid resumeId, Guid userId);
}
