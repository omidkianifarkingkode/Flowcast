namespace Identity.Infrastructure.Options;

/// <summary>
/// Google ID token verification settings.
/// </summary>
public class GoogleOptions
{
    /// <summary>Allowed Google OAuth Client IDs (audiences).</summary>
    public string[] ClientIds { get; set; } = [];

    /// <summary>Optional: restrict to a Google Workspace domain ("hd").</summary>
    public string? HostedDomain { get; set; }

    /// <summary>Require payload.EmailVerified to be true.</summary>
    public bool RequireEmailVerified { get; set; } = true;

    /// <summary>Clock skew for iat/exp tolerance in Google payload validation.</summary>
    public int ClockSkewSeconds { get; set; } = 60;
}
