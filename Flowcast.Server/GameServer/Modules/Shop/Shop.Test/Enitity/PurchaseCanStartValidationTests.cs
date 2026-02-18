//using FluentAssertions;
//using Shop.Domain.Entities;
//using Shop.Domain.Enums;

//namespace Shop.Test.Enitity;

//public class PurchaseCanStartValidationTests
//{
//    private const int MaxRetries = 5;
//    private readonly DateTimeOffset _now = new DateTimeOffset(2026, 2, 2, 12, 0, 0, TimeSpan.Zero);

//    #region Helper Methods

  
//    private Purchase CreateLoggedPurchase()
//    {
//        var validId = PurchaseId.FromString("PUR_0193ec9736e479c0892095f939e6027a");
//        return Purchase.LogNew(
//            validId,
//            OrderId.Create("ORD_01935567756b779383624864f9f4682d"),
//            Store.GooglePlay,
//            PurchaseToken.Create("TOK_1"),
//            "prod_1", "rec_1", "pay_1", "user_1", _now).Value;
//    }

//    #endregion

//    #region Scenario 1: Final States (VALID / INVALID)

//    [Fact]
//    public void CanStartValidation_WhenPurchaseIsAlreadyValid_ShouldReturnFailure()
//    {
//        var purchase = CreateLoggedPurchase();
//        purchase.MarkValidating(_now.AddMinutes(-5));
//        purchase.MarkValid(_now.AddMinutes(-2));

//        var result = purchase.CanStartValidation(MaxRetries, _now);

//        result.IsFailure.Should().BeTrue();
//        result.Error.Code.Should().Be("Purchase.AlreadyValid");
//        result.Error.Description.Should().Contain("already validated successfully"); 
//    }

//    [Theory]
//    [InlineData(PurchaseState.VALID, "Purchase.AlreadyValid")]
//    [InlineData(PurchaseState.INVALID, "Purchase.AlreadyInvalid")]
//    public void CanStartValidation_WhenInFinalState_ShouldReturnFailure(PurchaseState state, string expectedErrorCode)
//    {
//        // Arrange
//        var purchase = CreateLoggedPurchase();
//        purchase.SetState(state, _now.AddMinutes(-1));

//        // Act
//        var result = purchase.CanStartValidation(MaxRetries, _now);

//        // Assert
//        result.IsFailure.Should().BeTrue();
//        result.Error.Code.Should().Be(expectedErrorCode);
//    }

//    #endregion

//    #region Scenario 2: Running Attempt

//    [Fact]
//    public void CanStartValidation_WhenAttemptIsRunning_AndNotStuck_ShouldReturnConflict()
//    {
//        // Arrange
//        var purchase = CreateLoggedPurchase();
//        purchase.MarkValidating(_now.AddMinutes(-3));
//        _ = purchase.StartAttempt(_now.AddMinutes(-3)).Value;

//        // Act
//        var result = purchase.CanStartValidation(MaxRetries, _now);

//        // Assert
//        result.IsFailure.Should().BeTrue();
//        result.Error.Code.Should().Be("Purchase.ValidationInProgress");
//        result.Error.Description.Should().Contain("already in progress");
//    }

//    [Fact]
//    public void CanStartValidation_WhenAttemptIsStuck_ShouldForceCompleteAndAllowNewAttempt()
//    {
//        // Arrange
//        var purchase = CreateLoggedPurchase(); 
//        var stuckStartTime = _now.AddMinutes(-10);

//        purchase.SetState(PurchaseState.VALIDATING, stuckStartTime);
//        _ = purchase.StartAttempt(stuckStartTime).Value;


//        // Act
//        purchase.ResetToLogged(stuckStartTime.AddMinutes(1));

//        var result = purchase.CanStartValidation(MaxRetries, _now);

//        // Assert
//        result.IsSuccess.Should().BeTrue();
//    }

//    [Theory]
//    [InlineData(4)] 
//    public void CanStartValidation_WhenAttemptIsNotYetStuck_ShouldReturnConflict(int minutesAgo)
//    {
//        // Arrange
//        var purchase = CreateLoggedPurchase();
//        var startTime = _now.AddMinutes(-minutesAgo);
//        purchase.MarkValidating(startTime);
//        _ = purchase.StartAttempt(startTime).Value;

//        // Act
//        var result = purchase.CanStartValidation(MaxRetries, _now);

//        // Assert
//        result.IsFailure.Should().BeTrue();
//        result.Error.Code.Should().Be("Purchase.ValidationInProgress");
//    }

//    [Fact]
//    public void CanStartValidation_WhenAttemptIsStuckExactlyAfter5Minutes_ShouldForceComplete()
//    {
//        // Arrange
//        var purchase = CreateLoggedPurchase();
//        var stuckStartTime = _now.AddMinutes(-5).AddSeconds(-1); 
//        purchase.MarkValidating(stuckStartTime);
//        _ = purchase.StartAttempt(stuckStartTime).Value;

//        // Act
//        var result = purchase.CanStartValidation(MaxRetries, _now);

//        // Assert
//        result.IsSuccess.Should().BeTrue();

        
//        var attempts = purchase.Attempt.ToList();
//        var stuckAttempt = attempts.First();
//        stuckAttempt.ErrorCode.Should().Be("TIMEOUT");
//    }

//    #endregion

//    #region Scenario 3: Retry Limit

//    [Fact]
//    public void CanStartValidation_WhenMaxRetriesExceeded_ShouldReturnFailure()
//    {
//        // Arrange
//        var purchase = CreateLoggedPurchase();

//        for(int i = 0; i < MaxRetries; i++)
//        {
//            var attemptTime = _now.AddMinutes(-30 + i * 5);

//            purchase.MarkValidating(attemptTime);
//            var attempt = purchase.StartAttempt(attemptTime).Value;

//            attempt.CompleteFailure(
//                "API_ERROR",
//                "Temporary failure",
//                attemptTime.AddMinutes(1)
//            );

//            purchase.ResetToLogged(attemptTime.AddMinutes(2));
//        }

//        // Act
//        var result = purchase.CanStartValidation(MaxRetries, _now);

//        // Assert
//        result.IsFailure.Should().BeTrue();
//        result.Error.Code.Should().Be("Purchase.MaxRetryExceeded");
//    }



//    #endregion

//    #region Scenario 4: Happy Path

//    [Fact]
//    public void CanStartValidation_WhenReadyToValidate_ShouldCreateNewAttempt()
//    {
//        // Arrange
//        var purchase = CreateLoggedPurchase();

//        // Act
//        var result = purchase.CanStartValidation(MaxRetries, _now);

//        // Assert
//        result.IsSuccess.Should().BeTrue();
//        purchase.State.Should().Be(PurchaseState.VALIDATING);
//    }

//    #endregion
//}
