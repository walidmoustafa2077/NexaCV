using Microsoft.EntityFrameworkCore;
using NexaCV.Api.Enums;
using NexaCV.Api.Models;

namespace NexaCV.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<NexaCvUserProfile> Profiles => Set<NexaCvUserProfile>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<Template> Templates => Set<Template>();
    public DbSet<Resume> Resumes => Set<Resume>();
    public DbSet<Regeneration> Regenerations => Set<Regeneration>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Download> Downloads => Set<Download>();
    public DbSet<ResumeHistory> ResumeHistories => Set<ResumeHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── NexaCvUserProfile ─────────────────────────────────────
        modelBuilder.Entity<NexaCvUserProfile>(e =>
        {
            e.HasKey(p => p.UserId);
            e.Property(p => p.UserId).ValueGeneratedNever(); // assigned from JWT claims

            e.HasMany(p => p.Resumes)
             .WithOne(r => r.User)
             .HasForeignKey(r => r.UserId);

            e.HasMany(p => p.Transactions)
             .WithOne(t => t.User)
             .HasForeignKey(t => t.UserId);

            e.HasMany(p => p.ActivityLogs)
             .WithOne(a => a.User)
             .HasForeignKey(a => a.UserId);
        });

        // ── ActivityLog ───────────────────────────────────────────
        modelBuilder.Entity<ActivityLog>()
            .Property(a => a.ActionType)
            .HasConversion<string>();

        // ── Enum → string conversions ─────────────────────────────
        modelBuilder.Entity<Resume>()
            .Property(r => r.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Transaction>()
            .Property(t => t.PaymentStatus)
            .HasConversion<string>();

        // ── Template PK auto-increment ────────────────────────────
        modelBuilder.Entity<Template>()
            .Property(t => t.Id)
            .ValueGeneratedOnAdd();

        // ── Global soft-delete filter on Resume ───────────────────
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
    }
}
