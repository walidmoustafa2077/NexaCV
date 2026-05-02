using NexaCV.Api.Models;

namespace NexaCV.Api.Data;

public class DataSeeder
{
    public async Task SeedAsync(AppDbContext db)
    {
        if (db.Templates.Any()) return;

        var utcNow = DateTime.UtcNow;

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
