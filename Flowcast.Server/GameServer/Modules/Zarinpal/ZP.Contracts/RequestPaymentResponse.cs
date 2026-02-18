namespace ZP.Contracts;

public sealed record RequestPaymentResponse(
    string PaymentUrl,
    string Authority,
    string OrderId);
