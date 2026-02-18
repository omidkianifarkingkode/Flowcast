using ZP.Core.Entities;

namespace ZP.Core.Repositories;

public interface IPaymentRequestRepository
{
    Task<PaymentRequest?> GetByAuthorityAsync(string authority, CancellationToken ct = default);
    Task<PaymentRequest?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default);
    void Add(PaymentRequest paymentRequest);
}
