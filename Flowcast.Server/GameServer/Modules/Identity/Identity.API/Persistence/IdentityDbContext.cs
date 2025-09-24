using Identity.API.Persistence.Configs;
using Identity.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Persistence;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<IdentityEntity> Identities => Set<IdentityEntity>();
    public DbSet<IdentityLoginAudit> IdentityLoginAudits => Set<IdentityLoginAudit>();
    public DbSet<SigningKey> SigningKeys => Set<SigningKey>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AccountConfig());
        modelBuilder.ApplyConfiguration(new IdentityEntityConfig());
        modelBuilder.ApplyConfiguration(new IdentityLoginAuditConfig());
        modelBuilder.ApplyConfiguration(new SigningKeyConfig());
    }
}