using Shop.Domain.Enums;

namespace Shop.Domain.Entities;

public sealed record ValidationResult(
    bool IsSuccess,
    PurchaseState State,
    string? ErrorCode = null,
    string? ErrorMessage = null)
{
    public static ValidationResult Success(PurchaseState state)
        => new(true, state);

    public static ValidationResult Failure(string code, string message)
        => new(false, PurchaseState.FAILED, code, message);
}
