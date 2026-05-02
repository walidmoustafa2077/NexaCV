using Microsoft.EntityFrameworkCore;
using NexaCV.Api.Data;
using NexaCV.Api.Repositories;
using NexaCV.Tests.Helpers;

namespace NexaCV.Tests.Services;

/// <summary>
/// AuthService tests backed by a real in-memory EF Core database.
/// xUnit creates a new class instance per test, so each test gets its own
/// isolated DB (unique name via Guid). Use SeedUserAsync() to pre-populate
/// the DB before testing conflict / login scenarios.
/// </summary>
public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly UserRepository _userRepo;
    private readonly UserMovementRepository _movementsRepo;
    private readonly JwtService _jwt = JwtTestHelper.Create();

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _userRepo = new UserRepository(_db);
        _movementsRepo = new UserMovementRepository(_db);
    }

    public void Dispose() => _db.Dispose();

    private AuthService CreateSut() => new(_userRepo, _movementsRepo, _jwt);

    /// <summary>
    /// Inserts a real user into the in-memory DB so subsequent service calls
    /// encounter genuine existing data rather than a mock stub.
    /// </summary>
    private async Task<User> SeedUserAsync(
        string email = "existing@example.com",
        string username = "existinguser",
        string password = "P@ssw0rd!")
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Existing",
            LastName = "User",
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateTime.UtcNow
        };

        await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();

        return user;
    }

    // ── Register ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: registers a brand-new user when the database is empty (no existing conflicts).
    /// <br/><b>Input:</b> RegisterRequest { FirstName="John", LastName="Doe", Username="johndoe",
    /// Email="john@example.com", Password="P@ssw0rd!" }, ipAddress="127.0.0.1", userAgent="TestAgent"
    /// <br/><b>Expected:</b> Non-null AuthResponse with a non-empty JWT token and ExpiresIn=86400;
    /// exactly 1 User row is persisted in the database.
    /// </summary>
    [Fact]
    public async Task RegisterAsync_WithNewUser_ReturnsAuthResponse()
    {
        // Arrange – DB is empty, no users exist yet
        var usersBefore = await _db.Users.CountAsync();

        // Input: fully valid new-user registration payload
        var req = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe",
            Email = "john@example.com",
            Password = "P@ssw0rd!"
        };

        // Act
        var result = await CreateSut().RegisterAsync(req, "127.0.0.1", "TestAgent");

        // Assert – Expected: JWT token issued, user row saved to DB
        var usersAfter = await _db.Users.CountAsync();

        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrWhiteSpace();
        result.ExpiresIn.Should().Be(86400);
        usersAfter.Should().Be(1, "one user should have been persisted");
    }

    /// <summary>
    /// Scenario: attempts to register with the same email and username as an already-existing user.
    /// <br/><b>Input:</b> DB seeded with user (email="jane@example.com", username="janedoe");
    /// RegisterRequest reuses that email and username (duplicates).
    /// <br/><b>Expected:</b> ConflictException thrown with message containing "email or username";
    /// Users table stays at 1 row — the failed attempt must not persist a new row.
    /// </summary>
    [Fact]
    public async Task RegisterAsync_WhenEmailOrUsernameExists_ThrowsConflictException()
    {
        // Arrange – seed a REAL user so the conflict is genuine, not a mock stub
        var seeded = await SeedUserAsync("jane@example.com", "janedoe");

        // Input: registration payload that duplicates the seeded user's email and username
        var req = new RegisterRequest
        {
            FirstName = "Jane2",
            LastName = "Doe2",
            Username = seeded.Username, // duplicate username
            Email = seeded.Email,       // duplicate email
            Password = "P@ssw0rd!"
        };

        // Act & Assert – Expected: ConflictException thrown, no additional DB row created
        Func<Task> act = () => CreateSut().RegisterAsync(req, null, null);

        await act.Should().ThrowAsync<ConflictException>()
                 .WithMessage("*email or username*");

        (await _db.Users.CountAsync()).Should().Be(1, "failed registration must not persist a new user");
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: logs in using the exact email and password that were used when seeding the user.
    /// <br/><b>Input:</b> Seeded user (email="alice@example.com", password="P@ssw0rd!");
    /// LoginRequest { Email="alice@example.com", Password="P@ssw0rd!" }
    /// <br/><b>Expected:</b> AuthResponse with UserId matching the seeded user and a non-empty JWT token.
    /// </summary>
    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
    {
        // Arrange – seed the user who will log in
        // Input: user bcrypt-hashed with "P@ssw0rd!"; login request uses the same password
        var seeded = await SeedUserAsync("alice@example.com", "alice", "P@ssw0rd!");

        var req = new LoginRequest { Email = seeded.Email, Password = "P@ssw0rd!" };

        // Act
        var result = await CreateSut().LoginAsync(req, "127.0.0.1", "TestAgent");

        // Assert – Expected: UserId matches seeded user, valid token string returned
        result.UserId.Should().Be(seeded.Id);
        result.Token.Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Scenario: attempts login with an email that has no matching account in the database.
    /// <br/><b>Input:</b> Empty DB; LoginRequest { Email="ghost@example.com", Password="P@ssw0rd!" }
    /// <br/><b>Expected:</b> UnauthorizedAccessException (generic response to avoid leaking user existence).
    /// </summary>
    [Fact]
    public async Task LoginAsync_WithUnknownEmail_ThrowsUnauthorizedAccessException()
    {
        // Arrange – DB is intentionally empty; no user with this email exists

        // Input: email that does not correspond to any user row
        var req = new LoginRequest { Email = "ghost@example.com", Password = "P@ssw0rd!" };

        // Act & Assert – Expected: UnauthorizedAccessException (user lookup returns null)
        Func<Task> act = () => CreateSut().LoginAsync(req, null, null);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    /// <summary>
    /// Scenario: logs in with a correct email but an incorrect password.
    /// BCrypt.Verify fails and the service must reject the attempt.
    /// <br/><b>Input:</b> Seeded user (hashed password="P@ssw0rd!"); LoginRequest { same email, Password="WrongPass!" }
    /// <br/><b>Expected:</b> UnauthorizedAccessException (BCrypt hash mismatch).
    /// </summary>
    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange – seed user with known password, then attempt login with wrong one
        // Input: user stored with "P@ssw0rd!" hash; request sends "WrongPass!"
        var seeded = await SeedUserAsync(password: "P@ssw0rd!");

        var req = new LoginRequest { Email = seeded.Email, Password = "WrongPass!" };

        // Act & Assert – Expected: BCrypt comparison returns false → UnauthorizedAccessException
        Func<Task> act = () => CreateSut().LoginAsync(req, null, null);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: calls LogoutAsync for a real seeded user and asserts that the audit trail is saved.
    /// <br/><b>Input:</b> Seeded user (defaults: email="existing@example.com", username="existinguser");
    /// LogoutAsync(seeded.Id)
    /// <br/><b>Expected:</b> Exactly one UserMovement row with UserId=seeded.Id and ActionType=Logout
    /// is written to the real in-memory database.
    /// </summary>
    [Fact]
    public async Task LogoutAsync_LogsLogoutMovement()
    {
        // Arrange – seed a user so the movement FK is valid
        // Input: default seeded user (email="existing@example.com", username="existinguser")
        var seeded = await SeedUserAsync();

        // Act
        await CreateSut().LogoutAsync(seeded.Id);

        // Assert – Expected: a UserMovement row with ActionType=Logout persisted in the DB
        var movement = await _db.UserMovements
            .FirstOrDefaultAsync(m => m.UserId == seeded.Id && m.ActionType == ActionType.Logout);

        movement.Should().NotBeNull("a LOGOUT movement must be written to the DB");
        movement!.ActionType.Should().Be(ActionType.Logout);
    }
}
