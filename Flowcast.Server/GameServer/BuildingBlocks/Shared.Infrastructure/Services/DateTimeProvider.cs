using Shared.Application.Services;

namespace Shared.Infrastructure.Services;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateTimeOffset UtcNowOffset => DateTimeOffset.UtcNow;

    public long UnixTimeMilliseconds => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}
