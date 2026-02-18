using NSubstitute;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Shop.Application.Interfaces;
using Shop.Application.Repositories;
using Shop.Domain.Entities;
using Shop.Domain.Enums;
using SharedKernel;
using Shared.Application.Services;
using NSubstitute.ExceptionExtensions;
using Shop.Application.Commands;

namespace Shop.Test.Application;

public class ValidatePurchaseCommandHandlerTests
{
    private readonly IPurchaseRepository _purchaseRepo = Substitute.For<IPurchaseRepository>();
    private readonly IPurchaseValidationService _validationService = Substitute.For<IPurchaseValidationService>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly ILogger<ValidatePurchaseCommandHandler> _logger = Substitute.For<ILogger<ValidatePurchaseCommandHandler>>();
    private readonly ValidatePurchaseCommandHandler _handler;
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    public ValidatePurchaseCommandHandlerTests()
    {
        _handler = new ValidatePurchaseCommandHandler(_purchaseRepo, _validationService, _uow, _dateTimeProvider, _logger);
    }

    private Purchase CreateLoggedPurchase()
    {
        var validId = PurchaseId.FromString("PUR_0193ec9736e479c0892095f939e6027a");

        return Purchase.LogNew(
            validId,
            OrderId.Create("ORD_0193ec9736e479c0892095f939e6027b"),
            Store.GooglePlay,
            PurchaseToken.Create("TOK_1"),
            "prod_1", "rec_1", "pay_1", "user_1", _now).Value;
    }
    #region Scenario 1: Business Failure (INVALID)

    [Fact]
    public async Task Handle_WhenValidationErrorOccurs_ShouldMarkAsInvalid()
    {
        // Arrange
        var purchase = CreateLoggedPurchase();
        _purchaseRepo.GetById(Arg.Is<PurchaseId>(id => id == purchase.Id), true, true, Arg.Any<CancellationToken>())
            .Returns(purchase);

        var validationError = Error.Validation("Store.InvalidReceipt", "Receipt is fake");
        _validationService.ValidateAsync(purchase, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<PurchaseState>(validationError));

        // Act
        await _handler.Handle(new ValidatePurchaseCommand(purchase.Id), CancellationToken.None);

        // Assert
        purchase.State.Should().Be(PurchaseState.INVALID);
    }

    #endregion

    #region Scenario 2: Technical Failure (FAILED)

    [Fact]
    public async Task Handle_WhenTechnicalErrorOccurs_ShouldMarkAsFailedForRetry()
    {
        // Arrange
        var purchase = CreateLoggedPurchase();
        _purchaseRepo.GetById(purchase.Id, true, true, Arg.Any<CancellationToken>()).Returns(purchase);

        var technicalError = Error.Failure("Network.Timeout", "Google API is down");
        _validationService.ValidateAsync(purchase, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<PurchaseState>(technicalError));

        // Act
        await _handler.Handle(new ValidatePurchaseCommand(purchase.Id), CancellationToken.None);

        // Assert
        purchase.State.Should().Be(PurchaseState.FAILED);
        purchase.Metadata.Should().ContainKey("fail_reason");
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Scenario 3: Unexpected Exception

    [Fact]
    public async Task Handle_WhenExceptionThrown_ShouldMarkAsFailedAndLog()
    {
        // Arrange
        var purchase = CreateLoggedPurchase();
        _purchaseRepo.GetById(purchase.Id, true, true, Arg.Any<CancellationToken>()).Returns(purchase);

        _validationService
            .ValidateAsync(purchase, Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Critical System Error"));

        // Act
        await _handler.Handle(new ValidatePurchaseCommand(purchase.Id), CancellationToken.None);

        // Assert
        purchase.State.Should().Be(PurchaseState.FAILED);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion
}