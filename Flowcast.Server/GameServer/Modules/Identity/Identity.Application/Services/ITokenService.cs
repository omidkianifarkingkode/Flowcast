namespace Identity.Application.Services;

public interface ITokenService
{
    Task<(string access, string refresh, DateTime expiresAtUtc)> IssueAsync(Guid accountId, CancellationToken ct);
    Task<(bool ok, Guid accountId, string access, string refresh, DateTime expiresAtUtc)> RefreshAsync(string refreshToken, CancellationToken ct);
    Task<bool> RevokeAsync(string refreshToken, CancellationToken ct);
}
