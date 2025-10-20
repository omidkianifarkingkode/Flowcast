namespace Shared.Application.Services;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }

    DateTimeOffset UtcNowOffset { get; }

    long UnixTimeMilliseconds { get; }
}
