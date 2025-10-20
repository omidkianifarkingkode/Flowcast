using Microsoft.EntityFrameworkCore;
using PlayerProgressStore.Domain;
using PlayerProgressStore.Infrastructure.Persistences.Configs;

namespace PlayerProgressStore.Infrastructure.Persistences
{
    public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<PlayerNamespace> PlayerNamespaces => Set<PlayerNamespace>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("PlayerProgress");

            modelBuilder.ApplyConfiguration(new PlayerNamespaceConfiguration());
        }
    }
}
