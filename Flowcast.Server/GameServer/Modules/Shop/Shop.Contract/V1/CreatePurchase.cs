using Shop.Domain.Entities;
using Shop.Domain.Enums;

namespace Shop.Contract.V1
{
    public static class CreatePurchase
    {
        public const string Method = "POST";
        public const string Route = "shop/purchases";

        public const string Summary = "Create a purchase";
        public const string Description = "Creates a pending purchase for a player.";

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
             Dictionary<string, object>? Metadata = null
         );


        public record Response(
            string PurchaseId
        );
    }
}
