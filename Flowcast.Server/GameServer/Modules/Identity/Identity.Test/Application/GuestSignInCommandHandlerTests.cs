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

public sealed class GuestSignInCommandHandlerTests
{
    private readonly IAccountRepository _accounts = Substitute.For<IAccountRepository>();
    private readonly IIdentityRepository _identities = Substitute.For<IIdentityRepository>();
    private readonly IIdentityLoginAuditRepository _audits = Substitute.For<IIdentityLoginAuditRepository>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly ITokenService _tokens = Substitute.For<ITokenService>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private readonly GuestSignInCommandHandler _handler;
    private readonly DateTime _now = DateTime.UtcNow;

    public GuestSignInCommandHandlerTests()
    {
        _clock.UtcNow.Returns(_now);
        _handler = new GuestSignInCommandHandler(_accounts, _identities, _audits, _clock, _tokens, _uow);
    }

    [Fact]
    public async Task Handle_WhenNoExistingGuest_CreatesAccountAndReturnsAuthResult()
    {
        var guestToken = "guest-token-123";
        var meta = new Dictionary<string, string> { ["guestToken"] = guestToken };
        _identities.GetByProviderAndSubject(IdentityProvider.None, guestToken, Arg.Any<CancellationToken>())
            .Returns((IdentityEntity?)null);

        _tokens.IssueAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(("access", "refresh", _now.AddHours(1)));

        var result = await _handler.Handle(new GuestSignInCommand(meta), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccountId.Should().NotBeEmpty();
        result.Value.AccessToken.Should().Be("access");
        result.Value.RefreshToken.Should().Be("refresh");
        await _accounts.Received(1).Add(Arg.Is<Account>(a => a.AccountId != default), Arg.Any<CancellationToken>());
        await _identities.Received(1).Add(Arg.Any<IdentityEntity>(), Arg.Any<CancellationToken>());
        await _audits.Received(1).Add(Arg.Any<IdentityLoginAudit>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExistingGuestWithLoginAllowed_ReusesAccountAndReturnsAuthResult()
    {
        var guestToken = "existing-guest";
        var accountId = Guid.NewGuid();
        var identityId = Guid.NewGuid();
        var existingIdentity = IdentityEntity.Create(identityId, accountId, IdentityProvider.None, guestToken, _now, true);
        var account = Account.Create(accountId, _now);

        _identities.GetByProviderAndSubject(IdentityProvider.None, guestToken, Arg.Any<CancellationToken>())
            .Returns(existingIdentity);
        _accounts.GetById(accountId, Arg.Any<CancellationToken>()).Returns(account);
        _tokens.IssueAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(("access", "refresh", _now.AddHours(1)));

        var result = await _handler.Handle(
            new GuestSignInCommand(new Dictionary<string, string> { ["guestToken"] = guestToken }),
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
    public async Task Handle_WhenExistingGuestLoginDisabled_ReturnsGuestLoginDisabled()
    {
        var guestToken = "disabled-guest";
        var accountId = Guid.NewGuid();
        var identityId = Guid.NewGuid();
        var existingIdentity = IdentityEntity.Create(identityId, accountId, IdentityProvider.None, guestToken, _now, true);
        existingIdentity.DisableLogin();

        _identities.GetByProviderAndSubject(IdentityProvider.None, guestToken, Arg.Any<CancellationToken>())
            .Returns(existingIdentity);

        var result = await _handler.Handle(
            new GuestSignInCommand(new Dictionary<string, string> { ["guestToken"] = guestToken }),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.GuestLoginDisabled);
        await _accounts.DidNotReceive().Add(Arg.Any<Account>(), Arg.Any<CancellationToken>());
        await _tokens.DidNotReceive().IssueAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
