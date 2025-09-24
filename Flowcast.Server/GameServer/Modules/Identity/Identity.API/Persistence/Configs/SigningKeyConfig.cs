using Identity.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.API.Persistence.Configs;

public sealed class SigningKeyConfig : IEntityTypeConfiguration<SigningKey>
{
    public void Configure(EntityTypeBuilder<SigningKey> b)
    {
        b.ToTable("SigningKeys");
        b.HasKey(x => x.Id);

        b.Property(x => x.KeyId)
            .IsRequired()
            .HasMaxLength(64);

        b.Property(x => x.Algorithm)
            .IsRequired()
            .HasMaxLength(10);

        b.Property(x => x.PublicKeyPem)
            .IsRequired();

        b.Property(x => x.PrivateKeyPem); // nullable

        b.Property(x => x.NotBeforeUtc).IsRequired();
        b.Property(x => x.ExpiresAtUtc);
        b.Property(x => x.IsActive).IsRequired();

        b.HasIndex(x => x.KeyId).IsUnique();
        b.HasIndex(x => x.IsActive);
        b.HasIndex(x => new { x.NotBeforeUtc, x.ExpiresAtUtc });
    }
}
