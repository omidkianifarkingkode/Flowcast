using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using ZP.Core.Entities;

namespace ZP.Core.Persisntence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<PaymentRequest> PaymentRequests => Set<PaymentRequest>();

    protected override void OnModelCreating(ModelBuilder mB)
    {
        base.OnModelCreating(mB);
        mB.HasDefaultSchema("ZainPal");
        mB.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
