using Shop.Domain.Entities;
using Shop.Infrastructure.Persistences.Config;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Interfaces;

namespace Shop.Infrastructure.Persistences;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IDataDbContext
{
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<PurchaseValidationAttempt> PurchaseValidationAttempts => Set<PurchaseValidationAttempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PurchaseConfiguration());
        modelBuilder.ApplyConfiguration(new PurchaseValidationAttemptConfiguration());
    }
    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return base.SaveChangesAsync(ct);
    }
}
