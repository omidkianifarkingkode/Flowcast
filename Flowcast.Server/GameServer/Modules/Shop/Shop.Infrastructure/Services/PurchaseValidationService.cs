using SharedKernel;
using Shop.Application.Interfaces;
using Shop.Domain.Entities;
using Shop.Domain.Enums;
using Shop.Domain.Shared;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Shop.Infrastructure.Services;

public class PurchaseValidationService : IPurchaseValidationService
{
    private readonly HttpClient _httpClient;
    private const string VerifyReceiptUrl = "api/VerifyReceipt";

    public PurchaseValidationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<PurchaseState>> ValidateAsync(Purchase purchase, CancellationToken ct = default)
    {
        // Build request matching sample format
        var request = new VerifyReceiptRequest
        {
            GameIdentifier = "tacticaldefence",             // ← hardcoded or from config/env
            ProviderName = purchase.Store.ToString(),       // GooglePlay / Apple etc.
            ProductId = purchase.ProductId,
            OrderId = purchase.OrderId.Value,
            Signature = purchase.Signature?.Value ?? "",
            Price = 0m,                                     // ← missing in entity → add to Purchase or config
            ReceiptData = purchase.Receipt ?? "",
            CallProviderName = "",                          // ← unclear, perhaps leave empty
            CurrencyCode = "EUR",                           // ← hardcoded or from metadata/config
            PayLoad = purchase.Payload ?? "",
            UserId = purchase.UserId,
            PurchaseAtUtc = purchase.PurchaseAtUtc,
            IsSandbox = purchase.IsSandbox
        };

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                VerifyReceiptUrl,
                request,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase },
                ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(ct);
                return Result.Failure<PurchaseState>(MapHttpError(response, errorContent));
            }

            var content = await response.Content.ReadFromJsonAsync<VerifyReceiptResponse>(ct);
            if (content == null)
                return Result.Failure<PurchaseState>(
                    Error.Failure("Store.InvalidResponse", "Empty or invalid response from verification service"));

            if (content.IsPurchaseValid)
                return PurchaseState.VALID;

            if (content.IsRefunded)
                return PurchaseState.REFUNDED;

            // Failure cases
            var error = content.ParsedContent;
            var errorMsg = error is not null ? error.error : "Unknown validation failure";

            return Result.Failure<PurchaseState>(Error.Validation(
                "Store.InvalidReceipt",
                $"Receipt verification failed: {errorMsg} (Status: {content.Status})"));
        }
        catch (HttpRequestException ex) when (ex.StatusCode.HasValue)
        {
            return Result.Failure<PurchaseState>(MapHttpError(new HttpResponseMessage(ex.StatusCode.Value), ex.Message));
        }
        catch (Exception ex)
        {
            return Result.Failure<PurchaseState>(Error.Failure("Store.ValidationUnexpected", ex.Message));
        }
    }

    private static Error MapHttpError(HttpResponseMessage response, string? content = null)
    {
        var status = (int)response.StatusCode;

        return status switch
        {
            400 => Error.Validation("Store.BadRequest", "Invalid receipt data"),
            401 => Error.Unauthorized("Store.Unauthorized", "API authentication failed"),
            403 => Error.Forbidden("Store.Forbidden", "Access denied to verification endpoint"),
            404 => Error.NotFound("Store.EndpointNotFound", "Verification service endpoint not found"),
            >= 500 => Error.Failure("Store.ServerError", $"Verification service internal error (HTTP {status})"),
            _ => Error.Failure("Store.VerifyReceiptFailed", $"Unexpected status code: {status}. Content: {content ?? "none"}")
        };
    }

    // Request DTO matching sample
    private sealed record VerifyReceiptRequest
    {
        public string GameIdentifier { get; init; } = string.Empty;
        public string ProviderName { get; init; } = string.Empty;
        public string ProductId { get; init; } = string.Empty;
        public string OrderId { get; init; } = string.Empty;
        public string Signature { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public string ReceiptData { get; init; } = string.Empty;
        public string CallProviderName { get; init; } = string.Empty;
        public string CurrencyCode { get; init; } = string.Empty;
        public string PayLoad { get; init; } = string.Empty;
        public string UserId { get; init; } = string.Empty;
        public DateTimeOffset PurchaseAtUtc { get; init; }
        public bool IsSandbox { get; init; }
    }

    // Response DTO based on sample
    private sealed record VerifyReceiptResponse
    {
        public bool Success { get; init; }
        public string? Content { get; init; }
        public string? Message { get; init; }
        public int Status { get; init; }
        public string? Error { get; init; }
        public string? ErrorDescription { get; init; }
        public bool IsPurchaseForTest { get; init; }
        public bool IsRefunded { get; init; }
        public bool IsConsumed { get; init; }
        public bool IsPurchaseValid { get; init; }
        public string? TransactionId { get; init; }

        public VerificationContent? ParsedContent => Content is null ? null : TryParseContent(Content);

        private static VerificationContent? TryParseContent(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<VerificationContent>(json);
            }
            catch
            {
                return null;
            }
        }
    }

    private sealed record VerificationContent
    {
        public string? error { get; init; }
        public bool status { get; init; }
        public object? payload { get; init; }
        public string? notice { get; init; }
    }
}