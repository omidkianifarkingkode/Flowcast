//using Application.Abstractions.Messaging;
//using Application.Realtime.Messaging;
//using Application.Realtime.Messaging.Core;
//using Application.Realtime.Services;
//using MessagePack;
//using Microsoft.Extensions.Logging;
//using Serilog;
//using SharedKernel;

//namespace Application.Realtime.Commons.PingPong;

//[MessagePackObject]
//[RealtimeMessage(RealtimeMessageType.Pong)]
//public sealed class PongCommand : IRealtimeCommand
//{
//    [Key(0)] public ulong PingId { get; set; }
//    [Key(1)] public long ClientTimestamp { get; set; } // optional
//}

//public sealed class PongCommandHandler(
//    IRealtimeContextAccessor realtimeContextAccessor,
//    IUserConnectionRegistry registry,
//    IDateTimeProvider clock,
//    ILogger<PongCommandHandler> logger)
//    : ICommandHandler<PongCommand>
//{
//    public Task<Result> Handle(PongCommand command, CancellationToken cancellationToken)
//    {
//        var userId = realtimeContextAccessor.Current?.UserId
//                     ?? throw new InvalidOperationException("Realtime context missing.");

//        var now = clock.UnixTimeMilliseconds;

//        registry.MarkPongReceived(userId, now);

//        if (command.PingId != 0 && registry.TryCompletePing(userId, command.PingId, now, out var rtt))
//        {
//            logger.LogDebug("RTT for {User} = {Rtt} ms (PingId={PingId})", userId, rtt, command.PingId);
//        }
//        else
//        {
//            logger.LogDebug("Pong for {User} but no matching PingId={PingId}.", userId, command.PingId);
//        }

//        return Task.FromResult(Result.Success());
//    }
//}