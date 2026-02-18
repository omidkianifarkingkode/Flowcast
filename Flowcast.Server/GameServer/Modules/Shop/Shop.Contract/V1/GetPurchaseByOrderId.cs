namespace Shop.Contracts.V1;

public static class GetPurchaseByOrderId
{
    public const string Method = "GET";
    public const string Route = "shop/admin/purchases/orders/{orderId}";

    public const string Summary = "Get purchase report by order id";
    public const string Description = "Return purchase report for a specific order id (admin).";

    public record Request(string OrderId);

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
