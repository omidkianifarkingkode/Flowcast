using Application.Realtime.Messaging;
using Application.Realtime.Services;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Infrastructure.Realtime.Services;

public class RealtimeMessageReceiver(
    IRealtimeCommandFactory factory,
    ICommandDispatcher commandDispatcher,
    IUserConnectionRegistry registry,
    IDateTimeProvider timeProvider,
    ILogger<RealtimeMessageReceiver> logger)
    : IRealtimeMessageReceiver
{
    public Task ReceiveTextMessage(Guid userId, string data, CancellationToken cancellationToken = default)
    {
        registry.MarkClientActivity(userId, timeProvider.UnixTimeMilliseconds);

        var message = factory.CreateFromJson(data);

        return HandleMessage(userId, message, cancellationToken);
    }

    public Task ReceiveBinaryMessage(Guid userId, byte[] data, CancellationToken cancellationToken = default)
    {
        registry.MarkClientActivity(userId, timeProvider.UnixTimeMilliseconds);

        var message = factory.CreateFromBinary(data);

        return HandleMessage(userId, message, cancellationToken);
    }

    private Task HandleMessage(Guid userId, IRealtimeMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Incoming message from {UserId}: Type={Type}, Id={Id}, Time={Timestamp}",
            userId, message.Header.Type, message.Header.Id, message.Header.Timestamp);

        return commandDispatcher.DispatchAsync(userId, message, cancellationToken);
    }
}