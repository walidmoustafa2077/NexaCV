using NexaCV.Api.DTOs.Resumes;

namespace NexaCV.Api.Services;

public interface IRegenerationService
{
    Task<RegenerateResponse> RegenerateAsync(Guid resumeId, Guid userId, RegenerateRequest req);
}
