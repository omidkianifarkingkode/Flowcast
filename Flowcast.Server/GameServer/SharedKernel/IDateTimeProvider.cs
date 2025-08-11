namespace SharedKernel;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }

    DateTimeOffset UtcNowOffset { get; }

    long UnixTimeMilliseconds { get; }
}
