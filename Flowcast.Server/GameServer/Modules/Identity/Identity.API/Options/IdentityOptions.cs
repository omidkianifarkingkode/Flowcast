namespace Identity.API.Options;

/// <summary>
/// Root options for the Identity service.
/// </summary>
public class IdentityOptions
{
    /// <summary>
    /// Database connection string. Required unless <see cref="UseInMemoryDatabase"/> is true.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Use EF InMemory provider (dev/test only).
    /// </summary>
    public bool UseInMemoryDatabase { get; set; } = false;

    /// <summary>
    /// Token issuance and validation options (issuer, audience, expirations, algorithm, skew).
    /// </summary>
    public TokenOptions TokenOptions { get; set; } = new();

    /// <summary>
    /// Google ID token verification options.
    /// </summary>
    public GoogleOptions Google { get; set; } = new();

    /// <summary>
    /// Server-side key management options for DB-backed signing keys and JWKS.
    /// </summary>
    public KeyManagementOptions KeyManagement { get; set; } = new();
}