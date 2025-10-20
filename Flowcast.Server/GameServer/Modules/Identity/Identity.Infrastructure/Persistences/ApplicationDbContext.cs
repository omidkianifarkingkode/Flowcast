using Identity.Domain.Entities;
using Identity.Infrastructure.Persistences.Configs;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistences;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<IdentityEntity> Identities => Set<IdentityEntity>();
    public DbSet<IdentityLoginAudit> IdentityLoginAudits => Set<IdentityLoginAudit>();
    public DbSet<SigningKey> SigningKeys => Set<SigningKey>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Identity");

        modelBuilder.ApplyConfiguration(new AccountConfig());
        modelBuilder.ApplyConfiguration(new IdentityEntityConfig());
        modelBuilder.ApplyConfiguration(new IdentityLoginAuditConfig());
        modelBuilder.ApplyConfiguration(new SigningKeyConfig());
    }
}