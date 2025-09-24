using Identity.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.API.Persistence.Configs;

public sealed class IdentityLoginAuditConfig : IEntityTypeConfiguration<IdentityLoginAudit>
{
    public void Configure(EntityTypeBuilder<IdentityLoginAudit> b)
    {
        b.ToTable("IdentityLoginAudits");
        b.HasKey(a => a.Id);
        b.Property(a => a.Id).ValueGeneratedOnAdd();


        b.Property(a => a.IdentityId).IsRequired();
        b.Property(a => a.AccountId).IsRequired();
        b.Property(a => a.LoginAtUtc).IsRequired();


        b.Property(a => a.Ip).HasMaxLength(45); // IPv6 max
        b.Property(a => a.Region).HasMaxLength(2).IsFixedLength();
        b.Property(a => a.UserAgent).HasMaxLength(512);
        b.Property(a => a.DeviceOs).HasMaxLength(64);
        b.Property(a => a.DeviceModel).HasMaxLength(128);
        b.Property(a => a.DeviceLanguage).HasMaxLength(16);
        b.Property(a => a.AppVersion).HasMaxLength(32);


        b.HasIndex(a => new { a.AccountId, a.LoginAtUtc });
    }
}