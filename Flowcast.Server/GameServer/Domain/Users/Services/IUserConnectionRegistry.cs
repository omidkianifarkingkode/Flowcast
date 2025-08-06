namespace Domain.Users.Services;

public interface IUserConnectionRegistry
{
    void Register(string connectionId, Guid userId);
    void Unregister(string connectionId);
    bool TryGetUserId(string connectionId, out Guid userId);
    IReadOnlyList<string> GetConnectionsForUser(Guid userId);
    bool IsUserConnected(Guid userId);
    event Action<Guid> UserConnected;
    event Action<Guid> UserDisconnected;
}