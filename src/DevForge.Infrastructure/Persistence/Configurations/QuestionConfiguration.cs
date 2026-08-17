using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DevForge.Domain.Entities;

namespace DevForge.Infrastructure.Persistence.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.HasKey(q => q.Id);

        builder.Property(q => q.Text)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(q => q.Explanation)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(q => q.Category)
            .IsRequired();

        builder.Property(q => q.Difficulty)
            .IsRequired();

        builder.HasIndex(q => q.Category);
        builder.HasIndex(q => q.Difficulty);
        builder.HasIndex(q => new { q.Category, q.Difficulty });

        builder.HasMany(q => q.Options)
            .WithOne(o => o.Question)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
