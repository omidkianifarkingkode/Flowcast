using Microsoft.Extensions.Logging;
using Realtime.Transport.Http;
using Realtime.Transport.Messaging.Factories;
using Realtime.Transport.Messaging.Sender;
using Realtime.Transport.UserConnection;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Realtime.Transport.Messaging.Receiver;

public class MessageReceiver(
    IRealtimePayloadFactory factory,
    IUserConnectionRegistry registry,
    IMessageFactory messageFactory,
    IRealtimeMessageSender messageSender,
    ILogger<MessageReceiver> logger)
    : IRealtimeMessageReceiver, IRealtimeGateway
{
    private readonly Channel<(RealtimeContext, IRealtimeMessage)> _channel = Channel.CreateBounded<(RealtimeContext, IRealtimeMessage)>(10);

    public event Action<RealtimeContext, IRealtimeMessage> OnFrame = delegate { };

    public Task ReceiveTextMessage(string userId, string data, CancellationToken cancellationToken = default)
    {
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

    public async IAsyncEnumerable<(RealtimeContext ctx, IRealtimeMessage frame)> ReadAllAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (await _channel.Reader.WaitToReadAsync(cancellationToken))
            while (_channel.Reader.TryRead(out var item))
                yield return item;
    }

    private async Task HandleMessage(string userId, IRealtimeMessage message, bool isText, CancellationToken cancellationToken)
    {
        logger.LogInformation("Incoming message from {UserId}: Type={Type}, Id={Id}, Time={Timestamp}",
            userId, message.Header.Type, message.Header.Id, message.Header.Timestamp);

        registry.TryGetUserConnectionInfo(userId, out var info);
        var context = new RealtimeContext { UserId = userId, ConnectionId = info.ConnectionId, Header = message.Header };

        if (message.Header.Type == RealtimeMessageType.Ping)
        {
            var pong = messageFactory.CreatePongFor(message.Header);

            await messageSender.SendToUserAsync(userId, pong, cancellationToken);
        }
        else
        {
            OnFrame?.Invoke(context, message);

            await _channel.Writer.WriteAsync((context, message), cancellationToken);
            //return commandDispatcher.DispatchAsync(userId, message, cancellationToken);
        }

    }
}

