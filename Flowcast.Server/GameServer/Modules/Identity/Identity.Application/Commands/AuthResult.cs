namespace Identity.Application.Commands;

public sealed record AuthResult(Guid AccountId, string AccessToken, string RefreshToken, DateTime ExpiresAtUtc);
