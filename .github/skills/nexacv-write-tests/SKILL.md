---
name: nexacv-write-tests
description: 'Write backend unit and integration tests for NexaCV services. Use when adding tests for a service, writing test cases for new logic, or verifying bug fixes. Covers two patterns: Moq-based unit tests (for repository-backed services) and real in-memory EF Core tests (for services with direct DB access). Triggers: "write tests", "add tests", "test service", "unit test", "integration test", "test coverage".'
argument-hint: 'Name the service or feature to test (e.g. "ResumeService", "AuthService")'
---

# NexaCV: Write Backend Tests

All tests live in `backend/NexaCV.Tests/`. Two patterns exist — choose the right one based on how the service is constructed.

## Stack Reference
- **Framework**: xUnit (`[Fact]`, `[Theory]`)
- **Mocking**: Moq (`Mock<T>`, `.Setup()`, `.Verify()`)
- **Assertions**: FluentAssertions (`.Should().Be()`, `.Should().NotBeNull()`, `.Should().Throw()`)
- **Test DB**: EF Core `UseInMemoryDatabase` — unique DB name per test class via `Guid.NewGuid().ToString()`
- **Helpers**: `JwtTestHelper.Create()`, `JwtTestHelper.MakeUser()`, `JwtTestHelper.MakeTemplate()`, `JwtTestHelper.MakeResume()`
- **Global usings**: All common namespaces pre-imported in `GlobalUsings.cs` — no `using` statements needed

---

## Pattern A — Moq-Based Tests (preferred for most services)

Use when the service under test depends on repository **interfaces** (not `AppDbContext` directly).

Examples: `ResumeService`, `TransactionService`, `TemplateService`

```csharp
// backend/NexaCV.Tests/Services/MyServiceTests.cs
namespace NexaCV.Tests.Services;

public class MyServiceTests
{
    // ── Mocks ─────────────────────────────────────────────────────────────────
    private readonly Mock<IMyRepository> _repo = new();
    // Add additional mocks as needed:
    private readonly Mock<IAiService> _ai = new();

    // ── SUT factory ───────────────────────────────────────────────────────────
    private MyService CreateSut() => new(_repo.Object, _ai.Object);

    // ── Tests ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: [describe what this test exercises]
    /// Input: [what you pass in]
    /// Expected: [what you assert]
    /// </summary>
    [Fact]
    public async Task MethodName_ExpectedBehavior()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entity = JwtTestHelper.MakeResume(userId);
        
        _repo.Setup(r => r.GetByIdAsync(entity.Id, userId))
             .ReturnsAsync(entity);

        // Act
        var result = await CreateSut().DoSomethingAsync(userId, entity.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(entity.Id.ToString());
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Resume>()), Times.Once);
    }
}
```

### Common Mock Setups

```csharp
// Return a value
_repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
     .ReturnsAsync(entity);

// Return null (simulate not found → expect KeyNotFoundException)
_repo.Setup(r => r.GetByIdAsync(id, userId))
     .ReturnsAsync((Resume?)null);

// Void async methods
_repo.Setup(r => r.AddAsync(It.IsAny<Resume>()))
     .Returns(Task.CompletedTask);
_repo.Setup(r => r.UpdateAsync(It.IsAny<Resume>()))
     .Returns(Task.CompletedTask);

// AI service
_ai.Setup(a => a.GenerateAsync(It.IsAny<string>()))
   .ReturnsAsync(new AiGenerationResult("{\"settings\":{},\"content\":{}}", AiAvailable: false));

// Verify call counts
_repo.Verify(r => r.UpdateAsync(It.IsAny<Resume>()), Times.Once);
_ai.Verify(a => a.GenerateAsync(It.IsAny<string>()), Times.Never);
```

---

## Pattern B — EF In-Memory Tests (for services with direct DB dependency)

Use when the service uses `AppDbContext` directly (e.g., `AuthService`, `UserService`).

```csharp
// backend/NexaCV.Tests/Services/MyDbServiceTests.cs
using Microsoft.EntityFrameworkCore;
using NexaCV.Api.Data;
using NexaCV.Api.Repositories;
using NexaCV.Tests.Helpers;

namespace NexaCV.Tests.Services;

public class MyDbServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly MyRepository _repo;
    private readonly JwtService _jwt = JwtTestHelper.Create();

    public MyDbServiceTests()
    {
        // Each test class gets its own isolated DB
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _repo = new MyRepository(_db);
    }

    public void Dispose() => _db.Dispose();

    private MyService CreateSut() => new(_repo, _jwt);

    // Seed helper — add whatever entities your tests need
    private async Task<User> SeedUserAsync(string email = "test@example.com")
    {
        var user = JwtTestHelper.MakeUser(email);
        await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task Register_CreatesUserAndReturnsToken()
    {
        // Arrange
        var req = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe",
            Email = "john@example.com",
            Password = "P@ssw0rd!"
        };

        // Act
        var result = await CreateSut().RegisterAsync(req, "127.0.0.1", "xUnit");

        // Assert
        result.Token.Should().NotBeNullOrEmpty();
        var userInDb = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        userInDb.Should().NotBeNull();
    }
}
```

---

## Test Scenarios Checklist

For each service method, cover at minimum:

| Scenario | Test name pattern |
|----------|------------------|
| Happy path | `MethodName_Returns<Expected>` |
| Not found | `MethodName_WhenEntityNotFound_Throws<Exception>` |
| Unauthorized (wrong owner) | `MethodName_WhenUserDoesNotOwn_ThrowsUnauthorized` |
| Invalid state | `MethodName_WhenStatusIs<X>_Throws<Exception>` |
| Limit exceeded | `MethodName_WhenLimitExceeded_Throws<Exception>` |

---

## Test Data Builders

```csharp
// Use JwtTestHelper helpers
var user = JwtTestHelper.MakeUser("custom@email.com");
var template = JwtTestHelper.MakeTemplate(id: 2, supportsWord: false);
var resume = JwtTestHelper.MakeResume(userId, template, ResumeStatus.Draft);

// Or build inline for specific edge cases
var resume = new Resume
{
    Id = Guid.NewGuid(),
    UserId = userId,
    Status = ResumeStatus.Completed,
    RawData = "{}",
    FinalData = "{}",
    CreatedAt = DateTime.UtcNow
};
```

---

## Asserting Exceptions

```csharp
// KeyNotFoundException
var act = () => CreateSut().GetByIdAsync(Guid.NewGuid(), userId);
await act.Should().ThrowAsync<KeyNotFoundException>();

// UnauthorizedAccessException
var act = () => CreateSut().DeleteAsync(resumeId, wrongUserId);
await act.Should().ThrowAsync<UnauthorizedAccessException>();

// InvalidOperationException (e.g., wrong status transition)
var act = () => CreateSut().PayAsync(resumeId, userId);
await act.Should().ThrowAsync<InvalidOperationException>()
    .WithMessage("*COMPLETED*");
```

---

## Running Tests

```powershell
# Run all tests
dotnet test backend/NexaCV.Tests/NexaCV.Tests.csproj

# Run specific test class
dotnet test --filter "FullyQualifiedName~ResumeServiceTests"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

---

## Checklist

- [ ] Test file placed in `backend/NexaCV.Tests/Services/`
- [ ] Class name ends in `Tests` (e.g., `MyServiceTests`)
- [ ] Correct pattern chosen: Moq (interface deps) or EF In-Memory (direct DB)
- [ ] `IDisposable` + `Dispose()` implemented when using EF In-Memory
- [ ] `CreateSut()` factory method isolates constructor args
- [ ] Each test has XML summary comment (Scenario / Input / Expected)
- [ ] Tests cover happy path, not-found, and at least one error state
- [ ] `JwtTestHelper` helpers used for consistent test data
- [ ] FluentAssertions used (`Should()`) not raw `Assert.*`
- [ ] `Verify()` used to assert side effects (saves, updates, AI calls)
