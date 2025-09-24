using Session.Domain;
using SharedKernel.Primitives;

namespace Session.Application.Shared;

public interface IJoinTokenValidator
{
    /// <summary>Return true if token is valid for this session and player, not expired, not revoked.</summary>
    bool Validate(SessionId sessionId, PlayerId playerId, string token);
}
