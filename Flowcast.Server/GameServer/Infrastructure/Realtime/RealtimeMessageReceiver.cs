using Application.Abstractions.Messaging;
using Application.Abstractions.Realtime;
using Application.MatchMakings.Commands;
using Domain.Sessions.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System.Text;

namespace Infrastructure.Realtime;

public class RealtimeMessageReceiver(IServiceProvider serviceProvider, IDateTimeProvider timeProvider, ILogger<RealtimeMessageReceiver> logger) : IRealtimeMessageReceiver
{
    public Task OnMessageReceivedAsync(Guid userId, string message, CancellationToken cancellationToken = default)
    {
        var data = Encoding.UTF8.GetBytes(message);
        return OnMessageReceivedAsync(userId, data, cancellationToken);
    }

    public async Task OnMessageReceivedAsync(Guid userId, byte[] data, CancellationToken cancellationToken = default)
    {
        var message = RealtimeMessage.FromBytes(data);

        logger.LogInformation($"Binary message from {userId}: Type={message.Type}, Id={message.Id}, Time={message.Timestamp}");

        if (message.Type == RealtimeMessageType.Command && message.GetPayloadAsString() == "matchmaking")
        {
            var command = new RequestMatchmakingCommand(new PlayerId(userId));
            var handler = serviceProvider.GetRequiredService<ICommandHandler<RequestMatchmakingCommand>>();
            await handler.Handle(command, cancellationToken).ConfigureAwait(false);
        }
    }
}