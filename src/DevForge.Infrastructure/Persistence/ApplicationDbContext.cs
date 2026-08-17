using System.Reflection;
using Microsoft.EntityFrameworkCore;
using DevForge.Domain.Entities;
using DevForge.Domain.Common;

namespace DevForge.Infrastructure.Persistence;

/// <summary>
/// Entity Framework DbContext for the DevForge database.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<SystemStatusInfo> SystemStatuses => Set<SystemStatusInfo>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    // Challenge entities
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<ChallengeAttempt> ChallengeAttempts => Set<ChallengeAttempt>();
    public DbSet<ChallengeAnswer> ChallengeAnswers => Set<ChallengeAnswer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                    entry.Entity.CreatedBy = "System";
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModifiedAt = DateTimeOffset.UtcNow;
                    entry.Entity.LastModifiedBy = "System";
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
