namespace ZP.Core.Options;

public sealed class ZarinpalOptions
{
    public const string SectionName = "Zarinpal";
    public bool UseInMemoryDatabase { get; init; } = false;
    public string? ConnectionStrings { get; init; }
    public string? MerchantId { get; init; }
    public string? BaseUrl { get; init; }
    public string? CallbackBaseUrl { get; init; }
    public string? AppReturnBaseUrl { get; init; }
}
