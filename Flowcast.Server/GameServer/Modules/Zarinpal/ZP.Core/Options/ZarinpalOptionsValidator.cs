using Microsoft.Extensions.Options;

namespace ZP.Core.Options;

public sealed class ZarinpalOptionsValidator : IValidateOptions<ZarinpalOptions>
{
    public ValidateOptionsResult Validate(string? name, ZarinpalOptions options)
    {
        if (options is null) return ValidateOptionsResult.Fail("Zarinpal options not found");
        if (string.IsNullOrWhiteSpace(options.MerchantId)) return ValidateOptionsResult.Fail("Zarinpal MerchantId is required");
        if (string.IsNullOrWhiteSpace(options.BaseUrl)) return ValidateOptionsResult.Fail("Zarinpal BaseUrl is required");
        if (string.IsNullOrWhiteSpace(options.CallbackBaseUrl)) return ValidateOptionsResult.Fail("Zarinpal CallbackBaseUrl is required");
        return ValidateOptionsResult.Success;
    }
}
