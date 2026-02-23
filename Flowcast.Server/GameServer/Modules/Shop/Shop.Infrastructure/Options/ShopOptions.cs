namespace Shop.Infrastructure.Options
{
    public sealed class ShopOptions
    {
        public const string SectionName = "Shop";

        public bool UseInMemoryDatabase { get; init; } = false;
        public string? ConnectionString { get; init; }
    }
}
