using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DevForge.Domain.Entities;

namespace DevForge.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the SystemStatusInfo entity.
/// </summary>
public class SystemStatusInfoConfiguration : IEntityTypeConfiguration<SystemStatusInfo>
{
    public void Configure(EntityTypeBuilder<SystemStatusInfo> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ApplicationName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Version)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.CheckedAt)
            .IsRequired();
    }
}
