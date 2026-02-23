using Shared.Application.Messaging;
using Shared.Application.Services;
using SharedKernel;
using Shop.Application.IRepositories;
using Shop.Domain.Entities;

namespace Shop.Application.Features.Commands;


public sealed class CreatePurchaseCommandHandler(
 IPurchaseRepository purchaseRepo,
 IUnitOfWork uow
) : ICommandHandler<CreatePurchaseCommand, CreatePurchaseResult>
{
    public async Task<Result<CreatePurchaseResult>> Handle(
     CreatePurchaseCommand command,
     CancellationToken ct)
    {
        var existing = await purchaseRepo
            .GetByStoreAndOrderId(command.Store, command.OrderId, ct);
        if(existing is not null)
            return Result.Success(
                    new CreatePurchaseResult(existing.Id, false)
                    );

        var purchaseResult = Purchase.LogNew(
            id: PurchaseId.New(),
            orderId: command.OrderId,
            store: command.Store,
            token: command.PurchaseToken,
            productId: command.ProductId,
            receipt: command.Receipt,
            payload: command.Payload,
            userId: command.UserId,
            purchaseAtUtc: command.PurchaseAtUtc,
            isSandbox: command.IsSandbox,
            signature: command.Signature,
            metadata: command.Metadata?.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? "")
        );
        if (purchaseResult.IsFailure)
            return Result.Failure<CreatePurchaseResult>(purchaseResult.Error);

        var purchase = purchaseResult.Value;
        var addResult = await purchaseRepo.AddNewLog(purchase, ct);
        if (addResult.IsFailure)
            return Result.Failure<CreatePurchaseResult>(addResult.Error);

        await uow.SaveChangesAsync(ct);

        return Result.Success(new CreatePurchaseResult(purchase.Id, true));
    }

}
