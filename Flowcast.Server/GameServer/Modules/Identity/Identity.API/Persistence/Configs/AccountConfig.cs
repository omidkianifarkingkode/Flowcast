using Identity.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.API.Persistence.Configs;

public sealed class AccountConfig : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> b)
    {
        b.ToTable("Accounts");
        b.HasKey(a => a.AccountId);


        b.Property(a => a.CreatedAtUtc)
        .IsRequired();


        b.Property(a => a.DisplayName)
        .HasMaxLength(128);


        b.Property(a => a.LastLoginRegion)
        .HasMaxLength(2)
        .IsFixedLength();


        // Backing field mapping for _identities
        b.HasMany<IdentityEntity>()
        .WithOne()
        .HasForeignKey(i => i.AccountId)
        .OnDelete(DeleteBehavior.Cascade);


        // Inform EF that Identities uses a backing field
        b.Metadata.FindNavigation(nameof(Account.Identities))!
        .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
