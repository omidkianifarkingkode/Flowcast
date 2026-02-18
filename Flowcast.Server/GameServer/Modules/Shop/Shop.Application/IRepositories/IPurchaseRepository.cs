using SharedKernel;
using Shop.Domain.Entities;

namespace Shop.Application.IRepositories
{
    public interface IPurchaseRepository
    {
        Task<Purchase?> GetByStoreAndOrderId(Store store, OrderId orderId, CancellationToken ct = default);
        Task<IReadOnlyList<Purchase>> GetAllPurchase(CancellationToken ct);
        Task<Result> AddNewLog(Purchase purchase, CancellationToken ct);
    }
}
