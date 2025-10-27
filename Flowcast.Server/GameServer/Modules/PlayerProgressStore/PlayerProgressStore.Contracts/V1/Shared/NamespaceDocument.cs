namespace PlayerProgressStore.Contracts.V1.Shared;

/// <summary>
/// Server → Client document for a namespace (authoritative).
/// </summary>
/// <param name="Namespace">Namespace name.</param>
/// <param name="Document">Authoritative document payload as raw bytes.</param>
/// <param name="Version">Server-owned version token (etag-like, monotonic/opaque).</param>
/// <param name="Progress">Authoritative progress value.</param>
/// <param name="Hash">Server-computed SHA-256 of the raw document bytes (e.g., "sha256:abcd...").</param>
/// <param name="UpdatedAtUtc">Last update timestamp in UTC.</param>
public readonly record struct NamespaceDocument(
    string Namespace,
    byte[] Document,
    string Version,
    long Progress,
    string Hash,
    DateTimeOffset UpdatedAtUtc
);
