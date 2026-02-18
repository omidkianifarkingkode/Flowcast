using Microsoft.EntityFrameworkCore;
using SharedKernel;
using Shop.Application.IRepositories;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistences.Repositories
{
    public sealed class PurchaseRepository(ApplicationDbContext ctx) : IPurchaseRepository
    {
        private readonly ApplicationDbContext _ctx = ctx;
        private readonly DbSet<Purchase> _dbSet = ctx.Set<Purchase>();
        public async Task<IReadOnlyList<Purchase>> GetAllPurchase(CancellationToken ct)
        {
           return await _dbSet
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Result> AddNewLog(Purchase purchase, CancellationToken ct)
        {
            try
            {
                await _dbSet.AddAsync(purchase, ct);
                return Result.Success();
            }catch (Exception ex)
            {
                return Result.Failure(Error.Failure("Add Purchase", ex.Message));
            }
        }

       public async Task<Purchase?> GetByStoreAndOrderId(Store store, OrderId orderId, CancellationToken ct)
        {
            return await _dbSet
                .FirstOrDefaultAsync(
                p => p.Store == store &&
                p.OrderId == orderId,
                ct);

        }

    }
}
