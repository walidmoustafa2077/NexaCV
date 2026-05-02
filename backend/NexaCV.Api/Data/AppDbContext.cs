using Microsoft.EntityFrameworkCore;
using NexaCV.Api.Enums;
using NexaCV.Api.Models;

namespace NexaCV.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserMovement> UserMovements => Set<UserMovement>();
    public DbSet<Template> Templates => Set<Template>();
    public DbSet<Resume> Resumes => Set<Resume>();
    public DbSet<Regeneration> Regenerations => Set<Regeneration>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Download> Downloads => Set<Download>();
    public DbSet<ResumeHistory> ResumeHistories => Set<ResumeHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enum → string conversions
        modelBuilder.Entity<UserMovement>()
            .Property(u => u.ActionType)
            .HasConversion<string>();

        modelBuilder.Entity<Resume>()
            .Property(r => r.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Transaction>()
            .Property(t => t.PaymentStatus)
            .HasConversion<string>();

        // Template PK auto-increment
        modelBuilder.Entity<Template>()
            .Property(t => t.Id)
            .ValueGeneratedOnAdd();

        // Global soft-delete filter on Resume
        modelBuilder.Entity<Resume>()
            .HasQueryFilter(r => !r.IsDeleted);

        // Matching filters on dependents so they're excluded when their Resume is soft-deleted
        modelBuilder.Entity<Download>()
            .HasQueryFilter(d => !d.Resume.IsDeleted);
        modelBuilder.Entity<Regeneration>()
            .HasQueryFilter(rg => !rg.Resume.IsDeleted);
        modelBuilder.Entity<ResumeHistory>()
            .HasQueryFilter(h => !h.Resume.IsDeleted);
        modelBuilder.Entity<Transaction>()
            .HasQueryFilter(t => !t.Resume.IsDeleted);

        // --- Commented out for PostgreSQL swap ---
        // modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        // modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        // modelBuilder.Entity<Resume>().HasIndex(r => r.UserId);
        // modelBuilder.Entity<Regeneration>().HasIndex(r => new { r.ResumeId, r.SectionIdentifier });
        // modelBuilder.Entity<Resume>().Property(r => r.RawData).HasColumnType("jsonb");
        // modelBuilder.Entity<Resume>().Property(r => r.FinalData).HasColumnType("jsonb");
    }
}
