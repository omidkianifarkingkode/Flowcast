using Microsoft.EntityFrameworkCore;
using SharedKernel.Pagination;
using Shop.Application.Repositories;
using Shop.Domain.Entities;
using Shop.Domain.Enums;

namespace Shop.Infrastructure.Persistences.Repositories;

public sealed class PurchaseRepository(ApplicationDbContext ctx) : IPurchaseRepository
{
    public async Task<Purchase?> GetById(
        PurchaseId purchaseId,
        bool asTracking = false,
        bool includeAttempts = true,
        CancellationToken ct = default)
    {
        var query = ApplyQueryOptions(ctx.Purchases.AsQueryable(), asTracking, includeAttempts);

        return await query
            .FirstOrDefaultAsync(p => p.Id == purchaseId, ct);
    }

    public async Task<Purchase?> GetByStoreAndOrderId(
        Store store,
        OrderId orderId,
        bool asTracking = false,
        bool includeAttempts = true,
        CancellationToken ct = default)
    {
        var query = ApplyQueryOptions(ctx.Purchases.AsQueryable(), asTracking, includeAttempts);

        return await query
            .FirstOrDefaultAsync(p => p.Store == store && p.OrderId == orderId, ct);
    }

    public async Task<Purchase?> GetByOrderId(
        OrderId orderId,
        bool asTracking = false,
        bool includeAttempts = true,
        CancellationToken ct = default)
    {
        var query = ApplyQueryOptions(ctx.Purchases.AsQueryable(), asTracking, includeAttempts);

        return await query
            .FirstOrDefaultAsync(p => p.OrderId == orderId, ct);
    }

    public async Task<IReadOnlyList<Purchase>> GetAll(
        bool asTracking = false,
        bool includeAttempts = false, // usually false for large lists
        CancellationToken ct = default)
    {
        var query = ApplyQueryOptions(ctx.Purchases.AsQueryable(), asTracking, includeAttempts);

        return await query.ToListAsync(ct);
    }

    public void AddNewLog(Purchase purchase, CancellationToken ct = default)
    {
        ctx.Purchases.Add(purchase);
    }

    public async Task<IReadOnlyCollection<Purchase>> GetPendingValidations(
        int limit,
        DateTimeOffset now,
        bool asTracking = false,
        bool includeAttempts = true,
        CancellationToken ct = default)
    {
        var cutoff = now - Purchase.ValidationTimeout;

        var query = ApplyQueryOptions(ctx.Purchases.AsQueryable(), asTracking, includeAttempts);

        return await query
            .Where(p =>
                p.State == PurchaseState.LOGGED ||
                (p.State == PurchaseState.FAILED &&
                 p.ValidationAttempts.Count < Purchase.MaxValidationAttempts) ||
                (p.State == PurchaseState.VALIDATING &&
                 p.ValidationAttempts.Any(a =>
                     !a.FinishedAtUtc.HasValue &&
                     a.StartedAtUtc < cutoff)))
            .OrderBy(p => p.CreatedAtUtc)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<PagedResult<Purchase>> GetAllPurchasesPaged(
        PaginationFilter filter,
        bool asTracking = false,
        bool includeAttempts = true,
        CancellationToken ct = default)
    {
        var query = ApplyQueryOptions(ctx.Purchases.AsQueryable(), asTracking, includeAttempts);

        // Search
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm!.Trim();
            query = query.Where(p =>
                p.Id.Value.Contains(term) ||
                p.OrderId.Value.Contains(term) ||
                p.UserId.Contains(term) ||
                p.ProductId.Contains(term));
        }

        // Date range
        if (filter.StartDate.HasValue)
            query = query.Where(p => p.CreatedAtUtc >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(p => p.CreatedAtUtc <= filter.EndDate.Value);

        // Sorting
        query = ApplySorting(query, filter.SortBy, filter.IsDescending);

        var totalCount = await query.CountAsync(ct);

        var pageNumber = Math.Max(1, filter.PageNumber);
        var pageSize = filter.PageSize;

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Purchase>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<PagedResult<Purchase>> GetUserPurchases(
        string userId,
        PaginationFilter filter,
        bool asTracking = false,
        bool includeAttempts = true,
        CancellationToken ct = default)
    {
        var query = ApplyQueryOptions(ctx.Purchases.AsQueryable(), asTracking, includeAttempts);

        query = query.Where(p => p.UserId == userId);

        // Search
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm!.Trim();
            query = query.Where(p =>
                p.Id.Value.Contains(term) ||
                p.OrderId.Value.Contains(term) ||
                p.ProductId.Contains(term));
        }

        // Date range
        if (filter.StartDate.HasValue)
            query = query.Where(p => p.CreatedAtUtc >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(p => p.CreatedAtUtc <= filter.EndDate.Value);

        // Sorting
        query = ApplySorting(query, filter.SortBy, filter.IsDescending);

        var totalCount = await query.CountAsync(ct);

        var pageNumber = Math.Max(1, filter.PageNumber);
        var pageSize = filter.PageSize;

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Purchase>(items, totalCount, pageNumber, pageSize);
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static IQueryable<Purchase> ApplyQueryOptions(
        IQueryable<Purchase> query,
        bool asTracking,
        bool includeAttempts)
    {
        if (includeAttempts)
            query = query.Include(p => p.ValidationAttempts);

        if (!asTracking)
            query = query.AsNoTracking();

        return query;
    }

    private static IQueryable<Purchase> ApplySorting(
        IQueryable<Purchase> query,
        string? sortBy,
        bool isDescending)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "purchaseatutc" => isDescending
                ? query.OrderByDescending(p => p.PurchaseAtUtc)
                : query.OrderBy(p => p.PurchaseAtUtc),

            "state" => isDescending
                ? query.OrderByDescending(p => p.State)
                : query.OrderBy(p => p.State),

            "store" => isDescending
                ? query.OrderByDescending(p => p.Store)
                : query.OrderBy(p => p.Store),

            "userid" => isDescending
                ? query.OrderByDescending(p => p.UserId)
                : query.OrderBy(p => p.UserId),

            "productid" => isDescending
                ? query.OrderByDescending(p => p.ProductId)
                : query.OrderBy(p => p.ProductId),

            "orderid" => isDescending
                ? query.OrderByDescending(p => p.OrderId.Value)
                : query.OrderBy(p => p.OrderId.Value),

            "id" => isDescending
                ? query.OrderByDescending(p => p.Id.Value)
                : query.OrderBy(p => p.Id.Value),

            _ => isDescending
                ? query.OrderByDescending(p => p.CreatedAtUtc)
                : query.OrderBy(p => p.CreatedAtUtc)
        };
    }
}