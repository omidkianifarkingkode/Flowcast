using PlayerProgressStore.Domain;

namespace PlayerProgressStore.Application.Services;

/// <summary>
/// Computes stable content hashes (e.g., "sha256:abcd…") from raw document bytes.
/// </summary>
public interface IContentHashService
{
    DocHash Compute(ReadOnlySpan<byte> document);
}
