using PlayerProgressStore.Domain;

namespace PlayerProgressStore.Application.Services;

/// <summary>
/// Generates new server-owned version tokens (opaque, monotonic per namespace).
/// </summary>
public interface IVersionTokenService
{
    VersionToken Next(VersionToken current);
}
