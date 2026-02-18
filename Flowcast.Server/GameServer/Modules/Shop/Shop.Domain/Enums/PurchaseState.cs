namespace Shop.Domain.Enums
{
    public enum PurchaseState
    {
        LOGGED = 0,
        VALIDATING = 1,
        VALID = 2,
        INVALID = 3,
        CANCELED = 4,
        REFUNDED = 5,
        FAILED = 6
    }
    
}
