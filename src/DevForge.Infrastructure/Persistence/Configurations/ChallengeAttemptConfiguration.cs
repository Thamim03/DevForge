using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DevForge.Domain.Entities;

namespace DevForge.Infrastructure.Persistence.Configurations;

public class ChallengeAttemptConfiguration : IEntityTypeConfiguration<ChallengeAttempt>
{
    public void Configure(EntityTypeBuilder<ChallengeAttempt> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.Property(a => a.TotalQuestions)
            .IsRequired();

        builder.Property(a => a.Score)
            .HasPrecision(5, 2);

        builder.Property(a => a.StartedAt)
            .IsRequired();

        builder.HasIndex(a => a.UserId);

        builder.HasMany(a => a.Answers)
            .WithOne(ans => ans.Attempt)
            .HasForeignKey(ans => ans.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
