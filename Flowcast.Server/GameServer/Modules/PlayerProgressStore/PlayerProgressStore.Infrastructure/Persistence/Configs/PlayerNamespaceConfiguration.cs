using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayerProgressStore.Domain;

namespace PlayerProgressStore.Infrastructure.Persistence.Configs;

public class PlayerNamespaceConfiguration : IEntityTypeConfiguration<PlayerNamespace>
{
    public void Configure(EntityTypeBuilder<PlayerNamespace> builder)
    {
        builder.ToTable("PlayerNamespaces"); // Map to table "PlayerNamespaces"

        builder.HasKey(x => new { x.PlayerId, x.Namespace }); // Composite key: PlayerId + Namespace

        builder.Property(x => x.PlayerId)
            .IsRequired();

        builder.Property(x => x.Namespace)
            .IsRequired()
            .HasMaxLength(100); // Limiting namespace length

        builder.Property(x => x.Version)
            .IsRequired()
            .HasConversion(v => v.Value, v => new VersionToken(v)) // Use custom VersionToken
            .HasMaxLength(50); // Limiting version string length

        builder.Property(x => x.Progress)
            .IsRequired();

        builder.Property(x => x.Document)
            .IsRequired()
            .HasMaxLength(4000); // Adjust depending on expected document size (SQL Server text column)

        builder.Property(x => x.Hash)
            .IsRequired()
            .HasMaxLength(256); // SHA256 hash length

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();
    }
}