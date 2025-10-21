using System.Text.Json;

namespace PlayerProgressStore.Contracts.V1.Shared;

/// <summary>
/// Client → Server write model for a single namespace.
/// </summary>
/// <param name="Namespace">Logical document bucket (e.g., "playerStats", "inventory", or "global").</param>
/// <param name="Document">The client document payload (JSON).</param>
/// <param name="Progress">Semantic gameplay progress metric used for conflict resolution.</param>
/// <param name="ClientVersion">
/// The last server version the client saw for this namespace (sync anchor).
/// If null/missing, treated as "v0".
/// </param>
/// <param name="ClientHash">
/// Optional hint for no-op/dedupe. The server should not trust this for conflict decisions.
/// </param>
public readonly record struct NamespaceWrite(
    string Namespace,
    JsonElement Document,
    long Progress,
    string? ClientVersion = null,
    string? ClientHash = null
);
