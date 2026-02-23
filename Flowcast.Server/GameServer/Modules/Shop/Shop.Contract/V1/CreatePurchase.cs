namespace Shop.Contracts.V1
{
    public static class CreatePurchase
    {
        public const string Method = "POST";
        public const string Route = "shop/purchases";

        public const string Summary = "Log a purchase";
        public const string Description = "Log purchase for a player.";

        public record Request(
             string OrderId,
             string Store,
             string PurchaseToken,
             string? Signature,
             string ProductId,
             string Receipt,
             string Payload,
             string UserId,
             DateTimeOffset PurchaseAtUtc,
             bool IsSandbox,
             Dictionary<string, string>? Metadata = null
         );


        public record Response(
            string PurchaseId
        );
    }
}
