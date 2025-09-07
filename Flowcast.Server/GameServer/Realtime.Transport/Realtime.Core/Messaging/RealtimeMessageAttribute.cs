namespace Realtime.Transport.Messaging;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class RealtimeMessageAttribute : Attribute
{
    public ushort MessageType { get; }
    public RealtimeMessageAttribute(ushort type) => MessageType = type;
}
