using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using NexaCV.Tests.Helpers;

namespace NexaCV.Tests.Utils;

public class JwtServiceTests
{
    private readonly JwtService _sut = JwtTestHelper.Create();

    // ── GenerateToken ─────────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: generates a JWT token for a valid user object.
    /// <br/><b>Input:</b> User stub (email="test@example.com", BCrypt-hashed password).
    /// <br/><b>Expected:</b> Returned token is a non-null, non-whitespace string.
    /// </summary>
    [Fact]
    public void GenerateToken_ReturnsNonEmptyString()
    {
        // Arrange – Input: default user stub from JwtTestHelper
        var user = JwtTestHelper.MakeUser();

        // Act
        var token = _sut.GenerateToken(user);

        // Assert – Expected: non-empty token string
        token.Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Scenario: the generated token must embed the user's ID in the "sub" claim.
    /// <br/><b>Input:</b> User stub with a unique Id (Guid).
    /// <br/><b>Expected:</b> Parsed JWT "sub" claim equals user.Id.ToString().
    /// </summary>
    [Fact]
    public void GenerateToken_ContainsUserIdAsSub()
    {
        // Arrange – Input: user stub with a known Id
        var user = JwtTestHelper.MakeUser();

        // Act
        var token = _sut.GenerateToken(user);

        // Assert – Expected: "sub" claim equals user.Id
        var handler = new JwtSecurityTokenHandler();
        var parsed = handler.ReadJwtToken(token);

        var sub = parsed.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        sub.Should().Be(user.Id.ToString());
    }

    /// <summary>
    /// Scenario: the generated token must include the user's email in the standard "email" claim.
    /// <br/><b>Input:</b> User stub with email="test@jwt.com".
    /// <br/><b>Expected:</b> Parsed JWT "email" claim equals "test@jwt.com".
    /// </summary>
    [Fact]
    public void GenerateToken_ContainsEmailClaim()
    {
        // Arrange – Input: user with a specific email address
        var user = JwtTestHelper.MakeUser("test@jwt.com");

        // Act
        var token = _sut.GenerateToken(user);

        // Assert – Expected: "email" claim matches the user's email
        var handler = new JwtSecurityTokenHandler();
        var parsed = handler.ReadJwtToken(token);

        var email = parsed.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
        email.Should().Be(user.Email);
    }

    /// <summary>
    /// Scenario: every generated token must contain a unique JWT ID ("jti") claim for replay-attack prevention.
    /// <br/><b>Input:</b> User stub (any user).
    /// <br/><b>Expected:</b> Parsed JWT claims collection contains at least one claim with type "jti".
    /// </summary>
    [Fact]
    public void GenerateToken_ContainsJtiClaim()
    {
        // Arrange – Input: default user stub
        var user = JwtTestHelper.MakeUser();

        // Act
        var token = _sut.GenerateToken(user);

        // Assert – Expected: "jti" claim present
        var handler = new JwtSecurityTokenHandler();
        var parsed = handler.ReadJwtToken(token);

        parsed.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
    }

    /// <summary>
    /// Scenario: two successive calls to GenerateToken must produce different tokens
    /// (unique "jti" each time).
    /// <br/><b>Input:</b> Same User stub passed twice.
    /// <br/><b>Expected:</b> The two resulting token strings are not equal.
    /// </summary>
    [Fact]
    public void GenerateToken_TwoCallsProduceDifferentTokens()
    {
        // Arrange – Input: same user stub, two separate calls
        var user = JwtTestHelper.MakeUser();

        // Act
        var token1 = _sut.GenerateToken(user);
        var token2 = _sut.GenerateToken(user);

        // Assert – Expected: different tokens due to unique JTI per call
        token1.Should().NotBe(token2);
    }

    // ── GetUserIdFromClaims ───────────────────────────────────────────────────

    /// <summary>
    /// Scenario: extracts the user ID from a valid ClaimsPrincipal that has a "sub" claim containing a Guid.
    /// <br/><b>Input:</b> ClaimsPrincipal with a single "sub" claim whose value is a valid Guid string.
    /// <br/><b>Expected:</b> The parsed Guid equals the original userId.
    /// </summary>
    [Fact]
    public void GetUserIdFromClaims_WithValidSubClaim_ReturnsUserId()
    {
        // Arrange – Input: ClaimsPrincipal with "sub" = a valid Guid
        var userId = Guid.NewGuid();
        var principal = MakePrincipalWithSub(userId.ToString());

        // Act
        var result = _sut.GetUserIdFromClaims(principal);

        // Assert – Expected: extracted Guid matches the original userId
        result.Should().Be(userId);
    }

    /// <summary>
    /// Scenario: the ClaimsPrincipal has no claims at all, so the "sub" claim is absent.
    /// <br/><b>Input:</b> ClaimsPrincipal with an empty claims identity.
    /// <br/><b>Expected:</b> UnauthorizedAccessException thrown.
    /// </summary>
    [Fact]
    public void GetUserIdFromClaims_WithMissingSubClaim_ThrowsUnauthorizedAccessException()
    {
        // Arrange – Input: principal with zero claims (no "sub")
        var principal = new ClaimsPrincipal(new ClaimsIdentity(Array.Empty<Claim>()));

        // Act
        Action act = () => _sut.GetUserIdFromClaims(principal);

        // Assert – Expected: UnauthorizedAccessException (missing sub claim)
        act.Should().Throw<UnauthorizedAccessException>();
    }

    /// <summary>
    /// Scenario: the "sub" claim exists but its value is not a parseable Guid.
    /// <br/><b>Input:</b> ClaimsPrincipal with "sub" = "not-a-guid".
    /// <br/><b>Expected:</b> UnauthorizedAccessException thrown (Guid.Parse fails).
    /// </summary>
    [Fact]
    public void GetUserIdFromClaims_WithNonGuidSubClaim_ThrowsUnauthorizedAccessException()
    {
        // Arrange – Input: "sub" claim contains a non-Guid string
        var principal = MakePrincipalWithSub("not-a-guid");

        // Act
        Action act = () => _sut.GetUserIdFromClaims(principal);

        // Assert – Expected: UnauthorizedAccessException (Guid parse failure)
        act.Should().Throw<UnauthorizedAccessException>();
    }

    private static ClaimsPrincipal MakePrincipalWithSub(string sub)
    {
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, sub) };
        return new ClaimsPrincipal(new ClaimsIdentity(claims));
    }
}
