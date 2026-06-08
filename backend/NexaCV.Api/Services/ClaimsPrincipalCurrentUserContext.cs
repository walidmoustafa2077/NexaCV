using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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

    public ClaimsPrincipalCurrentUserContext(IHttpContextAccessor accessor)
    {
        _userId = new Lazy<Guid>(() => GetUserIdFromClaims(accessor.HttpContext!.User));
    }

    public Guid UserId => _userId.Value;

    private static Guid GetUserIdFromClaims(ClaimsPrincipal principal)
    {
        var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (sub is null || !Guid.TryParse(sub, out var userId))
            throw new UnauthorizedAccessException("Invalid or missing user identity claim.");

        return userId;
    }
}
