using Realtime.Transport.Messaging.Codec;
using Realtime.Transport.Messaging.Factories;
using Realtime.Transport.UserConnection;
using System.Net.WebSockets;

namespace Realtime.Transport.Messaging.Sender;

public class BinarySender(
    IUserConnectionRegistry connectionRegistry,
    ICodec codec,
    IMessageFactory messageFactory)
    : IRealtimeMessageSender
{
    public async Task SendToUserAsync(string userId, RealtimeMessage message, CancellationToken cancellationToken = default)
    {
        if (connectionRegistry.TryGetWebSocketByUserId(userId, out var socket)
             && socket.State == WebSocketState.Open)
        {
            var buffer = codec.ToBytes(message);
            await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, cancellationToken);
        }
    }

    public async Task SendToUserAsync<TPayload>(string userId, RealtimeMessage<TPayload> message, CancellationToken cancellationToken = default)
        where TPayload : IPayload
    {
        if (connectionRegistry.TryGetWebSocketByUserId(userId, out var socket)
             && socket.State == WebSocketState.Open)
        {
            var buffer = codec.ToBytes(message);
            await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, cancellationToken);
        }
    }

    public Task SendToUserAsync<TPayload>(string userId, ushort type, TPayload payload, CancellationToken cancellationToken = default)
        where TPayload : IPayload
    {
        var message = messageFactory.Create(type, payload, userId: userId);

        return SendToUserAsync(userId, message, cancellationToken);
    }
}
