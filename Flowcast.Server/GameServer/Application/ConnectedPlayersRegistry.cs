using Domain.Users;
using System.Collections.Concurrent;

namespace Application.Services;

public class ConnectedPlayersRegistry
{
    private readonly ConcurrentDictionary<Guid, ConnectedPlayer> _connected = new();

    public void Add(User user, string connectionId)
    {
        var connectedPlayer = new ConnectedPlayer(user.Id, user.DisplayName, connectionId);
        _connected[user.Id] = connectedPlayer;
    }

    public bool TryGet(Guid playerId, out ConnectedPlayer player)
        => _connected.TryGetValue(playerId, out player);

    public void Remove(Guid playerId) => _connected.TryRemove(playerId, out _);
}

public record ConnectedPlayer(Guid PlayerId, string DisplayName, string ConnectionId);
