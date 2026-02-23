using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Database;
using Shop.Domain.Entities;
using System.Text.Json;

namespace Shop.Infrastructure.Persistences.Config;

public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.ToTable("Purchases");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
                .HasConversion(
                    id => id.Value,
                    value => PurchaseId.FromString(value))
                .HasMaxLength(40)
                .IsUnicode(false)
                .ValueGeneratedNever();

        builder.HasIndex(p => p.Id)
               .IsDescending();

        builder.Property(p => p.OrderId)
            .HasConversion(
                id => id.Value,
                value => OrderId.Create(value))
            .HasMaxLength(2024)
            .IsRequired();

        builder.Property(p => p.Store)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(p => p.PurchaseToken)
            .HasConversion(
                token => token.Value,
                value => PurchaseToken.Create(value))
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(p => p.Signature)
            .HasConversion(
                sig => sig.HasValue ? sig.Value.Value : null,
                value => value != null ? PurchaseSignature.Create(value) : null)
            .IsRequired(false);

        builder.Property(p => p.ProductId)
            .IsRequired()
            .HasMaxLength(100);


        builder.Property(p => p.Receipt)
            .IsRequired(false);

        builder.Property(p => p.Payload)
            .IsRequired(false);


        builder.Property(p => p.PurchaseAtUtc)
            .IsRequired();

        builder.Property(p => p.UserId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.State)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(p => p.IsSandbox)
            .IsRequired();

        builder.Property(p => p.Metadata)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions.Default),
                v => string.IsNullOrWhiteSpace(v)
                    ? new Dictionary<string, string>()
                    : JsonSerializer.Deserialize<Dictionary<string, string>>(v, JsonOptions.Default)!)
            .IsRequired(false);

        builder.HasMany(p => p.ValidationAttempts)
            .WithOne()
            .HasForeignKey(a => a.PurchaseId)
            .OnDelete(DeleteBehavior.Cascade);

        var attemptNavigation = builder.Metadata
            .FindNavigation(nameof(Purchase.ValidationAttempts))!;

        attemptNavigation.SetField("_validationAttempts");
        attemptNavigation.SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(p => new { p.Store, p.OrderId })
               .IsUnique();

        builder.HasIndex(p => new { p.Store, p.Id, p.State, p.IsSandbox })
               .HasDatabaseName("IX_Purchases_Filter");

        builder.ConfigureAuditable();
    }

    private static class JsonOptions
    {
        public static readonly JsonSerializerOptions Default = new()
        {
            PropertyNamingPolicy = null,
            WriteIndented = false
        };
    }
}
