namespace Identity.API.Options;

/// <summary>
/// Options for DB-backed signing keys, caching, rotation policy, and JWKS exposure.
/// </summary>
public class KeyManagementOptions
{
    /// <summary>
    /// If true, generate and activate an initial key on startup when none exist.
    /// </summary>
    public bool AutoSeedOnStartup { get; set; } = true;

    /// <summary>
    /// Planned rotation cadence in days (manual trigger can still be used).
    /// </summary>
    public int RotationDays { get; set; } = 90;

    /// <summary>
    /// Overlap window in days to keep retired public keys available via JWKS for validation.
    /// </summary>
    public int OverlapDays { get; set; } = 30;

    /// <summary>
    /// RSA key size used when generating RS* keys. Valid: 2048, 3072, 4096. Ignored for ES*.
    /// </summary>
    public int RsaKeySize { get; set; } = 2048;

    /// <summary>
    /// If false, the service will avoid persisting private keys in the database (validators).
    /// Issuer nodes should keep this true. Your key store implementation decides enforcement.
    /// </summary>
    public bool PersistPrivateKey { get; set; } = true;

    /// <summary>
    /// Cache TTL (minutes) for the active signing key material.
    /// </summary>
    public int ActiveKeyCacheMinutes { get; set; } = 1;

    /// <summary>
    /// Cache TTL (minutes) for the validation key set (JWKS).
    /// </summary>
    public int ValidationSetCacheMinutes { get; set; } = 5;

    /// <summary>
    /// JWKS path (usually '/.well-known/jwks.json'). Primarily for documentation/health.
    /// </summary>
    public string JwksPath { get; set; } = "/.well-known/jwks.json";
}
