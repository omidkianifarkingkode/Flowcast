using Application.Abstractions.Messaging;
using MessagePack;
using Realtime.Transport.Messaging;

namespace Application.MatchMakings.Shared
{
    [MessagePackObject]
    public sealed class MatchQueuedCmd : IPayload
    {
        public const ushort Type = PayloadTypes.Queued;

        [Key(0)] public Guid TicketId { get; set; }
        [Key(1)] public string Mode { get; set; } = "";
        [Key(2)] public DateTime EnqueuedAtUtc { get; set; }
        [Key(3)] public string? CorrId { get; set; }      // optional echo from client
    }

    [MessagePackObject]
    public sealed class MatchFoundCmd : IPayload
    {
        public const ushort Type = PayloadTypes.Found;

        [Key(0)] public Guid MatchId { get; set; }
        [Key(1)] public string Mode { get; set; } = "";
        [Key(2)] public Guid[] Players { get; set; } = [];
        [Key(3)] public long ReadyDeadlineUnixMs { get; set; }
    }

    [MessagePackObject]
    public sealed class MatchFoundFailCmd : IPayload
    {
        public const ushort Type = PayloadTypes.FoundFail;

        [Key(0)] public string Mode { get; set; } = "";
        [Key(1)] public string ReasonCode { get; set; } = "";
        [Key(2)] public string Message { get; set; } = "";
        [Key(3)] public bool Retryable { get; set; }
        [Key(4)] public string? CorrId { get; set; }
    }

    [MessagePackObject]
    public sealed class MatchAbortedCmd : IPayload
    {
        public const ushort Type = PayloadTypes.Aborted;

        [Key(0)] public Guid MatchId { get; init; }
        [Key(1)] public string Reason { get; init; } = "";
    }

    [MessagePackObject]
    [RealtimeMessage(PayloadTypes.Confirmed)]
    public sealed class MatchConfirmedCmd : IPayload
    {
        public const ushort Type = PayloadTypes.Confirmed;

        [Key(0)] public Guid MatchId { get; init; }
        [Key(1)] public string Mode { get; init; } = "";
        [Key(2)] public Guid[] Players { get; init; } = Array.Empty<Guid>();
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
