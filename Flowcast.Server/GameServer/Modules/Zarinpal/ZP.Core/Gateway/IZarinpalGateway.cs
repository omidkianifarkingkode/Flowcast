namespace ZP.Core.Gateway;

public interface IZarinpalGateway
{
    Task<ZarinpalRequestResult> RequestPaymentAsync(
        int amount,
        string callbackUrl,
        string description,
        ZarinpalMetadata? metadata,
        string? currency,
        CancellationToken ct = default);

    Task<ZarinpalVerifyResult> VerifyPaymentAsync(
        string authority,
        int amount,
        CancellationToken ct = default);
}

public sealed record ZarinpalRequestResult(bool Success, string? Authority, int Code, string? Message);

public sealed record ZarinpalVerifyResult(bool Success, long? RefId, int Code, string? Message);
