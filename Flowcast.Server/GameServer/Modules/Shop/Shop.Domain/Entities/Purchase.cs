using SharedKernel;
using Shop.Domain.Enums;
using Shop.Domain.Shared;

namespace Shop.Domain.Entities;

public sealed class Purchase : Entity<PurchaseId>, ICreatable, IModifiable
{
    public static readonly TimeSpan ValidationTimeout = TimeSpan.FromMinutes(5);
    public const int MaxValidationAttempts = 5;

    public OrderId OrderId { get; private set; }
    public Store Store { get; private set; }
    public PurchaseToken PurchaseToken { get; private set; }
    public PurchaseSignature? Signature { get; private set; }
    public string ProductId { get; private set; }
    public string Receipt { get; private set; }
    public string Payload { get; private set; }
    public DateTimeOffset PurchaseAtUtc { get; private set; }
    public string UserId { get; private set; }
    public bool IsSandbox { get; private set; }
    public PurchaseState State { get; private set; }
    public IReadOnlyDictionary<string, string>? Metadata { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public string? CreatorUser { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? ModifierUser { get; set; }

    public IReadOnlyCollection<PurchaseValidationAttempt> ValidationAttempts => _validationAttempts.AsReadOnly();
    private readonly List<PurchaseValidationAttempt> _validationAttempts = [];

    private Purchase() { } // EF

    private Purchase(PurchaseId id, OrderId orderId, Store store, PurchaseToken token, PurchaseSignature? signature,
                     string productId, string receipt, string payload, string userId, DateTimeOffset purchaseAtUtc,
                     bool isSandbox, Dictionary<string, string>? metadata) : base(id)
    {
        OrderId = orderId;
        Store = store;
        PurchaseToken = token;
        Signature = signature;
        ProductId = productId;
        Receipt = receipt;
        Payload = payload;
        UserId = userId;
        PurchaseAtUtc = purchaseAtUtc;
        IsSandbox = isSandbox;
        Metadata = metadata;
        State = PurchaseState.LOGGED;
    }

    // ---------- Factory ----------

    public static Result<Purchase> LogNew(PurchaseId id, OrderId orderId, Store store, PurchaseToken token,
                                          string productId, string receipt, string payload, string userId,
                                          DateTimeOffset purchaseAtUtc, bool isSandbox = false,
                                          PurchaseSignature? signature = null, Dictionary<string, string>? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(id.Value))
            return Result.Failure<Purchase>(PurchaseErrors.InvalidPurchaseId);

        if (string.IsNullOrWhiteSpace(productId))
            return Result.Failure<Purchase>(PurchaseErrors.InvalidProductId);

        if (string.IsNullOrWhiteSpace(receipt))
            return Result.Failure<Purchase>(PurchaseErrors.InvalidReceipt);

        if (string.IsNullOrWhiteSpace(payload))
            return Result.Failure<Purchase>(PurchaseErrors.InvalidPayload);

        if (string.IsNullOrWhiteSpace(userId))
            return Result.Failure<Purchase>(PurchaseErrors.InvalidUserId);

        return Result.Success(
            new Purchase(
                id,
                orderId,
                store,
                token,
                signature,
                productId,
                receipt,
                payload,
                userId,
                purchaseAtUtc,
                isSandbox, 
                metadata ?? []));
    }

    // ---------- Public API ----------

    public Result<PurchaseValidationAttempt> BeginValidation(DateTimeOffset now, bool force = false)
    {
        // 1. Normal flow: only LOGGED or FAILED allowed
        if (State != PurchaseState.LOGGED && State != PurchaseState.FAILED)
        {
            return Result.Failure<PurchaseValidationAttempt>(PurchaseErrors.InvalidStateForValidation);
        }

        // 1. Except flow: forced or not reached to max attempts
        if (State == PurchaseState.FAILED && !force && _validationAttempts.Count >= MaxValidationAttempts)
        {
            return Result.Failure<PurchaseValidationAttempt>(PurchaseErrors.ForceRequiredAfterMaxAttempts);
        }

        // 3. Clean up stuck attempt (timeout logic stays the same)
        ResolveStuckAttempt(now);

        // 4. Check retry limit
        if (_validationAttempts.Count >= MaxValidationAttempts)
        {
            State = PurchaseState.FAILED;
            return Result.Failure<PurchaseValidationAttempt>(PurchaseErrors.MaxValidationAttemptsExceeded);
        }

        // 5. Create new attempt
        var attemptResult = PurchaseValidationAttempt.Start(Id, _validationAttempts.Count + 1, now);
        if (attemptResult.IsFailure)
            return Result.Failure<PurchaseValidationAttempt>(attemptResult.Error);

        _validationAttempts.Add(attemptResult.Value);
        State = PurchaseState.VALIDATING;

        return Result.Success(attemptResult.Value);
    }

    public Result SubmitValidation(
        ValidationResult result,
        DateTimeOffset now)
    {
        var attempt = GetPendingAttempt();
        if (attempt is null)
        {
            return Result.Failure(PurchaseErrors.NoPendingValidationAttempt);
        }

        if (result.IsSuccess)
        {
            attempt.CompleteSuccess(now);
            State = result.State;
        }
        else
        {
            attempt.CompleteFailure(
                result.ErrorCode!,
                result.ErrorMessage!,
                now);

            State = PurchaseState.FAILED;
        }

        return Result.Success();
    }

    public void RecoverStuckAttemptIfAny(DateTimeOffset now)
    {
        var attempt = GetPendingAttempt();
        if (attempt is null) return;

        if (attempt.StartedAtUtc + ValidationTimeout > now)
            return;

        attempt.CompleteFailure("TIMEOUT", $"Timed out after {ValidationTimeout.TotalMinutes} min", now);
        State = PurchaseState.FAILED;
    }

    // ---------- Helpers ----------

    /// <summary>
    /// Clean up / timeout any attempt that has been running too long.
    /// </summary>
    /// <param name="now"></param>
    private void ResolveStuckAttempt(DateTimeOffset now)
    {
        var runningAttempt = GetPendingAttempt();
        if (runningAttempt is null)
            return;

        // still fresh → do nothing
        if (runningAttempt.StartedAtUtc + ValidationTimeout > now)
            return;

        runningAttempt.CompleteFailure(
            "TIMEOUT",
            $"Validation timed out after {ValidationTimeout.TotalMinutes} minutes",
            now);

        State = PurchaseState.FAILED;
    }

    /// <summary>
    /// Find if there is currently an unfinished validation attempt.
    /// </summary>
    /// <returns></returns>
    private PurchaseValidationAttempt? GetPendingAttempt()
        => _validationAttempts.FirstOrDefault(a => !a.FinishedAtUtc.HasValue);
}
