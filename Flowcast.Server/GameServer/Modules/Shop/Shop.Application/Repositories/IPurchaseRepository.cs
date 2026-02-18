using SharedKernel.Pagination;
using Shop.Domain.Entities;
using Shop.Domain.Enums;

namespace Shop.Application.Repositories
{
    public interface IPurchaseRepository
    {
        void AddNewLog(
           Purchase purchase,
           CancellationToken ct = default);

        Task<Purchase?> GetByStoreAndOrderId(
            Store store,
            OrderId orderId,
            bool asTracking = false,
            bool includeAttempts = true,
            CancellationToken ct = default);

        Task<Purchase?> GetByOrderId(
            OrderId orderId,
            bool asTracking = false,
            bool includeAttempts = true,
            CancellationToken ct = default);

        Task<IReadOnlyList<Purchase>> GetAll(
            bool asTracking = false,
            bool includeAttempts = false,   // ← usually false for large lists
            CancellationToken ct = default);

        Task<IReadOnlyCollection<Purchase>> GetPendingValidations(
            int limit,
            DateTimeOffset now,
            bool asTracking = false,
            bool includeAttempts = true,   // almost always needed
            CancellationToken ct = default);

        Task<Purchase?> GetById(
            PurchaseId purchaseId,
            bool asTracking = false,
            bool includeAttempts = true,
            CancellationToken ct = default);

        Task<PagedResult<Purchase>> GetAllPurchasesPaged(
            PaginationFilter filter,
            bool asTracking = false,
            bool includeAttempts = true,
            CancellationToken ct = default);

        Task<PagedResult<Purchase>> GetUserPurchases(
            string userId,
            PaginationFilter filter,
            bool asTracking = false,
            bool includeAttempts = true,
            CancellationToken ct = default);
    }
}
