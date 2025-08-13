using Application.Abstractions.Realtime.Messaging;
using Application.Abstractions.Realtime.Services;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Realtime.Services;

public class RealtimeMessageReceiver(
    IRealtimeCommandFactory factory,
    ICommandDispatcher commandDispatcher,
    ILogger<RealtimeMessageReceiver> logger)
    : IRealtimeMessageReceiver
{
    public Task ReceiveTextMessage(Guid userId, string data, CancellationToken cancellationToken = default)
    {
        var message = factory.CreateFromJson(data); // factory must provide not generic ICommand

        return HandleMessage(userId, message, cancellationToken);
    }

    public Task ReceiveBinaryMessage(Guid userId, byte[] data, CancellationToken cancellationToken = default)
    {
        var message = factory.CreateFromBinary(data); // factory must provide not generic ICommand

        return HandleMessage(userId, message, cancellationToken);
    }

    private Task HandleMessage(Guid userId, IRealtimeMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Incoming message from {UserId}: Type={Type}, Id={Id}, Time={Timestamp}",
            userId, message.Header.Type, message.Header.Id, message.Header.Timestamp);

        return commandDispatcher.DispatchAsync(userId, message, cancellationToken);
    }
}