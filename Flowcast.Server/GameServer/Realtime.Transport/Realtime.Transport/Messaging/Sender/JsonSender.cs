using Realtime.Transport.Messaging.Codec;
using Realtime.Transport.UserConnection;
using System.Net.WebSockets;
using System.Text;

namespace Realtime.Transport.Messaging.Sender;

public class JsonSender(IUserConnectionRegistry connectionRegistry, ICodec codec) : IRealtimeMessageSender
{
    public async Task SendToUserAsync(string userId, RealtimeMessage message, CancellationToken cancellationToken = default)
    {
        if (connectionRegistry.TryGetWebSocketByUserId(userId, out var socket)
            && socket.State == WebSocketState.Open)
        {
            var bytes = codec.ToJson(message);
            var segment = new ArraySegment<byte>(bytes);

            await socket.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
        }
    }

    public async Task SendToUserAsync<TPayload>(string userId, RealtimeMessage<TPayload> message, CancellationToken cancellationToken = default)
        where TPayload : IPayload
    {
        if (connectionRegistry.TryGetWebSocketByUserId(userId, out var socket)
            && socket.State == WebSocketState.Open)
        {
            var bytes = codec.ToJson(message);
            var segment = new ArraySegment<byte>(bytes);

            await socket.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
        }
    }
}