using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel;

namespace Shared.Infrastructure.Database;

public static class AuditableEntityConfigurationExtensions
{
    /// <summary>
    /// Configures common audit fields for entities implementing ICreatable
    /// </summary>
    public static EntityTypeBuilder<T> ConfigureCreatable<T>(this EntityTypeBuilder<T> builder, bool ignoredUser = false)
        where T : class, ICreatable
    {
        builder.Property(e => e.CreatedAtUtc)
            .HasColumnName(nameof(ICreatable.CreatedAtUtc))
            .IsRequired();

        if (!ignoredUser)
            builder.Property(e => e.CreatorUser)
                .HasColumnName(nameof(ICreatable.CreatorUser))
                .HasMaxLength(100)
                .IsRequired(false)
                .IsUnicode(false);

        // Optional: if you want to make CreatorUser ignored in some entities, do it per entity
        // Do NOT ignore it globally here

        return builder;
    }

    /// <summary>
    /// Configures common audit fields for entities implementing IModifiable
    /// </summary>
    public static EntityTypeBuilder<T> ConfigureModifiable<T>(this EntityTypeBuilder<T> builder, bool ignoredUser = false)
        where T : class, IModifiable
    {
        builder.Property(e => e.ModifiedAtUtc)
            .HasColumnName(nameof(IModifiable.ModifiedAtUtc))
            .IsRequired(false);

        if (!ignoredUser)
            builder.Property(e => e.ModifierUser)
            .HasColumnName(nameof(IModifiable.ModifierUser))
            .HasMaxLength(100)
            .IsRequired(false)
            .IsUnicode(false);

        return builder;
    }

    /// <summary>
    /// Configures both creatable and modifiable audit fields (most common case)
    /// </summary>
    public static EntityTypeBuilder<T> ConfigureAuditable<T>(this EntityTypeBuilder<T> builder, bool ignoredUser = false)
        where T : class, ICreatable, IModifiable
    {
        return builder
            .ConfigureCreatable(ignoredUser)
            .ConfigureModifiable(ignoredUser);
    }
}
