using NexaCV.Api.Models;

namespace NexaCV.Api.Data;

public class DataSeeder
{
    public async Task SeedAsync(AppDbContext db)
    {
        var utcNow = DateTime.UtcNow;

        // Seed default user
        if (!db.Users.Any())
        {
            var defaultUser = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "User",
                Username = "testuser",
                Email = "walidmoustafa1215@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("nMxT..iwREcYJ2Y"),
                CreatedAt = utcNow
            };

            db.Users.Add(defaultUser);
            await db.SaveChangesAsync();
        }

        // Seed templates
        if (db.Templates.Any()) return;

        db.Templates.AddRange(
            new Template
            {
                Name = "Modern Minimalist",
                IndustryCategory = "Corporate",
                BasePriceUsd = 3.00m,
                SupportsWord = true,
                IsActive = true,
                CreatedAt = utcNow
            },
            new Template
            {
                Name = "Creative",
                IndustryCategory = "Creative",
                BasePriceUsd = 3.00m,
                SupportsWord = false,
                IsActive = true,
                CreatedAt = utcNow
            },
            new Template
            {
                Name = "Executive",
                IndustryCategory = "Corporate",
                BasePriceUsd = 3.75m,
                SupportsWord = true,
                IsActive = true,
                CreatedAt = utcNow
            }
        );

        await db.SaveChangesAsync();
    }
}
