using Microsoft.EntityFrameworkCore;
using PlayerProgressStore.Domain;
using PlayerProgressStore.Infrastructure.Persistence.Configs;

namespace PlayerProgressStore.Infrastructure.Persistence
{
    public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<PlayerNamespace> PlayerNamespaces => Set<PlayerNamespace>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new PlayerNamespaceConfiguration());
        }
    }
}
