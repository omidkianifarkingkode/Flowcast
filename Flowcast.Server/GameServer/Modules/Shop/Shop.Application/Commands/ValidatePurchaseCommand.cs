using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Application.Messaging;
using Shared.Application.Services;
using SharedKernel;
using Shop.Application.Interfaces;
using Shop.Application.Repositories;
using Shop.Domain.Entities;
using Shop.Domain.Shared;

namespace Shop.Application.Commands;

public sealed record ValidatePurchaseCommand(PurchaseId PurchaseId) : ICommand<ValidatePurchaseResult>;

public record ValidatePurchaseResult(
        string PurchaseId,
        string State,
        DateTimeOffset? UpdatedAtUtc,
        int ValidationAttemptsCount
    );

public class ValidatePurchaseCommandHandler(
    IPurchaseRepository purchases,
    IPurchaseValidationService validator,
    [FromKeyedServices("shop")] IUnitOfWork uow,
    IDateTimeProvider clock,
    ILogger<ValidatePurchaseCommandHandler> logger) : ICommandHandler<ValidatePurchaseCommand, ValidatePurchaseResult>
{
    public async Task<Result<ValidatePurchaseResult>> Handle(ValidatePurchaseCommand command, CancellationToken ct)
    {
        // 1. Load with attempts (needed for BeginValidation)
        var purchase = await purchases.GetById(
            command.PurchaseId,
            asTracking: true,           // we will modify → need tracking
            includeAttempts: true,
            ct);

        if (purchase is null)
            return Result.Failure<ValidatePurchaseResult>(PurchaseErrors.PurchaseNotFound(command.PurchaseId.Value));

        var now = clock.UtcNowOffset;

        // 2. Try to start validation
        var beginResult = purchase.BeginValidation(now);

        if (beginResult.IsFailure)
        {
            logger.LogWarning(
                "Cannot start validation for purchase {PurchaseId}. State={State}, Attempts={AttemptCount}, Error={ErrorCode} - {Message}",
                purchase.Id.Value,
                purchase.State,
                purchase.ValidationAttempts.Count,
                beginResult.Error?.Code ?? "unknown",
                beginResult.Error?.Description ?? "no description");

            return Result.Failure<ValidatePurchaseResult>(beginResult.Error!);
        }

        // 3. Persist VALIDATING + new attempt (short transaction)
        var saveBegin = await uow.SaveChangesAsync(ct);
        if (saveBegin.IsFailure)
        {
            logger.LogError("Failed to persist VALIDATING state for {PurchaseId}", purchase.Id.Value);
            return Result.Failure<ValidatePurchaseResult>(saveBegin.Error!);
        }

        logger.LogInformation(
            "Validation started for {PurchaseId} - attempt #{Attempt}",
            purchase.Id.Value,
            purchase.ValidationAttempts.Count);

        // 4. Perform external validation
        var validationOutcome = await validator.ValidateAsync(purchase, ct);

        // 5. Submit final result
        var submitResult = purchase.SubmitValidation(
            new ValidationResult(
                validationOutcome.IsSuccess,
                validationOutcome.Value,
                validationOutcome.Error?.Code,
                validationOutcome.Error?.Description),
            now);

        if (submitResult.IsFailure)
        {
            // Very rare — domain invariant broken
            return Result.Failure<ValidatePurchaseResult>(submitResult.Error!);
        }

        // 6. Persist final state (second short transaction)
        var saveFinal = await uow.SaveChangesAsync(ct);
        if (saveFinal.IsFailure)
        {
            logger.LogError(
                "Failed to persist final validation result for {PurchaseId} - state left as VALIDATING",
                purchase.Id.Value);

            // Here you could add alerting / compensation logic
            return Result.Failure<ValidatePurchaseResult>(saveFinal.Error!);
        }

        // 7. Build success result
        var finalResult = new ValidatePurchaseResult(
            PurchaseId: purchase.Id.Value,
            State: purchase.State.ToString(),
            UpdatedAtUtc: purchase.ModifiedAtUtc ?? now,
            ValidationAttemptsCount: purchase.ValidationAttempts.Count
        );

        logger.LogInformation(
            "Validation completed for {PurchaseId} → {FinalState} (attempts: {Attempts})",
            purchase.Id.Value, purchase.State, purchase.ValidationAttempts.Count);

        return Result.Success(finalResult);
    }
}