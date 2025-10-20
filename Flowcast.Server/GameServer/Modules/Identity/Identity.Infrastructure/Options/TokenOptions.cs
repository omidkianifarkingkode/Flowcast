namespace Identity.Infrastructure.Options;

/// <summary>
/// Token issuance/validation settings (PEMs are no longer used; keys are DB-backed).
/// </summary>
public class TokenOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Asymmetric algorithm: RS256/RS384/RS512 or ES256/ES384/ES512.
    /// </summary>
    public string Algorithm { get; set; } = "RS256";

    /// <summary>
    /// Access token lifetime in minutes.
    /// </summary>
    public int AccessTokenExpiryMinutes { get; set; } = 15;

    /// <summary>
    /// Refresh token lifetime in days.
    /// </summary>
    public int RefreshTokenExpiryDays { get; set; } = 30;

    /// <summary>
    /// Allowed clock skew (seconds) when validating tokens.
    /// </summary>
    public int ClockSkewSeconds { get; set; } = 60;
}
