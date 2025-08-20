using Application.Abstractions.Security;

namespace Infrastructure.Security;

public sealed class PassthroughJoinTokenValidator : IJoinTokenValidator
{
    public bool Validate(Guid sessionId, Guid playerId, string token)
        => !string.IsNullOrWhiteSpace(token); // accept anything non-empty for dev
}
