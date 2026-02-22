using SharedKernel;
using Shop.Domain.Entities;
using Shop.Domain.Enums;

namespace Shop.Application.Interfaces;

public interface IPurchaseValidationService
{
    Task<Result<PurchaseState>> ValidateAsync(Purchase purchase, CancellationToken ct);
}
