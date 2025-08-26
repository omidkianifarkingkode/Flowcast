using Application.MatchMakings.Shared;
using Domain.Sessions;
using Realtime.Transport.UserConnection;

namespace Infrastructure.Realtime.Services;

public sealed class RegistryLivenessProbe(IUserConnectionRegistry connections) : ILivenessProbe
{
    public bool IsHealthy(PlayerId playerId)
        => connections.IsUserConnected(playerId.Value.ToString());
}
