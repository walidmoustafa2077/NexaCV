using Microsoft.AspNetCore.Http;

namespace NexaCV.Api.Services;

/// <summary>
/// Resolves the current user's identity lazily from the <see cref="IHttpContextAccessor"/>.
/// Lazy evaluation is intentional: the <see cref="UserId"/> property is only computed on first access,
/// so this service can be safely registered as Scoped without throwing during unauthenticated requests
/// (e.g. registration/login) where <c>UserId</c> is never accessed.
/// </summary>
public class ClaimsPrincipalCurrentUserContext : ICurrentUserContext
{
    private readonly Lazy<Guid> _userId;

    public ClaimsPrincipalCurrentUserContext(IHttpContextAccessor accessor, JwtService jwt)
    {
        _userId = new Lazy<Guid>(
            () => jwt.GetUserIdFromClaims(accessor.HttpContext!.User));
    }

    public Guid UserId => _userId.Value;
}
