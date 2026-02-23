namespace Shop.Domain.Enums
{
    public enum PurchaseState
    {
        /// <summary>
        /// Just logged, not yet validated
        /// </summary>
        LOGGED = 0,

        /// <summary>
        /// In the process of being validated
        /// </summary>
        VALIDATING = 1,

        /// <summary>
        /// Successfully validated
        /// </summary>
        VALID = 2,

        /// <summary>
        /// Permanently bad (signature wrong, product revoked, …)
        /// </summary>
        INVALID = 3,

        /// <summary>
        /// User canceled the purchase
        /// </summary>
        CANCELED = 4,

        /// <summary>
        /// User got their money back
        /// </summary>
        REFUNDED = 5,

        /// <summary>
        /// Temporary failure / network / timeout / store down
        /// </summary>
        FAILED = 6
    }
    
    public enum Store
    {
        GooglePlay = 0,
        Appstore = 1,
        OTHER = 3
    }
}
