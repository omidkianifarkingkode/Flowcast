using Application.Realtime.Messaging;
using Application.Realtime.Services;
using System.Net.WebSockets;
using System.Text;

namespace Infrastructure.Realtime.Services;

public class JsonRealtimeMessageSender(IUserConnectionRegistry connectionRegistry, IRealtimeMessageCodec codec) : IRealtimeMessageSender
{
    public async Task SendToUserAsync(Guid userId, RealtimeMessage message, CancellationToken cancellationToken = default)
    {
        if (connectionRegistry.TryGetWebSocketByUserId(userId, out var socket)
            && socket.State == WebSocketState.Open)
        {
            var json = codec.ToJson(message);
            var buffer = Encoding.UTF8.GetBytes(json);
            var segment = new ArraySegment<byte>(buffer);

            await socket.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
        }
    }

    public async Task SendToUserAsync<T>(Guid userId, RealtimeMessage<T> message, CancellationToken cancellationToken = default)
        where T : IRealtimeCommand
    {
        if (connectionRegistry.TryGetWebSocketByUserId(userId, out var socket)
            && socket.State == WebSocketState.Open)
        {
            var json = codec.ToJson(message);
            var buffer = Encoding.UTF8.GetBytes(json);
            var segment = new ArraySegment<byte>(buffer);

            await socket.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
        }
    }
}