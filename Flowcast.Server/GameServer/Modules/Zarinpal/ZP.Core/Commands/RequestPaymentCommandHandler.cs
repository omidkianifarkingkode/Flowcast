using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Application.Messaging;
using Shared.Application.Services;
using SharedKernel;
using ZP.Contracts;
using ZP.Core.Entities;
using ZP.Core.Gateway;
using ZP.Core.Options;
using ZP.Core.Repositories;

namespace ZP.Core.Commands;

public sealed class RequestPaymentCommandHandler(
    IPaymentRequestRepository repository,
    IUnitOfWork unitOfWork,
    IZarinpalGateway gateway,
    IOptions<ZarinpalOptions> options,
    IDateTimeProvider dateTimeProvider,
    ILogger<RequestPaymentCommandHandler> logger) : ICommandHandler<RequestPaymentCommand, RequestPaymentCommandResult>
{
    public const string CallbackPath = "/api/v1/zarinpal/payment/callback";
    private const string StartPayPath = "/pg/StartPay/";

    public async Task<Result<RequestPaymentCommandResult>> Handle(RequestPaymentCommand command, CancellationToken ct)
    {
        var opts = options.Value;

        /*----------- Idempotency -----------*/
        if (!string.IsNullOrWhiteSpace(command.IdempotencyKey))
        {
            var existing = await repository.GetByIdempotencyKeyAsync(command.IdempotencyKey.Trim(), ct);
            if (existing is not null)
            {
                logger.LogInformation("Idempotent request: returning existing OrderId {OrderId} for Key {Key}", existing.OrderId.Value, command.IdempotencyKey);
               
                var existingUrl = opts.BaseUrl!.TrimEnd('/') + StartPayPath + existing.Authority.Value;
                var existingResponse = new RequestPaymentResponse(existingUrl, existing.Authority.Value, existing.OrderId.Value);
                
                return Result.Success(new RequestPaymentCommandResult(existingResponse, Created: false));
            }
        }
        /*----------- Payment Request -----------*/
        var orderId = OrderId.New();
       
        logger.LogInformation("Payment request initiated for UserId {UserId}, Amount {Amount}, OrderId {OrderId}", command.UserId, command.Amount, orderId.Value);
        
        var callbackUrl = opts.CallbackBaseUrl!.TrimEnd('/') + CallbackPath;
        var metadata = new ZarinpalMetadata
        {
            Mobile = string.IsNullOrWhiteSpace(command.Mobile) ? null : command.Mobile,
            Email = string.IsNullOrWhiteSpace(command.Email) ? null : command.Email,
            OrderId = orderId.Value
        };
        var gatewayResult = await gateway.RequestPaymentAsync(
            command.Amount,
            callbackUrl,
            command.Description,
            metadata,
            "IRR",
            ct);
        if (!gatewayResult.Success || string.IsNullOrEmpty(gatewayResult.Authority))
            return Result.Failure<RequestPaymentCommandResult>(Error.Failure("ZarinpalGateway", gatewayResult.Message ?? "Payment request failed."));

        /*----------- Create payment Result -----------*/
        var authority = Authority.Create(gatewayResult.Authority);
        var id = PaymentRequestId.New();
        var createResult = PaymentRequest.Create(id, authority, orderId, command.UserId, command.Amount, command.Description, dateTimeProvider, "IRT", command.IdempotencyKey);
      
        if (createResult.IsFailure)
            return Result.Failure<RequestPaymentCommandResult>(createResult.Error);
        var paymentRequest = createResult.Value;
        repository.Add(paymentRequest);

        /*----------- Check save result -----------*/
        var saveResult = await unitOfWork.SaveChangesAsync(ct);
        
        if (saveResult.IsFailure)
            return Result.Failure<RequestPaymentCommandResult>(saveResult.Error);
        var paymentUrl = opts.BaseUrl!.TrimEnd('/') + StartPayPath + gatewayResult.Authority;
        var response = new RequestPaymentResponse(paymentUrl, gatewayResult.Authority, orderId.Value);
        
        return Result.Success(new RequestPaymentCommandResult(response, Created: true));
    }
}
