namespace NexaCV.Api.Services;

/// <summary>
/// Abstracts the current authenticated user's identity away from the concrete JWT claims principal.
/// Endpoints depend on this interface rather than the JWT implementation class (DIP).
/// </summary>
public interface ICurrentUserContext
{
    /// <summary>The authenticated user's ID, extracted from the current request's JWT claims.</summary>
    Guid UserId { get; }
}
