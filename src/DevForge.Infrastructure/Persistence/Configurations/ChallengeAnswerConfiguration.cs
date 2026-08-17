using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DevForge.Domain.Entities;

namespace DevForge.Infrastructure.Persistence.Configurations;

public class ChallengeAnswerConfiguration : IEntityTypeConfiguration<ChallengeAnswer>
{
    public void Configure(EntityTypeBuilder<ChallengeAnswer> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AttemptId)
            .IsRequired();

        builder.Property(a => a.QuestionId)
            .IsRequired();

        builder.Property(a => a.SelectedOptionId)
            .IsRequired();

        builder.Property(a => a.IsCorrect)
            .IsRequired();

        // Question navigation — no cascade delete (questions are shared across attempts)
        builder.HasOne(a => a.Question)
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
