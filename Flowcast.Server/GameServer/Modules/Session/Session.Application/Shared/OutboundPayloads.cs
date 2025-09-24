using MessagePack;
using Realtime.Transport.Messaging;
using Session.Application.Shared;

namespace Application.Sessions.Shared;

[MessagePackObject]
public sealed class SessionCreatedCommand : IPayload
{
    public const ushort Type = SessionV1.Created;

    [Key(0)] public Guid SessionId { get; set; }
    [Key(1)] public string Mode { get; set; } = "";
    [Key(2)] public string StartBarrier { get; set; } = "ConnectedAndLoaded";
    [Key(3)] public DateTime CreatedAtUtc { get; set; }
    [Key(4)] public DateTime? JoinDeadlineUtc { get; set; }
    [Key(5)] public Guid[] Participants { get; set; } = [];
}

[MessagePackObject]
public sealed class SessionCreateFailCommand : IPayload
{
    public const ushort Type = SessionV1.CreateFail;

    [Key(0)] public string ReasonCode { get; set; } = "";
    [Key(1)] public string Message { get; set; } = "";
}