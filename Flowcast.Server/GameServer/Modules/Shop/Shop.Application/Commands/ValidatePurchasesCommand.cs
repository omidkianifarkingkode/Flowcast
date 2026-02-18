using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Application.Messaging;
using Shared.Application.Services;
using SharedKernel;
using Shop.Application.Interfaces;
using Shop.Application.Repositories;
using Shop.Domain.Entities;

namespace Shop.Application.Commands;

public sealed record ValidatePurchasesCommand(int BatchSize = 50) : ICommand;

public class ValidatePurchasesCommandHandler(
    IPurchaseRepository purchases,
    IPurchaseValidationService validator,
    [FromKeyedServices("shop")] IUnitOfWork uow,
    IDateTimeProvider clock,
    ILogger<ValidatePurchasesCommandHandler> logger)
    : ICommandHandler<ValidatePurchasesCommand>
{
    public async Task<Result> Handle(ValidatePurchasesCommand command, CancellationToken ct)
    {
        var batchSize = Math.Clamp(command.BatchSize, 1, 200);

        var pending = await purchases.GetPendingValidations(
            batchSize,
            clock.UtcNowOffset,
            asTracking: true,
            includeAttempts: true,
            ct);

        if (pending.Count == 0)
        {
            logger.LogDebug("No pending purchases to validate");
            return Result.Success();
        }

        int processed = 0, succeeded = 0, failed = 0, skipped = 0;

        foreach (var purchase in pending)
        {
            ct.ThrowIfCancellationRequested();

            var purchaseIdStr = purchase.Id.Value;

            // Phase 1: Try to start validation
            var beginResult = purchase.BeginValidation(clock.UtcNowOffset);

            if (beginResult.IsFailure)
            {
                logger.LogDebug("Skipped {PurchaseId} - {Reason} (state={State})",
                    purchaseIdStr, beginResult.Error?.Description, purchase.State);

                skipped++;
                continue;
            }

            // Persist VALIDATING + new attempt (short tx)
            var save1 = await uow.SaveChangesAsync(ct);
            if (save1.IsFailure)
            {
                logger.LogWarning("Failed to save VALIDATING state {PurchaseId} - {Error}",
                    purchaseIdStr, save1.Error?.Description);
                failed++;
                continue;
            }

            processed++;

            // Phase 2: External validation
            var outcome = await validator.ValidateAsync(purchase, ct);

            // Phase 3: Submit result (do not access outcome.Value when outcome.IsFailure)
            var validationResult = outcome.IsSuccess
                ? new ValidationResult(true, outcome.Value, null, null)
                : ValidationResult.Failure(outcome.Error?.Code ?? "ValidationFailed", outcome.Error?.Description ?? "External verification failed");
            var submitResult = purchase.SubmitValidation(validationResult, clock.UtcNowOffset);

            if (submitResult.IsFailure)
            {
                logger.LogError("Submit failed for {PurchaseId} - {Error}", purchaseIdStr, submitResult.Error?.Description);
                failed++;
                continue;
            }

            // Persist final state (short tx)
            var save2 = await uow.SaveChangesAsync(ct);
            if (save2.IsFailure)
            {
                logger.LogError("Final save failed for {PurchaseId} - left in VALIDATING - {Error}",
                    purchaseIdStr, save2.Error?.Description);
                failed++;
                continue;
            }

            succeeded++;

            logger.LogInformation("Validated {PurchaseId} → {State}", purchaseIdStr, purchase.State);
        }

        logger.LogInformation(
            "Batch finished - processed: {p}, succeeded: {s}, failed: {f}, skipped: {sk}",
            processed, succeeded, failed, skipped);

        return Result.Success();
    }
}