using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Realtime.Transport.Http;
using Realtime.Transport.Messaging.Factories;
using Realtime.Transport.Messaging.Sender;
using Realtime.Transport.Options;
using Realtime.Transport.UserConnection;
using System.Net.WebSockets;

namespace Realtime.Transport.Messaging.Receiver;

public class MessageReceiver(
    IRealtimePayloadFactory factory,
    IUserConnectionRegistry registry,
    IMessageFactory messageFactory,
    IRealtimeMessageSender messageSender,
    IOptions<RealtimeOptions> realtimeOptionsAccessor,
    ILogger<MessageReceiver> logger)
    : IMessageReceiver
{
    public event Action<RealtimeContext, IRealtimeMessage> OnMessageReceived = delegate { };

    public Task ReceiveTextMessage(string userId, string data, CancellationToken cancellationToken = default)
    {
        if (realtimeOptionsAccessor.Value.Liveness.CountAnyInboundAsActivity)
            registry.MarkClientActivity(userId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

        try
        {
            var message = factory.CreateFromJson(data);
            return HandleMessage(userId, message, isText: true, cancellationToken);
        }
        catch
        {
            if (registry.TryGetWebSocketByUserId(userId, out var socket))
                _ = socket.CloseAsync(WebSocketCloseStatus.ProtocolError, "payload-mismatch", cancellationToken);

            return Task.CompletedTask;
        }
    }

    public Task ReceiveBinaryMessage(string userId, byte[] data, CancellationToken cancellationToken = default)
    {
        if (realtimeOptionsAccessor.Value.Liveness.CountAnyInboundAsActivity)
            registry.MarkClientActivity(userId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

        try
        {
            var message = factory.CreateFromBinary(data);
            return HandleMessage(userId, message, isText: false, cancellationToken);
        }
        catch
        {
            if (registry.TryGetWebSocketByUserId(userId, out var socket))
                _ = socket.CloseAsync(WebSocketCloseStatus.ProtocolError, "payload-mismatch", cancellationToken);

            return Task.CompletedTask;
        }
    }

    private async Task HandleMessage(string userId, IRealtimeMessage message, bool isText, CancellationToken cancellationToken)
    {
        logger.LogInformation("Incoming message from {UserId}: Type={Type}, Id={Id}, Time={Timestamp}",
            userId, message.Header.Type, message.Header.Id, message.Header.Timestamp);

        if (message.Header.Type == RealtimeMessageType.Ping)
        {
            var pong = messageFactory.CreatePongFor(message.Header);

            await messageSender.SendToUserAsync(userId, pong, cancellationToken);
        }
        else
        {
            registry.TryGetUserConnectionInfo(userId, out var info);
            var context = new RealtimeContext { UserId = userId, ConnectionId = info.ConnectionId, Header = message.Header };

            OnMessageReceived?.Invoke(context, message);
        }
    }
}

