using Microsoft.EntityFrameworkCore;
using NexaCV.Api.Models;

namespace NexaCV.Api.Data;

public class DataSeeder
{
    /// <summary>Reads an HTML template file from the Templates/ folder relative to the entry assembly.</summary>
    private static string? ReadTemplate(string fileName)
    {
        try
        {
            var dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
                      ?? AppContext.BaseDirectory;
            var path = Path.Combine(dir, "Templates", fileName);
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }
        catch { return null; }
    }

    /// <summary>Maps each template Name (as stored in DB) to its HTML file name.</summary>
    private static readonly Dictionary<string, string> TemplateFiles = new()
    {
        ["Meridian"]       = "executive_1_meridian.html",
        ["Prestige"]       = "executive_2_prestige.html",
        ["Linden"]         = "executive_3_linden.html",
        ["Summit"]         = "executive_4_summit.html",
        ["Cobalt Split"]   = "creative_1_cobalt.html",
        ["Emerald Flow"]   = "creative_2_emerald.html",
        ["Violet Canvas"]  = "creative_3_violet.html",
        ["Coral Sidebar"]  = "creative_4_coral.html",
        ["Obsidian Glass"] = "tech_1_obsidian.html",
        ["Slate Shift"]    = "tech_2_slate.html",
        ["Neon Circuit"]   = "tech_3_neon.html",
        ["Midnight Grid"]  = "tech_4_midnight.html",
    };

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

        // Seed templates on first run
        if (!db.Templates.Any())
        {
            db.Templates.AddRange(
            // ── Executive (4) ─────────────────────────────────────────────────
            new Template
            {
                Name = "Meridian",
                IndustryCategory = "Corporate",
                StyleCategory = "Executive",
                BasePriceUsd = 3.75m,
                SupportsWord = true,
                IsActive = true,
                CreatedAt = utcNow,
                HtmlContent = ReadTemplate("executive_1_meridian.html")
            },
            new Template
            {
                Name = "Prestige",
                IndustryCategory = "Corporate",
                StyleCategory = "Executive",
                BasePriceUsd = 3.75m,
                SupportsWord = true,
                IsActive = true,
                CreatedAt = utcNow,
                HtmlContent = ReadTemplate("executive_2_prestige.html")
            },
            new Template
            {
                Name = "Linden",
                IndustryCategory = "Corporate",
                StyleCategory = "Executive",
                BasePriceUsd = 3.75m,
                SupportsWord = true,
                IsActive = true,
                CreatedAt = utcNow,
                HtmlContent = ReadTemplate("executive_3_linden.html")
            },
            new Template
            {
                Name = "Summit",
                IndustryCategory = "Corporate",
                StyleCategory = "Executive",
                BasePriceUsd = 3.75m,
                SupportsWord = true,
                IsActive = true,
                CreatedAt = utcNow,
                HtmlContent = ReadTemplate("executive_4_summit.html")
            },
            // ── Creative (4) ──────────────────────────────────────────────────
            new Template
            {
                Name = "Cobalt Split",
                IndustryCategory = "Creative",
                StyleCategory = "Creative",
                BasePriceUsd = 3.00m,
                SupportsWord = false,
                IsActive = true,
                CreatedAt = utcNow,
                HtmlContent = ReadTemplate("creative_1_cobalt.html")
            },
            new Template
            {
                Name = "Emerald Flow",
                IndustryCategory = "Creative",
                StyleCategory = "Creative",
                BasePriceUsd = 3.00m,
                SupportsWord = false,
                IsActive = true,
                CreatedAt = utcNow,
                HtmlContent = ReadTemplate("creative_2_emerald.html")
            },
            new Template
            {
                Name = "Violet Canvas",
                IndustryCategory = "Creative",
                StyleCategory = "Creative",
                BasePriceUsd = 3.00m,
                SupportsWord = false,
                IsActive = true,
                CreatedAt = utcNow,
                HtmlContent = ReadTemplate("creative_3_violet.html")
            },
            new Template
            {
                Name = "Coral Sidebar",
                IndustryCategory = "Creative",
                StyleCategory = "Creative",
                BasePriceUsd = 3.00m,
                SupportsWord = false,
                IsActive = true,
                CreatedAt = utcNow,
                HtmlContent = ReadTemplate("creative_4_coral.html")
            },
            // ── Modern Tech (4) ───────────────────────────────────────────────
            new Template
            {
                Name = "Obsidian Glass",
                IndustryCategory = "Technology",
                StyleCategory = "ModernTech",
                BasePriceUsd = 3.50m,
                SupportsWord = false,
                IsActive = true,
                CreatedAt = utcNow,
                HtmlContent = ReadTemplate("tech_1_obsidian.html")
            },
            new Template
            {
                Name = "Slate Shift",
                IndustryCategory = "Technology",
                StyleCategory = "ModernTech",
                BasePriceUsd = 3.50m,
                SupportsWord = false,
                IsActive = true,
                CreatedAt = utcNow,
                HtmlContent = ReadTemplate("tech_2_slate.html")
            },
            new Template
            {
                Name = "Neon Circuit",
                IndustryCategory = "Technology",
                StyleCategory = "ModernTech",
                BasePriceUsd = 3.50m,
                SupportsWord = false,
                IsActive = true,
                CreatedAt = utcNow,
                HtmlContent = ReadTemplate("tech_3_neon.html")
            },
            new Template
            {
                Name = "Midnight Grid",
                IndustryCategory = "Technology",
                StyleCategory = "ModernTech",
                BasePriceUsd = 3.50m,
                SupportsWord = false,
                IsActive = true,
                CreatedAt = utcNow,
                HtmlContent = ReadTemplate("tech_4_midnight.html")
            }
        );

            await db.SaveChangesAsync();
        }

        // Always sync HtmlContent from disk so template changes are picked up on restart.
        var existing = await db.Templates.ToListAsync();
        bool anyUpdated = false;
        foreach (var tpl in existing)
        {
            if (!TemplateFiles.TryGetValue(tpl.Name, out var fileName)) continue;
            var fresh = ReadTemplate(fileName);
            if (fresh is not null && fresh != tpl.HtmlContent)
            {
                tpl.HtmlContent = fresh;
                anyUpdated = true;
            }
        }
        if (anyUpdated)
            await db.SaveChangesAsync();
    }
}
