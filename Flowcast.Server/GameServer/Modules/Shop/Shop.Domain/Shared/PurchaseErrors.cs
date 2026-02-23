using SharedKernel;
using Shop.Domain.Entities;

namespace Shop.Domain.Shared;

public static class PurchaseErrors
{
    // ── Creation / Factory validation errors ─────────────────────────────────

    public static readonly Error InvalidPurchaseId =
        Error.Validation("Purchase.InvalidId", "Purchase ID cannot be empty or whitespace.");

    public static readonly Error InvalidProductId =
        Error.Validation("Purchase.InvalidProductId", "Product ID cannot be empty.");

    public static readonly Error InvalidReceipt =
        Error.Validation("Purchase.InvalidReceipt", "Receipt cannot be empty.");

    public static readonly Error InvalidPayload =
        Error.Validation("Purchase.InvalidPayload", "Payload cannot be empty.");

    public static readonly Error InvalidUserId =
        Error.Validation("Purchase.InvalidUserId", "User ID cannot be empty.");

    // ── State machine / validation flow errors ───────────────────────────────

    public static readonly Error InvalidStateForValidation =
        Error.Conflict(
            "Purchase.InvalidState",
            "Validation can only be started from LOGGED or FAILED states."
        );

    public static readonly Error MaxValidationAttemptsExceeded =
        Error.Conflict(
            "Purchase.MaxRetriesExceeded",
            $"Maximum number of validation attempts ({Purchase.MaxValidationAttempts}) has been reached."
        );

    public static readonly Error ForceRequiredAfterMaxAttempts =
        Error.Conflict(
            "Purchase.ForceRequired",
            $"Maximum validation attempts ({Purchase.MaxValidationAttempts}) reached. Use force=true to attempt again."
        );

    public static readonly Error CannotStartWhileAlreadyValidating =
        Error.Conflict(
            "Purchase.AlreadyValidating",
            "A validation attempt is already in progress."
        );

    // ── Terminal / business outcome errors ───────────────────────────────────

    public static readonly Error PurchaseAlreadyValidated =
        Error.Conflict(
            "Purchase.AlreadyValid",
            "This purchase has already been successfully validated."
        );

    public static readonly Error PurchaseInvalidated =
        Error.Conflict(
            "Purchase.Invalid",
            "This purchase has been marked as invalid after validation attempts."
        );

    // ── Timeout / stuck attempt related ──────────────────────────────────────

    public static readonly Error ValidationAttemptTimedOut =
        Error.Failure(
            "Purchase.ValidationTimeout",
            $"Validation attempt timed out after {Purchase.ValidationTimeout.TotalMinutes} minutes."
        );

    // ── Authorization / ownership checks (if needed later) ───────────────────

    public static readonly Error PurchaseNotOwnedByUser =
        Error.Unauthorized(
            "Purchase.NotOwned",
            "The purchase does not belong to the current user."
        );

    public static Error PurchaseNotFound(string purchaseId) =>
        Error.NotFound(
            "Purchase.NotFound",
            $"Purchase with the specified ID:{purchaseId} was not found."
        );

    public static Error PurchaseNotFoundByOrderId(OrderId orderId) =>
        Error.NotFound(
            "Purchase.NotFound",
            $"Purchase with the specified ID:{orderId.Value} was not found."
        );

    // Validation flow specific

    public static Error CannotStartValidation(string currentState) =>
        Error.Conflict(
            "Purchase.InvalidStateForValidation",
            $"Validation can only start from LOGGED or qualifying FAILED states. Current state: {currentState}");

    public static Error MaxValidationAttemptsReached(int maxAttempts) =>
        Error.Conflict(
            "Purchase.MaxAttemptsReached",
            $"Maximum validation attempts ({maxAttempts}) reached. Force flag required to continue.");

    public static Error NoPendingValidationAttempt =>
        Error.Conflict(
            "Purchase.NoPendingAttempt",
            "Cannot submit validation result — no active validation attempt found.");

    public static Error AlreadyValidating =>
        Error.Conflict(
            "Purchase.AlreadyValidating",
            "A validation attempt is already in progress.");

    public static Error ValidationAlreadyFinalized(string state) =>
        Error.Conflict(
            "Purchase.AlreadyFinalized",
            $"Cannot start new validation — purchase is already in terminal state {state}.");

    public static Error ValidationServiceFailure(string? code, string? message) =>
        Error.Failure(
            code ?? "Purchase.ValidationFailed",
            message ?? "External validation service returned failure.");
}