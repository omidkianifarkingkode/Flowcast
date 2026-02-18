using Microsoft.Extensions.Logging;
using Shared.Application.Messaging;
using Shared.Application.Services;
using SharedKernel;
using ZP.Core.Entities;
using ZP.Core.Gateway;
using ZP.Core.Repositories;

namespace ZP.Core.Commands;

public sealed class HandlePaymentCallbackCommandHandler(
    IPaymentRequestRepository repository,
    IUnitOfWork unitOfWork,
    IZarinpalGateway gateway,
    IDateTimeProvider dateTimeProvider,
    ILogger<HandlePaymentCallbackCommandHandler> logger) : ICommandHandler<HandlePaymentCallbackCommand, CallbackHandleResult>
{
    public async Task<Result<CallbackHandleResult>> Handle(HandlePaymentCallbackCommand command, CancellationToken ct)
    {
        var authority = command.Authority;
        var status = command.Status;
        /*--------- Check authority from callback ---------*/
        if(string.IsNullOrWhiteSpace(authority))
        {
            logger.LogWarning("Payment callback: empty authority");
            return Result.Success(new CallbackHandleResult(false, string.Empty, null));
        }

        logger.LogInformation("Payment callback received Authority {Authority}, Status {Status}", authority, status);
        /*--------- Get autority from Db ^^ ---------*/
        var pr = await repository.GetByAuthorityAsync(authority, ct);
        if(pr is null)
        {
            logger.LogWarning("Payment callback: no request found for Authority {Authority}", authority);
            return Result.Success(new CallbackHandleResult(false, string.Empty, null));
        }


        /*--------- Verifing the Payment ---------*/
        var isOkStatus = status == "OK";
        long? verifiedRefId = null;
        if(isOkStatus)
        {
            var verifyResult = await gateway.VerifyPaymentAsync(authority, pr.Amount, ct);
            if(verifyResult.Success && verifyResult.RefId.HasValue)
                verifiedRefId = verifyResult.RefId;
            else
                logger.LogWarning("Payment callback: verify failed for OrderId {OrderId}", pr.OrderId.Value);
        }

        var outcomeResult = pr.ProcessCallback(isOkStatus, verifiedRefId, dateTimeProvider.UtcNowOffset);
        if(outcomeResult.IsFailure)
            return Result.Failure<CallbackHandleResult>(outcomeResult.Error);

        var outcome = outcomeResult.Value;

        /*--------- check the outcome result from process callback (o_0) ---------*/
        var saveResult = await unitOfWork.SaveChangesAsync(ct);
        if(saveResult.IsFailure)
        {
            logger.LogError("Payment callback: failed to persist for Authority {Authority}", authority);
            return Result.Success(new CallbackHandleResult(false, pr.OrderId.Value, null));
        }

        if(outcome.Success)
            logger.LogInformation("Payment callback: completed OrderId {OrderId}, RefId {RefId}", pr.OrderId.Value, outcome.RefId);

        return Result.Success(new CallbackHandleResult(outcome.Success, pr.OrderId.Value, outcome.RefId));
    }
}
