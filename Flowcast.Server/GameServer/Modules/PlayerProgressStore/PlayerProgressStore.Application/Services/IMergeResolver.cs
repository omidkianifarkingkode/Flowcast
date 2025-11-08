using SharedKernel;

namespace PlayerProgressStore.Application.Services;

/// <summary>
/// Equal-progress merge strategy per namespace.
/// Implementation lives outside the Domain.
/// </summary>
public interface IMergeResolver
{
    /// <summary>The logical namespace this resolver handles (e.g., "playerStats").</summary>
    string Namespace { get; }

    /// <summary>
    /// Merge current server document with client document (equal progress).
    /// Returns merged document bytes owned by the resolver.
    /// </summary>
    Result<byte[]> Merge(byte[] serverDocument, byte[] clientDocument);
}
