using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PlayerProgressStore.Domain;

namespace PlayerProgressStore.Infrastructure.Persistences.Configs;

public class PlayerNamespaceConfiguration : IEntityTypeConfiguration<PlayerNamespace>
{
    public void Configure(EntityTypeBuilder<PlayerNamespace> builder)
    {
        builder.ToTable("PlayerNamespaces");

        // Composite key
        builder.HasKey(x => new { x.PlayerId, x.Namespace });

        builder.Property(x => x.PlayerId)
            .IsRequired();

        builder.Property(x => x.Namespace)
            .IsRequired()
            .HasMaxLength(100);

        // VersionToken <-> string
        builder.Property(x => x.Version)
            .IsRequired()
            .HasConversion(
                v => v.Value,
                v => new VersionToken(v))
            .HasMaxLength(50)
            .IsUnicode(false);

        // ProgressScore <-> long (bigint)
        builder.Property(x => x.Progress)
            .IsRequired()
            .HasConversion(
                p => p.Value,
                v => new ProgressScore(v))
            .HasColumnType("bigint");

        // JSON document stored as UTF-8 bytes (allow large payloads)
        builder.Property(x => x.Document)
            .IsRequired()
            .HasColumnType("varbinary(max)");

        // DocHash <-> string
        builder.Property(x => x.Hash)
            .IsRequired()
            .HasConversion(
                h => h.Value,
                v => new DocHash(v))
            // SHA-256 hex = 64 chars; base64 = 44. Use what your format needs.
            .HasMaxLength(64)
            .IsUnicode(false);

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();
    }
}
