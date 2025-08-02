namespace Domain.Sessions;

public record SessionId(string Value)
{
    public static SessionId NewId() => new(Guid.NewGuid().ToString("N"));
}
