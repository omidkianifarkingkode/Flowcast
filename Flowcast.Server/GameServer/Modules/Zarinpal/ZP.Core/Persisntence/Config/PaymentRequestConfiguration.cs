using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZP.Core.Entities;

namespace ZP.Core.Persisntence.Config;

public sealed class PaymentRequestConfiguration : IEntityTypeConfiguration<PaymentRequest>
{
    public void Configure(EntityTypeBuilder<PaymentRequest> builder)
    {
        builder.ToTable("PaymentRequests");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(
                id => id.Value,
                value => PaymentRequestId.FromString(value))
            .HasMaxLength(40)
            .IsUnicode(false)
            .ValueGeneratedNever();

        builder.Property(p => p.Authority)
            .HasConversion(
                a => a.Value,
                value => Authority.Create(value))
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(p => p.OrderId)
            .HasConversion(
                o => o.Value,
                value => OrderId.Create(value))
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(p => p.UserId).HasMaxLength(128).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(500).IsRequired();
        builder.Property(p => p.Currency).HasMaxLength(8);

        builder.Property(p => p.RefId)
            .HasConversion(
                r => r.HasValue ? r.Value.Value : (long?)null,
                value => value.HasValue ? RefId.Create(value.Value) : null)
            .IsRequired(false);

        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(p => p.IdempotencyKey).HasMaxLength(128).IsRequired(false);
        builder.HasIndex(p => p.Authority).IsUnique();
        builder.HasIndex(p => p.OrderId);
        builder.HasIndex(p => p.IdempotencyKey).IsUnique().HasFilter("[IdempotencyKey] IS NOT NULL");
    }
}
