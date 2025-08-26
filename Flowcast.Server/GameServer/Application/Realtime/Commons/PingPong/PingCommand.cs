//using Application.Abstractions.Messaging;
//using Application.Realtime.Messaging;
//using Application.Realtime.Messaging.Core;
//using MessagePack;
//using SharedKernel;

//namespace Application.Realtime.Commons.PingPong;

//[MessagePackObject]
//[RealtimeMessage(RealtimeMessageType.MatchMaking)]
//public sealed class PingCommand : ICommand
//{
//    [Key(0)] public ulong PingId { get; set; }
//    [Key(1)] public long ServerTimestamp { get; set; }
//}

//public sealed class PingCommandHandler : ICommandHandler<PingCommand>
//{
//    public Task<Result> Handle(PingCommand command, CancellationToken cancellationToken)
//        => Task.FromResult(Result.Success()); // No-op (server-originated heartbeats)
//}
