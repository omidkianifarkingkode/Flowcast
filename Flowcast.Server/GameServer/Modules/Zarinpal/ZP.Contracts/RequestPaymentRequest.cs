namespace ZP.Contracts;

public sealed record RequestPaymentRequest(
    string UserId,
    string ProductId,
    int Amount,
    string Description,
    string? Mobile = null,
    string? Email = null,
    string? IdempotencyKey = null);
