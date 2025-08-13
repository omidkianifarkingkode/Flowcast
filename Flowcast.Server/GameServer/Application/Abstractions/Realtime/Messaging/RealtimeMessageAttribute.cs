namespace Application.Abstractions.Realtime.Messaging;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class RealtimeMessageAttribute : Attribute
{
    public RealtimeMessageType MessageType { get; }
    public RealtimeMessageAttribute(RealtimeMessageType type) => MessageType = type;
}
