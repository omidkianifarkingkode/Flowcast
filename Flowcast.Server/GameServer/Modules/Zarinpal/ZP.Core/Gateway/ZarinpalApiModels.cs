using System.Text.Json;
using System.Text.Json.Serialization;

namespace ZP.Core.Gateway;

public sealed class ZarinpalRequestPayload
{
    [JsonPropertyName("merchant_id")]
    public string MerchantId { get; set; } = string.Empty;
    [JsonPropertyName("amount")]
    public int Amount { get; set; }
    [JsonPropertyName("callback_url")]
    public string CallbackUrl { get; set; } = string.Empty;
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
    [JsonPropertyName("metadata")]
    public ZarinpalMetadata? Metadata { get; set; }
}

public sealed class ZarinpalMetadata
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("mobile")]
    public string? Mobile { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("email")]
    public string? Email { get; set; }
    [JsonPropertyName("order_id")]
    public string? OrderId { get; set; }
}

public sealed class ZarinpalRequestResponse
{
    [JsonPropertyName("data")]
    public ZarinpalData? Data { get; set; }
    [JsonPropertyName("errors")]
    public JsonElement? Errors { get; set; }
}

public sealed class ZarinpalData
{
    [JsonPropertyName("code")]
    public int Code { get; set; }
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    [JsonPropertyName("authority")]
    public string? Authority { get; set; }
    [JsonPropertyName("ref_id")]
    public long? RefId { get; set; }
}

public sealed class ZarinpalVerifyPayload
{
    [JsonPropertyName("merchant_id")]
    public string MerchantId { get; set; } = string.Empty;
    [JsonPropertyName("amount")]
    public int Amount { get; set; }
    [JsonPropertyName("authority")]
    public string Authority { get; set; } = string.Empty;
}
