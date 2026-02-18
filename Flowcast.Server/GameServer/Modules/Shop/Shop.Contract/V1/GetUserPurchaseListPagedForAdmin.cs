using SharedKernel.Pagination;

namespace Shop.Contracts.V1;

public static class GetUserPurchaseListPagedForAdmin
{
    public const string Method = "GET";
    public const string Route = "shop/admin/purchases/users/{userId}";

    public const string Summary = "Get user purchases list report (paged)";
    public const string Description = "Return paged purchase list for a specific user (admin).";

    public record Request(
        string UserId,
        int PageNumber = 1,
        int PageSize = 10,
        string? SortBy = null,
        bool IsDescending = false,
        string? SearchTerm = null,
        DateTime? StartDate = null,
        DateTime? EndDate = null
    );

    public record Item(
        string Id,
        string OrderId,
        string Store,
        string ProductId,
        string UserId,
        string State,
        DateTimeOffset PurchaseAtUtc,
        DateTimeOffset CreatedAtUtc,
        int ValidationAttemptsCount
    );

    public record Response(PagedResult<Item> Data);
}