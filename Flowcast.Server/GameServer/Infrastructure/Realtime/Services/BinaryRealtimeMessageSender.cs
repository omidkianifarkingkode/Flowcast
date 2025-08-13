using Application.Abstractions.Realtime;
using Application.Abstractions.Realtime.Messaging;
using Application.Abstractions.Realtime.Services;
using System.Net.WebSockets;

namespace Infrastructure.Realtime.Services;

public class BinaryRealtimeMessageSender(IUserConnectionRegistry connectionRegistry, IRealtimeMessageCodec realtimeMessageCodec) : IRealtimeMessageSender
{
    public async Task SendToUserAsync(Guid userId, RealtimeMessage message, CancellationToken cancellationToken = default)
    {
        if (connectionRegistry.TryGetWebSocketByUserId(userId, out var socket)
             && socket.State == WebSocketState.Open)
        {
            var buffer = realtimeMessageCodec.ToBytes(message);
            var segment = new ArraySegment<byte>(buffer);

            await socket.SendAsync(segment, WebSocketMessageType.Binary, true, cancellationToken);
        }
    }
}