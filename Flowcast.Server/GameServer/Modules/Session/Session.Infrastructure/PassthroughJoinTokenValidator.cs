using Session.Application.Shared;
using Session.Domain;
using SharedKernel.Primitives;

namespace Session.Infrastructure;

public sealed class PassthroughJoinTokenValidator : IJoinTokenValidator
{
    public bool Validate(SessionId sessionId, PlayerId playerId, string token)
        => !string.IsNullOrWhiteSpace(token); // accept anything non-empty for dev
}
