namespace PlayerProgressStore.Contracts.V1.Shared;

/// <summary>
/// Server → Client document for a namespace represented as raw UTF-8 bytes.
/// </summary>
/// <param name="Namespace">Namespace name.</param>
/// <param name="Document">Authoritative document payload encoded as UTF-8 byte array.</param>
/// <param name="Version">Server-owned version token (etag-like, monotonic/opaque).</param>
/// <param name="Progress">Authoritative progress value.</param>
/// <param name="Hash">Server-computed SHA-256 (canonical JSON) e.g., "sha256:abcd...".</param>
/// <param name="UpdatedAtUtc">Last update timestamp in UTC.</param>
public readonly record struct NamespaceBinaryDocument(
    string Namespace,
    byte[] Document,
    string Version,
    long Progress,
    string Hash,
    DateTimeOffset UpdatedAtUtc
);
