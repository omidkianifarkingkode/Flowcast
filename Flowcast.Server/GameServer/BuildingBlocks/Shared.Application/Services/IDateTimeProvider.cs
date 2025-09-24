namespace Shared.Application.Services;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }

    DateTimeOffset UtcNowOffset { get; }

    long UnixTimeMilliseconds { get; }
}

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateTimeOffset UtcNowOffset => DateTimeOffset.UtcNow;

    public long UnixTimeMilliseconds => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}
