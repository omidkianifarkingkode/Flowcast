using Identity.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Identity.API.Persistence.Configs;

public sealed class IdentityEntityConfig : IEntityTypeConfiguration<IdentityEntity>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };


    private static readonly ValueConverter<Dictionary<string, string>?, string?> DictToJson =
        new(
            v => v == null ? null : JsonSerializer.Serialize(v, JsonOptions),
            v => string.IsNullOrWhiteSpace(v) ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(v!, JsonOptions)
        );


    private static readonly ValueComparer<Dictionary<string, string>?> DictComparer =
        new(
            (a, b) =>
                ReferenceEquals(a, b) ||
                (a != null && b != null &&
                 a.Count == b.Count &&
                 a.All(kv => b.ContainsKey(kv.Key) &&
                             string.Equals(kv.Value, b[kv.Key], StringComparison.Ordinal))),
            v => v == null
                ? 0
                : v.Aggregate(0, (h, kv) => HashCode.Combine(h, kv.Key.GetHashCode(), string.IsNullOrEmpty(kv.Value)? 0 : kv.Value.GetHashCode())),
            v => v == null
                ? null
                : v.ToDictionary(kv => kv.Key, kv => kv.Value)
        );


    public void Configure(EntityTypeBuilder<IdentityEntity> b)
    {
        b.ToTable("Identities");
        b.HasKey(i => i.IdentityId);

        b.Property(i => i.AccountId).IsRequired();

        b.Property(i => i.Provider)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        b.Property(i => i.Subject)
            .HasMaxLength(256)
            .IsRequired();

        b.Property(i => i.LoginAllowed).IsRequired();

        b.Property(i => i.CreatedAtUtc).IsRequired();

        b.Property(i => i.LastSeenAtUtc);

        b.Property(i => i.LastMeta)
            .HasConversion(DictToJson)
            .HasColumnType("nvarchar(max)")                   // optional but nice on SQL Server
            .Metadata.SetValueComparer(DictComparer);

        b.HasIndex(i => new { i.Provider, i.Subject }).IsUnique();
        b.HasIndex(i => i.AccountId);
    }
}
