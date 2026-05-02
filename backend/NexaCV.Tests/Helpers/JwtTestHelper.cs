namespace NexaCV.Tests.Helpers;

internal static class JwtTestHelper
{
    public static JwtService Create()
    {
        var settings = Options.Create(new JwtSettings
        {
            Secret = "test-super-secret-key-that-is-at-least-32-chars",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpiresInSeconds = 86400
        });
        return new JwtService(settings);
    }

    public static User MakeUser(string email = "test@example.com") => new()
    {
        Id = Guid.NewGuid(),
        FirstName = "Test",
        LastName = "User",
        Username = "testuser",
        Email = email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd!"),
        CreatedAt = DateTime.UtcNow
    };

    public static Template MakeTemplate(int id = 1, bool supportsWord = true) => new()
    {
        Id = id,
        Name = "Modern Minimalist",
        IndustryCategory = "Corporate",
        BasePriceUsd = 3.00m,
        SupportsWord = supportsWord,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    public static Resume MakeResume(Guid userId, Template? template = null, ResumeStatus status = ResumeStatus.Completed)
    {
        var t = template ?? MakeTemplate();
        return new Resume
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TemplateId = t.Id,
            Template = t,
            Status = status,
            RawData = "{\"name\":\"John\"}",
            FinalData = "{\"settings\":{\"summaryType\":\"SUMMARY\",\"descriptionFormat\":\"BULLET\",\"skillsFormat\":\"GRID\"},\"content\":{\"personal\":{\"firstName\":\"John\",\"lastName\":\"Doe\",\"title\":\"Developer\"},\"summary\":\"Experienced dev\",\"skills\":[\"C#\",\".NET\"]}}",
            AiAvailable = false,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
