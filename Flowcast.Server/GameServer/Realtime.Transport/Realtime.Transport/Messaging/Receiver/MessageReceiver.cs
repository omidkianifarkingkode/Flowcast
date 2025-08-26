using Microsoft.Extensions.Logging;
using Realtime.Transport.Http;
using Realtime.Transport.Messaging.Factories;
using Realtime.Transport.UserConnection;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Realtime.Transport.Messaging.Receiver;

public interface IRealtimeMessageReceiver
{
    Task ReceiveTextMessage(string userId, string message, CancellationToken cancellationToken = default);
    Task ReceiveBinaryMessage(string userId, byte[] data, CancellationToken cancellationToken = default);
}

public interface IRealtimeGateway
{
    event Action<RealtimeContext, IRealtimeMessage> OnFrame;
    IAsyncEnumerable<(RealtimeContext ctx, IRealtimeMessage frame)> ReadAllAsync(CancellationToken ct);
}

public class RealtimeMessageReceiver(
    IRealtimePayloadFactory factory,
    IUserConnectionRegistry registry,
    ILogger<RealtimeMessageReceiver> logger)
    : IRealtimeMessageReceiver, IRealtimeGateway
{
    private readonly Channel<(RealtimeContext, IRealtimeMessage)> _channel = Channel.CreateBounded<(RealtimeContext, IRealtimeMessage)>(10);

    public event Action<RealtimeContext, IRealtimeMessage> OnFrame;

    public Task ReceiveTextMessage(string userId, string data, CancellationToken cancellationToken = default)
    {
        registry.MarkClientActivity(userId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

        var message = factory.CreateFromJson(data);

        return HandleMessage(userId, message, cancellationToken);
    }

    public Task ReceiveBinaryMessage(string userId, byte[] data, CancellationToken cancellationToken = default)
    {
        registry.MarkClientActivity(userId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

        var message = factory.CreateFromBinary(data);

        return HandleMessage(userId, message, cancellationToken);
    }

    public async IAsyncEnumerable<(RealtimeContext ctx, IRealtimeMessage frame)> ReadAllAsync([EnumeratorCancellation] CancellationToken ct)
    {
        while (await _channel.Reader.WaitToReadAsync(ct))
            while (_channel.Reader.TryRead(out var item))
                yield return item;
    }

    private async Task HandleMessage(string userId, IRealtimeMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Incoming message from {UserId}: Type={Type}, Id={Id}, Time={Timestamp}",
            userId, message.Header.Type, message.Header.Id, message.Header.Timestamp);

        registry.TryGetUserConnectionInfo(userId, out var info);
        var context = new RealtimeContext { UserId = userId, ConnectionId = info.ConnectionId, Header = message.Header };

        // event
        OnFrame?.Invoke(context, message);
        // buffer
        await _channel.Writer.WriteAsync((context, message), cancellationToken);

        //return commandDispatcher.DispatchAsync(userId, message, cancellationToken);
    }
}

