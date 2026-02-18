using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Shared.Presentation.Endpoints;
using System.Text;
using ZP.Core.Commands;
using ZP.Core.Options;
using Shared.Application.Messaging;

namespace ZP.Apphost.Endpoints.V1;

public sealed class PaymentCallbackEndpoint : IEndpoint
{
    public const string Route = "zarinpal/payment/callback";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(Route,
                async (
                    string authority,
                    string? status,
                    ICommandHandler<HandlePaymentCallbackCommand, CallbackHandleResult> handler,
                    IOptions<ZarinpalOptions> options,
                    CancellationToken ct) =>
                {
                    var result = await handler.Handle(
                        new HandlePaymentCallbackCommand(authority, status ?? string.Empty),
                        ct);
                    var data = result.IsFailure
                        ? new CallbackHandleResult(false, string.Empty, null)
                        : result.Value;
                    var baseUrl = options.Value.AppReturnBaseUrl?.TrimEnd('/') ?? string.Empty;
                    var returnUrl = string.IsNullOrEmpty(baseUrl)
                        ? "#"
                        : $"{baseUrl}?orderId={Uri.EscapeDataString(data.OrderId)}&success={data.Success}&refId={data.RefId?.ToString() ?? ""}";
                    var html = BuildResultHtml(data.Success, data.OrderId, data.RefId, returnUrl);
                    return Results.Content(html, "text/html", Encoding.UTF8);
                })
            .WithTags("Zarinpal")
            .WithSummary("Payment callback")
            .WithDescription("Zarinpal redirects here after payment; returns HTML result with link back to app.")
            .MapToApiVersion(1.0);
    }

    private static string BuildResultHtml(bool success, string orderId, long? refId, string returnUrl)
    {
        var title = success ? "پرداخت موفق" : "پرداخت ناموفق";
        var message = success
            ? $"سفارش {orderId} با موفقیت پرداخت شد." + (refId.HasValue ? $" کد پیگیری: {refId}" : "")
            : $"پرداخت برای سفارش {orderId} انجام نشد یا با خطا مواجه شد.";
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html dir='rtl' lang='fa'><head><meta charset='utf-8'/><meta name='viewport' content='width=device-width,initial-scale=1'/>");
        sb.Append($"<title>{title}</title></head><body style='font-family:sans-serif;padding:2rem;text-align:center;'>");
        sb.Append($"<h1>{title}</h1><p>{message}</p>");
        if (returnUrl != "#")
            sb.Append($"<p><a href=\"{returnUrl}\">بازگشت به برنامه</a></p>");
        sb.Append("</body></html>");
        return sb.ToString();
    }
}
