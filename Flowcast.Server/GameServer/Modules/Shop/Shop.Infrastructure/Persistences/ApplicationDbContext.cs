using Microsoft.EntityFrameworkCore;
using Shop.Domain.Entities;
using Shop.Infrastructure.Persistences.Config;

namespace Shop.Infrastructure.Persistences;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<PurchaseValidationAttempt> PurchaseValidationAttempts => Set<PurchaseValidationAttempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("Shop");

        modelBuilder.ApplyConfiguration(new PurchaseConfiguration());
        modelBuilder.ApplyConfiguration(new PurchaseValidationAttemptConfiguration());
    }
    
}
