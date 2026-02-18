using Microsoft.EntityFrameworkCore;
using ZP.Core.Entities;
using ZP.Core.Repositories;

namespace ZP.Core.Persisntence;

public sealed class PaymentRequestRepository : IPaymentRequestRepository
{
    private readonly ApplicationDbContext _db;

    public PaymentRequestRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<PaymentRequest?> GetByAuthorityAsync(string authority, CancellationToken ct = default)
    {
        var authorityVo = Authority.Create(authority);
        return _db.PaymentRequests
            .FirstOrDefaultAsync(p => p.Authority == authorityVo, ct);
    }

    public Task<PaymentRequest?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default)
    {
        return _db.PaymentRequests
            .FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey, ct);
    }

    public void Add(PaymentRequest paymentRequest)
    {
        _db.PaymentRequests.Add(paymentRequest);
    }
}
