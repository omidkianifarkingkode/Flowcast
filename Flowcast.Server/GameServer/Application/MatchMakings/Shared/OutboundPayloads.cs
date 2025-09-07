using MessagePack;
using Realtime.Transport.Messaging;

namespace Application.MatchMakings.Shared
{
    [MessagePackObject]
    public sealed class MatchQueuedCommand : IPayload
    {
        public const ushort Type = MatchmakingV1.Queued;

        [Key(0)] public Guid TicketId { get; set; }
        [Key(1)] public string Mode { get; set; } = "";
        [Key(2)] public DateTime EnqueuedAtUtc { get; set; }
    }

    [MessagePackObject]
    public sealed class MatchFoundCommand : IPayload
    {
        public const ushort Type = MatchmakingV1.Found;

        [Key(0)] public Guid MatchId { get; set; }
        [Key(1)] public string Mode { get; set; } = "";
        [Key(2)] public Guid[] Players { get; set; } = [];
        [Key(3)] public long ReadyDeadlineUnixMs { get; set; }
    }

    [MessagePackObject]
    public sealed class MatchFoundFailCommand : IPayload
    {
        public const ushort Type = MatchmakingV1.FoundFail;

        [Key(0)] public string Mode { get; set; } = "";
        [Key(1)] public string ReasonCode { get; set; } = "";
        [Key(2)] public string Message { get; set; } = "";
        [Key(3)] public bool Retryable { get; set; }
    }

    [MessagePackObject]
    public sealed class MatchAbortedCommand : IPayload
    {
        public const ushort Type = MatchmakingV1.Aborted;

        [Key(0)] public Guid MatchId { get; set; }
        [Key(1)] public string Reason { get; set; } = "";
    }

    [MessagePackObject]
    public sealed class MatchConfirmedCommand : IPayload
    {
        public const ushort Type = MatchmakingV1.Confirmed;

        [Key(0)] public Guid MatchId { get; set; }
        [Key(1)] public string Mode { get; set; } = "";
        [Key(2)] public Guid[] Players { get; set; } = [];
    }

    [MessagePackObject]
    public sealed class CancelMatchFailCommand : IPayload
    {
        public const ushort Type = MatchmakingV1.CancelFail;

        [Key(0)] public string Mode { get; set; } = "";
        [Key(1)] public string ReasonCode { get; set; } = "";
        [Key(2)] public string Message { get; set; } = "";
    }

    [MessagePackObject]
    public sealed class TicketCancelledCommand : IPayload
    {
        public const ushort Type = MatchmakingV1.Cancelled;

        [Key(0)] public Guid TicketId { get; set; }
        [Key(1)] public string Mode { get; set; } = "";
        [Key(2)] public DateTime EnqueuedAtUtc { get; set; }
        [Key(3)] public string State { get; set; } = "Cancelled"; // could be other (e.g., Failed)
    }

    [MessagePackObject]
    public sealed class ReadyAcknowledgedCommand : IPayload
    {
        public const ushort Type = MatchmakingV1.ReadyAck;

        [Key(0)] public Guid MatchId { get; set; }
        [Key(1)] public Guid[] ReadyPlayers { get; set; } = [];
        [Key(2)] public long? ReadyDeadlineUnixMs { get; set; }  // null if no deadline
    }

    [MessagePackObject]
    public sealed class ReadyAcknowledgeFailCommand : IPayload
    {
        public const ushort Type = MatchmakingV1.ReadyAckFail;

        [Key(0)] public Guid MatchId { get; set; }
        [Key(1)] public string ReasonCode { get; set; } = "";
        [Key(2)] public string Message { get; set; } = "";
    }

    //[MessagePackObject]
    //[RealtimeMessage((ushort)RealtimeMessageType.SessionAllocated)]
    //public sealed class SessionAllocatedCmd : IPayload
    //{
    //    [Key(0)] public Guid SessionId { get; init; }
    //    [Key(1)] public string? Endpoint { get; init; } // null if same socket
    //    [Key(2)] public string JoinToken { get; init; } = "";
    //    [Key(3)] public long JoinDeadlineUnixMs { get; init; }
    //    [Key(4)] public string StartBarrier { get; init; } = "ConnectedAndLoaded";
    //}
}
