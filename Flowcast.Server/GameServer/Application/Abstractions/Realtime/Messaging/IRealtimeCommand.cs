using Application.Abstractions.Messaging;
using Application.Abstractions.Realtime.Services;
using MessagePack;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Abstractions.Realtime.Messaging;

public interface IRealtimeCommand : ICommand;

[MessagePackObject]
[RealtimeMessage(RealtimeMessageType.Spawn)]
public class SpawnCommand : IRealtimeCommand
{
    [Key(0)]
    public int UnitId { get; set; }
}

public class SpawnCommandHandler(ILogger<SpawnCommandHandler> logger, IRealtimeMessageSender sender, IDateTimeProvider dateTimeProvider, IRealtimeContextAccessor contextAccessor) : ICommandHandler<SpawnCommand>
{
    public async Task<Result> Handle(SpawnCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Log from Spawn:" + command.UnitId);

        var pingMessage = RealtimeMessage.Create(RealtimeMessageType.Pong, dateTimeProvider.UnixTimeMilliseconds);

        await sender.SendToUserAsync(contextAccessor.Current.UserId, pingMessage, cancellationToken);

        return Result.Success();
    }
}
