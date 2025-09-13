using Identity.API.Shared;

namespace Identity.API.Services;

public interface ITokenService
{
    Task<(string access, string refresh, DateTime expiresAtUtc)> IssueAsync(Guid accountId, CancellationToken ct);
    Task<(bool ok, Guid accountId, string access, string refresh, DateTime expiresAtUtc)> RefreshAsync(string refreshToken, CancellationToken ct);
    Task<bool> RevokeAsync(string refreshToken, CancellationToken ct);
}

public interface IProviderTokenVerifier
{
    // Return provider "sub" if valid; otherwise null
    Task<string?> VerifyAndGetSubjectAsync(IdentityProvider provider, string idToken, CancellationToken ct);
}
