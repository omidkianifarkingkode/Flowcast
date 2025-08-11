using Application.Abstractions.Realtime;
using System.Net.WebSockets;

namespace Infrastructure.Realtime;

public class BinaryRealtimeMessageSender(IUserConnectionRegistry connectionRegistry) : IRealtimeMessageSender
{
    public async Task SendToUserAsync(Guid userId, RealtimeMessage message, CancellationToken cancellationToken = default)
    {
        if (connectionRegistry.TryGetWebSocketByUserId(userId, out var socket)
             && socket.State == WebSocketState.Open)
        {
            var buffer = message.ToBytes();
            var segment = new ArraySegment<byte>(buffer);

            await socket.SendAsync(segment, WebSocketMessageType.Binary, true, cancellationToken);
        }
    }
}