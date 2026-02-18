using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
           .HasColumnName("OrderId")
           .IsRequired();

        builder.Property(p => p.Store)
           .HasConversion(
               store => store.Value,
               value => Store.Create(value))
           .HasColumnName("Store")
           .HasMaxLength(50)
           .IsRequired();

        builder.Property(p => p.PurchaseToken)
            .HasConversion(
                token => token != null ? token.Value : null,
                value => value != null ? PurchaseToken.Create(value) : default)
            .HasColumnName("PurchaseToken")
            .HasMaxLength(200)
            .IsRequired();

        builder.OwnsOne(p => p.Signature, s =>
        {
            s.Property(x => x.Value)
             .HasColumnName("Signature")
             .HasMaxLength(100)
             .IsRequired(false);
        });

        builder.Property(p => p.ProductId)
            .IsRequired()
            .HasMaxLength(100);


        builder.Property(p => p.Receipt)
            .IsRequired(false)
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.Payload)
            .IsRequired(false)
            .HasColumnType("nvarchar(max)");


        builder.Property(p => p.PurchaseAtUtc)
            .IsRequired();

        builder.Property(p => p.UserId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.State)
            .IsRequired();

        builder.Property(p => p.IsSandbox)
            .HasField("_isSandBox")
            .HasColumnName("IsSandbox")
            .IsRequired();

        builder.Property(p => p.CreatedAtUtc)
            .IsRequired();

        builder.Property(p => p.UpdatedAtUtc)
            .IsRequired(false);

        builder.Property(p => p.Meta)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, JsonSerializerOptions.Default) ?? new Dictionary<string, object>())
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.HasMany(p => p.PurchaseValidationAttempts)
            .WithOne()
            .HasForeignKey(a => a.PurchaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.Store, p.OrderId })
               .IsUnique();

        builder.HasIndex(p => new { p.Store, p.Id, p.State, p.IsSandbox })
               .HasDatabaseName("IX_Purchases_Filter");
    }
}
