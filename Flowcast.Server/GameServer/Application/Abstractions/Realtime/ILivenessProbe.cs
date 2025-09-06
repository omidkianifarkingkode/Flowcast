using Domain.Sessions;

namespace Application.Abstractions.Realtime;

/// Liveness view (can be backed by IUserConnectionRegistry + health window policy)
public interface ILivenessProbe
{
    bool IsHealthy(PlayerId playerId); // last Pong within window etc.
}
