using Realtime.Transport.Messaging;

namespace Realtime.Transport.Http;

public sealed class RealtimeContext
{
    public required string UserId { get; init; }
    public required string ConnectionId { get; init; }
    public required MessageHeader Header { get; init; }
}
