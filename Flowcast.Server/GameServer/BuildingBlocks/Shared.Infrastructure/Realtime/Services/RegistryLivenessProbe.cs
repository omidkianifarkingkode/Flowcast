using Realtime.Transport.UserConnection;
using Shared.Application.Realtime;
using SharedKernel.Primitives;

namespace Shared.Infrastructure.Realtime.Services;

public sealed class RegistryLivenessProbe(IUserConnectionRegistry connections) : ILivenessProbe
{
    public bool IsHealthy(PlayerId playerId)
        => connections.IsUserConnected(playerId.Value.ToString());
}
