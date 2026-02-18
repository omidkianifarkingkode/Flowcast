using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Application.Messaging;
using Shared.Application.Services;
using SharedKernel;
using Shop.Application.Repositories;
using Shop.Domain.Entities;
using Shop.Domain.Enums;

namespace Shop.Application.Commands;

public sealed record CreatePurchaseResult(PurchaseId PurchaseId, bool IsNew);

public sealed record CreatePurchaseCommand(
    OrderId OrderId,
    Store Store,
    PurchaseToken PurchaseToken,
    PurchaseSignature Signature,
    string ProductId,
    string Receipt,
    string Payload,
    string UserId,
    DateTimeOffset PurchaseAtUtc,
    bool IsSandbox,
    Dictionary<string, string>? Metadata = null

) : ICommand<CreatePurchaseResult>;

public sealed class CreatePurchaseCommandHandler(
    IPurchaseRepository purchases,
    [FromKeyedServices("shop")] IUnitOfWork uow,
    ILogger<CreatePurchaseCommandHandler> logger) : 
    ICommandHandler<CreatePurchaseCommand, CreatePurchaseResult>
{
    public async Task<Result<CreatePurchaseResult>> Handle(CreatePurchaseCommand command, CancellationToken ct)
    {
        // 1. Idempotency check – we usually don't need validation attempts here
        var existing = await purchases.GetByStoreAndOrderId(
            command.Store,
            command.OrderId,
            asTracking: false,
            includeAttempts: false,
            ct);

        if (existing is not null)
        {
            logger.LogInformation(
                "Duplicate purchase attempt for OrderId {OrderId} from Store {Store} - returning existing Id {PurchaseId}",
                command.OrderId.Value, command.Store, existing.Id.Value);

            return Result.Success(new CreatePurchaseResult(existing.Id, IsNew: false));
        }

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
            metadata: command.Metadata
        );

        if (purchaseResult.IsFailure)
            return Result.Failure<CreatePurchaseResult>(purchaseResult.Error);

        var purchase = purchaseResult.Value;

        // 3. Stage the insert
        purchases.AddNewLog(purchase, ct);

        // 4. Persist — rely on UnitOfWork to handle transaction + exceptions
        var saveResult = await uow.SaveChangesAsync(ct);

        if (saveResult.IsFailure)
        {
            // Here you could log more context if needed
            logger.LogWarning(
                "Failed to persist new purchase for OrderId {OrderId}. Error: {ErrorMessage}",
                command.OrderId.Value, saveResult.Error?.Description);

            return Result.Failure<CreatePurchaseResult>(saveResult.Error!);
        }

        logger.LogInformation(
            "Successfully created new purchase {PurchaseId} for OrderId {OrderId}",
            purchase.Id.Value, command.OrderId.Value);

        return Result.Success(new CreatePurchaseResult(purchase.Id, IsNew: true));
    }
}

public class CreatePurchaseCommandValidator : AbstractValidator<CreatePurchaseCommand>
{
    public CreatePurchaseCommandValidator()
    {
        RuleFor(x => x.Receipt)
            .NotEmpty().WithErrorCode("REQUIRED").WithMessage("Receipt is required.")
            .MaximumLength(10_000).WithErrorCode("TOO_LARGE").WithMessage("Receipt is too long.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithErrorCode("REQUIRED").WithMessage("ProductId is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithErrorCode("REQUIRED").WithMessage("UserId is required.");

        RuleFor(x => x.PurchaseToken)
            .NotEmpty().WithErrorCode("REQUIRED").WithMessage("PurchaseToken is required.");
    }
}
