using Microsoft.EntityFrameworkCore;
using NexaCV.Identity.Data;
using NexaCV.Identity.Models;

namespace NexaCV.Identity.Data;

/// <summary>Seeds a default test user so the service is immediately usable after startup.</summary>
public sealed class IdentitySeeder
{
    // Fixed GUID matching the seed user in NexaCV.Api's DataSeeder.
    // Both services must agree on this ID so the same user works across both.
    private static readonly Guid SeedUserId = Guid.Parse("A1B2C3D4-E5F6-7890-ABCD-EF1234567890");

    public static async Task SeedAsync(IdentityDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (await db.Users.AnyAsync())
            return;

        var defaultUser = new ApplicationUser
        {
            Id = SeedUserId,
            Email = "walidmoustafa1215@gmail.com",
            Username = "testuser",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("nMxT..iwREcYJ2Y"),
            FirstName = "Walid",
            LastName = "Mostafa",
            DateOfBirth = new DateOnly(1995, 6, 15),
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(defaultUser);
        await db.SaveChangesAsync();
    }
}
