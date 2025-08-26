using Application.Abstractions.Messaging;
using MessagePack;
using Realtime.Transport.Messaging;

namespace Presentation.Realtime.Contracts
{
    [MessagePackObject]
    [RealtimeMessage((ushort)RealtimeMessageType.Ping)]
    public sealed class PingCommand : ICommand
    {
        [Key(0)] public ulong PingId { get; set; }
        [Key(1)] public long ServerTimestamp { get; set; }
    }

    [MessagePackObject]
    [RealtimeMessage((ushort)RealtimeMessageType.MatchFound)]
    public sealed class MatchFoundCmd : ICommand
    {
        [Key(0)] public Guid MatchId { get; init; }
        [Key(1)] public string Mode { get; init; } = "";
        [Key(2)] public Guid[] Players { get; init; } = Array.Empty<Guid>();
        [Key(3)] public long ReadyDeadlineUnixMs { get; init; }
    }

    [MessagePackObject]
    [RealtimeMessage((ushort)RealtimeMessageType.MatchAborted)]
    public sealed class MatchAbortedCmd : ICommand
    {
        [Key(0)] public Guid MatchId { get; init; }
        [Key(1)] public string Reason { get; init; } = "";
    }

    [MessagePackObject]
    [RealtimeMessage((ushort)RealtimeMessageType.MatchConfirmed)]
    public sealed class MatchConfirmedCmd : ICommand
    {
        [Key(0)] public Guid MatchId { get; init; }
        [Key(1)] public string Mode { get; init; } = "";
        [Key(2)] public Guid[] Players { get; init; } = Array.Empty<Guid>();
    }

    [MessagePackObject]
    [RealtimeMessage((ushort)RealtimeMessageType.SessionAllocated)]
    public sealed class SessionAllocatedCmd : ICommand
    {
        [Key(0)] public Guid SessionId { get; init; }
        [Key(1)] public string? Endpoint { get; init; } // null if same socket
        [Key(2)] public string JoinToken { get; init; } = "";
        [Key(3)] public long JoinDeadlineUnixMs { get; init; }
        [Key(4)] public string StartBarrier { get; init; } = "ConnectedAndLoaded";
    }
}
