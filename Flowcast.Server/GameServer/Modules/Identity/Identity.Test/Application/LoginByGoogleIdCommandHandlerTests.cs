using FluentAssertions;
using Identity.Application.Commands;
using Xunit;
using Identity.Application.Repositories;
using Identity.Application.Services;
using Identity.Domain.Entities;
using Identity.Domain.Shared;
using NSubstitute;
using Shared.Application.Services;
using SharedKernel;

namespace Identity.Test.Application;

public sealed class LoginByGoogleIdCommandHandlerTests
{
    private readonly IAccountRepository _accounts = Substitute.For<IAccountRepository>();
    private readonly IIdentityRepository _identities = Substitute.For<IIdentityRepository>();
    private readonly IIdentityLoginAuditRepository _audits = Substitute.For<IIdentityLoginAuditRepository>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly ITokenService _tokens = Substitute.For<ITokenService>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private readonly LoginByGoogleIdCommandHandler _handler;
    private readonly DateTime _now = DateTime.UtcNow;

    public LoginByGoogleIdCommandHandlerTests()
    {
        _clock.UtcNow.Returns(_now);
        _handler = new LoginByGoogleIdCommandHandler(_accounts, _identities, _audits, _clock, _tokens, _uow);
    }

    [Fact]
    public async Task Handle_WhenGoogleUserIdEmpty_ReturnsInvalidGoogleUserId()
    {
        var result = await _handler.Handle(
            new LoginByGoogleIdCommand("", null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.InvalidGoogleUserId);
        await _identities.DidNotReceive().GetByProviderAndSubject(Arg.Any<IdentityProvider>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("  ")]
    [InlineData("\t")]
    public async Task Handle_WhenGoogleUserIdWhitespace_ReturnsInvalidGoogleUserId(string userId)
    {
        var result = await _handler.Handle(
            new LoginByGoogleIdCommand(userId, null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.InvalidGoogleUserId);
    }

    [Fact]
    public async Task Handle_WhenGoogleUserIdTooLong_ReturnsInvalidGoogleUserId()
    {
        var longId = new string('x', 129);
        var result = await _handler.Handle(
            new LoginByGoogleIdCommand(longId, null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.InvalidGoogleUserId);
    }

    [Fact]
    public async Task Handle_WhenNoExistingIdentity_CreatesAccountAndReturnsAuthResult()
    {
        var googleUserId = "103547318597142817347";
        _identities.GetByProviderAndSubject(IdentityProvider.Google, googleUserId, Arg.Any<CancellationToken>())
            .Returns((IdentityEntity?)null);
        _tokens.IssueAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(("access", "refresh", _now.AddHours(1)));

        var result = await _handler.Handle(
            new LoginByGoogleIdCommand(googleUserId, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccountId.Should().NotBeEmpty();
        result.Value.AccessToken.Should().Be("access");
        result.Value.RefreshToken.Should().Be("refresh");
        await _accounts.Received(1).Add(Arg.Is<Account>(a => a.AccountId != default), Arg.Any<CancellationToken>());
        await _identities.Received(1).Add(Arg.Is<IdentityEntity>(i =>
            i.Provider == IdentityProvider.Google && i.Subject == googleUserId), Arg.Any<CancellationToken>());
        await _audits.Received(1).Add(Arg.Any<IdentityLoginAudit>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExistingIdentity_ReusesAccountAndReturnsAuthResult()
    {
        var googleUserId = "103547318597142817347";
        var accountId = Guid.NewGuid();
        var identityId = Guid.NewGuid();
        var existingIdentity = IdentityEntity.Create(identityId, accountId, IdentityProvider.Google, googleUserId, _now, true);
        var account = Account.Create(accountId, _now);

        _identities.GetByProviderAndSubject(IdentityProvider.Google, googleUserId, Arg.Any<CancellationToken>())
            .Returns(existingIdentity);
        _accounts.GetById(accountId, Arg.Any<CancellationToken>()).Returns(account);
        _tokens.IssueAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(("access", "refresh", _now.AddHours(1)));

        var result = await _handler.Handle(
            new LoginByGoogleIdCommand(googleUserId, new Dictionary<string, string> { ["region"] = "IR" }),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccountId.Should().Be(accountId);
        result.Value.AccessToken.Should().Be("access");
        await _accounts.DidNotReceive().Add(Arg.Any<Account>(), Arg.Any<CancellationToken>());
        await _identities.DidNotReceive().Add(Arg.Any<IdentityEntity>(), Arg.Any<CancellationToken>());
        await _audits.Received(1).Add(Arg.Any<IdentityLoginAudit>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TrimsGoogleUserId()
    {
        var googleUserId = "  103547318597142817347  ";
        _identities.GetByProviderAndSubject(IdentityProvider.Google, "103547318597142817347", Arg.Any<CancellationToken>())
            .Returns((IdentityEntity?)null);
        _tokens.IssueAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(("access", "refresh", _now.AddHours(1)));

        var result = await _handler.Handle(
            new LoginByGoogleIdCommand(googleUserId, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _identities.Received(1).GetByProviderAndSubject(IdentityProvider.Google, "103547318597142817347", Arg.Any<CancellationToken>());
    }
}
