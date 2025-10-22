namespace PlayerProgressStore.Contracts.V1.Shared;

/// <summary>
/// Client → Server write model for a single namespace represented as raw UTF-8 bytes.
/// </summary>
/// <param name="Namespace">Logical document bucket (e.g., "playerStats", "inventory", or "global").</param>
/// <param name="Document">The client document payload encoded as UTF-8 byte array.</param>
/// <param name="Progress">Semantic gameplay progress metric used for conflict resolution.</param>
/// <param name="ClientVersion">
/// The last server version the client saw for this namespace (sync anchor).
/// If null/missing, treated as "v0".
/// </param>
/// <param name="ClientHash">
/// Optional hint for no-op/dedupe. The server should not trust this for conflict decisions.
/// </param>
public readonly record struct NamespaceBinaryWrite(
    string Namespace,
    byte[]? Document,
    long Progress,
    string? ClientVersion = null,
    string? ClientHash = null
);
