using Domain.Users.Services;
using SharedKernel;
using System.Collections.Concurrent;

namespace Infrastructure.Persistence.Users.Services;

public class InMemoryUserConnectionRegistry(IDateTimeProvider dateTimeProvider) : IUserConnectionRegistry
{
    private readonly ConcurrentDictionary<string, ConnectionInfo> _connections = new();
    private readonly ConcurrentDictionary<Guid, HashSet<string>> _userConnections = new();
    private readonly object _lock = new();

    public event Action<Guid> UserConnected = delegate { };
    public event Action<Guid> UserDisconnected = delegate { };

    public void Register(string connectionId, Guid userId)
    {
        var connection = new ConnectionInfo(connectionId, userId, dateTimeProvider.UtcNow);
        _connections[connectionId] = connection;

        lock (_lock)
        {
            if (!_userConnections.TryGetValue(userId, out var connections))
            {
                connections = [];
                _userConnections[userId] = connections;
            }

            connections.Add(connectionId);

            if (connections.Count == 1)
                UserConnected?.Invoke(userId);
        }
    }

    public void Unregister(string connectionId)
    {
        if (_connections.TryRemove(connectionId, out var conn))
        {
            conn.MarkDisconnected(dateTimeProvider.UtcNow);
            var userId = conn.UserId;

            lock (_lock)
            {
                if (!_userConnections.TryGetValue(userId, out var conns))
                    return;

                conns.Remove(connectionId);
                if (conns.Count == 0)
                {
                    _userConnections.TryRemove(userId, out _);
                    UserDisconnected?.Invoke(userId);
                }
            }
        }
    }

    public bool TryGetUserId(string connectionId, out Guid userId)
    {
        if (_connections.TryGetValue(connectionId, out var info))
        {
            userId = info.UserId;
            return true;
        }

        userId = default;
        return false;
    }

    public IReadOnlyList<string> GetConnectionsForUser(Guid userId)
    {
        lock (_lock)
        {
            return _userConnections.TryGetValue(userId, out var connections)
                ? connections.ToList()
                : [];
        }
    }

    public bool IsUserConnected(Guid userId)
    {
        lock (_lock)
        {
            return _userConnections.TryGetValue(userId, out var connections) && connections.Count > 0;
        }
    }
}
