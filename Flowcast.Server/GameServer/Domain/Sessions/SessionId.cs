namespace Domain.Sessions;

public record SessionId(string Value)
{
    public static SessionId NewId() => new(Guid.NewGuid().ToString("N"));

    public static SessionId FromGuid(Guid id) => new(id.ToString("N"));
}
