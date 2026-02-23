using Microsoft.EntityFrameworkCore;
using Shop.Domain.Entities;

namespace Shop.Application.Interfaces
{
    public interface IDataDbContext
    {
        DbSet<Purchase> Purchases { get; }

        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
