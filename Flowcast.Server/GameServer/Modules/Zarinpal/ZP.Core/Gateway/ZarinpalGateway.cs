using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZP.Core.Options;

namespace ZP.Core.Gateway;

public sealed class ZarinpalGateway(
    HttpClient httpClient, 
    IOptions<ZarinpalOptions> options,
    ILogger<ZarinpalGateway> logger
    ) : IZarinpalGateway
{
    private const int SuccessCode = 100;
    private const int AlreadyVerifiedCode = 101;
    private readonly ILogger<ZarinpalGateway> _logger = logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };
    private readonly HttpClient _httpClient = httpClient;
    private readonly ZarinpalOptions _options = options.Value;


    /// <summary>
    /// Send payment request to Zarin Pal ^^
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="callbackUrl"></param>
    /// <param name="description"></param>
    /// <param name="metadata"></param>
    /// <param name="currency"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<ZarinpalRequestResult> RequestPaymentAsync(
        int amount,
        string callbackUrl,
        string description,
        ZarinpalMetadata? metadata,
        string? currency,
        CancellationToken ct = default)
    {
        /*--------- Create new zarinpal request payload ---------*/
        var payload = new ZarinpalRequestPayload
        {
            MerchantId = _options.MerchantId!,
            Amount = amount,
            CallbackUrl = callbackUrl,
            Description = description,
            Currency = currency,
            Metadata = metadata
        };

        /*--------- Get the response ---------*/
        var response = await _httpClient.PostAsJsonAsync("pg/v4/payment/request.json", payload, JsonOptions, ct);
        var json = await response.Content.ReadAsStringAsync(ct);

        /*--------- check response ---------*/
        if(!response.IsSuccessStatusCode)
            _logger.LogWarning("Zarinpal request failed {StatusCode}: {Response}", response.StatusCode, json);

        /*--------- create result ---------*/
        var result = JsonSerializer.Deserialize<ZarinpalRequestResponse>(json, JsonOptions);
        
        /*--------- Check result ---------*/
        if(result?.Data is null)
        {
            var msg = TryGetErrorMessage(result?.Errors) ?? TryGetMessageFromRaw(json) ?? (response.IsSuccessStatusCode ? "Invalid response from gateway" : $"Zarinpal returned {(int)response.StatusCode}. Check server logs for response body.");
            return new ZarinpalRequestResult(false, null, -1, msg);
        }
       
        var success = result.Data.Code == SuccessCode && !string.IsNullOrEmpty(result.Data.Authority);
       
        return new ZarinpalRequestResult(success, result.Data.Authority, result.Data.Code, result.Data.Message ?? (success ? null : "Payment request failed"));
    }
    /// <summary>
    /// Verify zarinpal payment o_o
    /// </summary>
    /// <param name="authority"></param>
    /// <param name="amount"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<ZarinpalVerifyResult> VerifyPaymentAsync(string authority, int amount, CancellationToken ct = default)
    {
        var payload = new ZarinpalVerifyPayload
        {
            MerchantId = _options.MerchantId!,
            Amount = amount,
            Authority = authority
        };
        /*--------- get response as json ---------*/
        var response = await _httpClient.PostAsJsonAsync("pg/v4/payment/verify.json", payload, JsonOptions, ct);
        var json = await response.Content.ReadAsStringAsync(ct);

        /*--------- Create response and deserialize it -_- ---------*/
        var result = JsonSerializer.Deserialize<ZarinpalRequestResponse>(json, JsonOptions);
        if (result?.Data is null)
            return new ZarinpalVerifyResult(false, null, -1, "Invalid response from gateway");

        var success = result.Data.Code == SuccessCode || result.Data.Code == AlreadyVerifiedCode;
        return new ZarinpalVerifyResult(success, result.Data.RefId, result.Data.Code, result.Data.Message);
    }

    /// <summary>
    /// Get the error message o_0
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    private static string? TryGetErrorMessage(JsonElement? errors)
    {
        if (!errors.HasValue) return null;
        var v = errors.Value;
        if (v.ValueKind == JsonValueKind.Array)
        {
            foreach (var e in v.EnumerateArray())
            {
                if (e.TryGetProperty("message", out var m)) return m.GetString();
                if (e.TryGetProperty("description", out var d)) return d.GetString();
            }
        }
        if (v.ValueKind == JsonValueKind.Object && v.TryGetProperty("message", out var msg)) return msg.GetString();
        return null;
    }

    /// <summary>
    /// Get message from raw
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    private static string? TryGetMessageFromRaw(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("errors", out var err))
                return TryGetErrorMessage(err) ?? err.ToString();
            if (root.TryGetProperty("data", out var data) && data.TryGetProperty("message", out var m))
                return m.GetString();
        }
        catch { }
        return null;
    }
}
