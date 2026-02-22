namespace Shop.Contracts.V1;

public static class GetPurchaseList
{
    public const string Method = "GET";
    public const string Route = "shop/purchase-list";

    public const string Summary = "Get purchases list report";
    public const string Description = "Return purchase list reposrts with all property for test";

    public record Request();

    public sealed record Response(
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
}
