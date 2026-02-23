namespace Shop.Contracts.V1;

public static class ValidatePurchase
{
    public const string Method = "POST";
    public const string Route = "shop/admin/purchases/{purchaseId}/validate";

    public const string Summary = "Validate a purchase";
    public const string Description = "Manually triggers validation for a purchase.";

    public record Request();
    public record Response(
        string PurchaseId,
        string State,
        DateTimeOffset? UpdatedAtUtc,
        int ValidationAttemptsCount
    );
}
