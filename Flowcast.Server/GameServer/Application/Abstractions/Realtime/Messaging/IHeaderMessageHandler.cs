namespace Application.Abstractions.Realtime.Messaging;

/// <summary>
/// Handles header-only realtime messages (no payload), e.g., Ping/Pong.
/// Return true when handled, false to let others try (or fall through).
/// </summary>
public interface IHeaderMessageHandler
{
    Task<bool> TryHandleAsync(Guid userId, RealtimeMessage message, CancellationToken ct);
}