using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistences.Config;

public class PurchaseValidationAttemptConfiguration : IEntityTypeConfiguration<PurchaseValidationAttempt>
{
    public void Configure(EntityTypeBuilder<PurchaseValidationAttempt> builder)
    {
        builder.ToTable("PurchaseValidationAttempts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedOnAdd();

        builder.Property(a => a.PurchaseId)
            .HasConversion(
                id => id.Value,
                value => PurchaseId.FromString(value))
            .IsRequired();

        builder.Property(a => a.AttemptNo)
            .IsRequired();

        builder.Property(a => a.StartedAtUtc)
            .IsRequired();

        builder.Property(a => a.FinishedAtUtc)
            .IsRequired(false);

        builder.Property(a => a.ErrorCode)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(a => a.ErrorMessage)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.HasIndex(a => a.PurchaseId);

        builder.HasIndex(a => new { a.PurchaseId, a.AttemptNo })
            .IsUnique();
    }
}
