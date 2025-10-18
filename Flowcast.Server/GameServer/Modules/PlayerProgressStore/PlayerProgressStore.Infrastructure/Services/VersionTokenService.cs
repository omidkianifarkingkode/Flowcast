using PlayerProgressStore.Application.Services;
using PlayerProgressStore.Domain;

namespace PlayerProgressStore.Infrastructure.Services;

public class VersionTokenService : IVersionTokenService
{
    public VersionToken Next(VersionToken current)
    {
        // Example: "v20251015-5f84b2dcb6"
        var token = $"v{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}"[..20];
        return new VersionToken(token);
    }
}
