using NexaCV.Tests.Helpers;

namespace NexaCV.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IUserMovementRepository> _movements = new();

    private UserService CreateSut() => new(_users.Object, _movements.Object);

    // ── GetProfile ────────────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: fetches the profile for a user whose ID exists in the repository.
    /// <br/><b>Input:</b> User stub (email="test@example.com"); GetByIdAsync returns that user.
    /// <br/><b>Expected:</b> UserProfileDto with Id, Email, and FirstName matching the user stub.
    /// </summary>
    [Fact]
    public async Task GetProfileAsync_WithValidUserId_ReturnsProfileDto()
    {
        // Arrange – Input: a user stub returned by the repository mock
        var user = JwtTestHelper.MakeUser();
        _users.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        // Act
        var result = await CreateSut().GetProfileAsync(user.Id);

        // Assert – Expected: DTO fields match the user entity
        result.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.FirstName.Should().Be(user.FirstName);
    }

    /// <summary>
    /// Scenario: requests a profile for a user ID that does not exist in the repository.
    /// <br/><b>Input:</b> GetByIdAsync returns null for any Guid; request uses a random Guid.
    /// <br/><b>Expected:</b> KeyNotFoundException with message containing "User not found".
    /// </summary>
    [Fact]
    public async Task GetProfileAsync_WithUnknownUserId_ThrowsKeyNotFoundException()
    {
        // Arrange – Input: repository returns null for every id (user not found)
        _users.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        // Act
        Func<Task> act = () => CreateSut().GetProfileAsync(Guid.NewGuid());

        // Assert – Expected: KeyNotFoundException with descriptive message
        await act.Should().ThrowAsync<KeyNotFoundException>()
                 .WithMessage("*User not found*");
    }

    // ── UpdateProfile ─────────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: updates only the FirstName field of an existing user.
    /// <br/><b>Input:</b> User stub; UpdateUserRequest { FirstName="Updated" } (all other fields null).
    /// <br/><b>Expected:</b> Returned UserProfileDto has FirstName="Updated"; other fields unchanged.
    /// </summary>
    [Fact]
    public async Task UpdateProfileAsync_UpdatesFirstName_ReturnsUpdatedDto()
    {
        // Arrange – Input: existing user stub, request changes only FirstName
        var user = JwtTestHelper.MakeUser();
        _users.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _users.Setup(r => r.UpdateAsync(user)).Returns(Task.CompletedTask);

        var req = new UpdateUserRequest { FirstName = "Updated" };

        // Act
        var result = await CreateSut().UpdateProfileAsync(user.Id, req);

        // Assert – Expected: FirstName changed to "Updated"
        result.FirstName.Should().Be("Updated");
    }

    /// <summary>
    /// Scenario: updates a user's password and verifies the audit movement is logged.
    /// <br/><b>Input:</b> User stub; UpdateUserRequest { Password="N3wP@ss!" }.
    /// <br/><b>Expected:</b> movements.LogAsync called exactly once with ActionType.PasswordUpdated.
    /// </summary>
    [Fact]
    public async Task UpdateProfileAsync_WithPasswordChange_LogsPasswordUpdated()
    {
        // Arrange – Input: user stub, request contains a new password
        var user = JwtTestHelper.MakeUser();
        _users.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _users.Setup(r => r.UpdateAsync(user)).Returns(Task.CompletedTask);
        _movements.Setup(r => r.LogAsync(user.Id, ActionType.PasswordUpdated, null, null))
                  .Returns(Task.CompletedTask);

        var req = new UpdateUserRequest { Password = "N3wP@ss!" };

        // Act
        await CreateSut().UpdateProfileAsync(user.Id, req);

        // Assert – Expected: PasswordUpdated movement logged exactly once
        _movements.Verify(r => r.LogAsync(user.Id, ActionType.PasswordUpdated, null, null), Times.Once);
    }

    /// <summary>
    /// Scenario: partial update — only FirstName is provided in the request; LastName must remain untouched.
    /// <br/><b>Input:</b> User stub with an existing LastName; UpdateUserRequest { FirstName="NewFirst" }.
    /// <br/><b>Expected:</b> Returned DTO has FirstName="NewFirst" and the original LastName unchanged.
    /// </summary>
    [Fact]
    public async Task UpdateProfileAsync_WithNullFields_OnlyAppliesNonNullChanges()
    {
        // Arrange – Input: user with existing LastName; request omits LastName (null)
        var user = JwtTestHelper.MakeUser();
        var originalLastName = user.LastName;
        _users.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _users.Setup(r => r.UpdateAsync(user)).Returns(Task.CompletedTask);

        // Only update first name; last name stays the same
        var req = new UpdateUserRequest { FirstName = "NewFirst" };

        // Act
        var result = await CreateSut().UpdateProfileAsync(user.Id, req);

        // Assert – Expected: FirstName updated, LastName unchanged
        result.LastName.Should().Be(originalLastName);
        result.FirstName.Should().Be("NewFirst");
    }

    /// <summary>
    /// Scenario: attempts to update a profile for a user ID that does not exist.
    /// <br/><b>Input:</b> GetByIdAsync returns null for any Guid; random Guid supplied.
    /// <br/><b>Expected:</b> KeyNotFoundException thrown.
    /// </summary>
    [Fact]
    public async Task UpdateProfileAsync_WithUnknownUserId_ThrowsKeyNotFoundException()
    {
        // Arrange – Input: repository returns null for any user id
        _users.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        // Act
        Func<Task> act = () => CreateSut().UpdateProfileAsync(Guid.NewGuid(), new UpdateUserRequest());

        // Assert – Expected: KeyNotFoundException (user not found)
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
