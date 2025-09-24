using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Identity.API.Options;

public sealed class IdentityOptionsValidator : IValidateOptions<IdentityOptions>
{
    private static readonly HashSet<string> SupportedAlgorithms = new(StringComparer.OrdinalIgnoreCase)
    {
        "RS256","RS384","RS512",
        "ES256","ES384","ES512",
    };

    private static readonly HashSet<int> AllowedRsaKeySizes = new() { 2048, 3072, 4096 };

    public ValidateOptionsResult Validate(string? name, IdentityOptions options)
    {
        var errors = new List<string>();
        if (options is null)
            return ValidateOptionsResult.Fail("Identity options missing.");

        // --- Database ---
        if (!options.UseInMemoryDatabase && string.IsNullOrWhiteSpace(options.ConnectionString))
            errors.Add("Identity:ConnectionString is required when UseInMemoryDatabase is false.");

        // --- TokenOptions ---
        var t = options.TokenOptions;
        if (string.IsNullOrWhiteSpace(t.Issuer))
            errors.Add("Identity:TokenOptions:Issuer is required.");
        if (string.IsNullOrWhiteSpace(t.Audience))
            errors.Add("Identity:TokenOptions:Audience is required.");

        var alg = string.IsNullOrWhiteSpace(t.Algorithm) ? "RS256" : t.Algorithm.Trim();
        if (!SupportedAlgorithms.Contains(alg))
            errors.Add($"Identity:TokenOptions:Algorithm '{alg}' is not supported. Allowed: {string.Join(", ", SupportedAlgorithms)}.");

        if (t.AccessTokenExpiryMinutes <= 0)
            errors.Add("Identity:TokenOptions:AccessTokenExpiryMinutes must be > 0.");
        if (t.RefreshTokenExpiryDays <= 0)
            errors.Add("Identity:TokenOptions:RefreshTokenExpiryDays must be > 0.");
        if (t.ClockSkewSeconds < 0)
            errors.Add("Identity:TokenOptions:ClockSkewSeconds cannot be negative.");

        // --- KeyManagementOptions ---
        var km = options.KeyManagement;

        if (km.RotationDays <= 0)
            errors.Add("Identity:KeyManagement:RotationDays must be > 0.");
        if (km.OverlapDays < 0)
            errors.Add("Identity:KeyManagement:OverlapDays cannot be negative.");
        if (km.OverlapDays > km.RotationDays * 2)
            errors.Add("Identity:KeyManagement:OverlapDays is unreasonably large (should be <= 2x RotationDays).");

        if (alg.StartsWith("RS", StringComparison.OrdinalIgnoreCase))
        {
            if (!AllowedRsaKeySizes.Contains(km.RsaKeySize))
                errors.Add($"Identity:KeyManagement:RsaKeySize must be one of {string.Join(", ", AllowedRsaKeySizes)} for RS* algorithms.");
        }

        if (km.ActiveKeyCacheMinutes < 0)
            errors.Add("Identity:KeyManagement:ActiveKeyCacheMinutes cannot be negative.");
        if (km.ValidationSetCacheMinutes < 0)
            errors.Add("Identity:KeyManagement:ValidationSetCacheMinutes cannot be negative.");
        if (string.IsNullOrWhiteSpace(km.JwksPath))
            errors.Add("Identity:KeyManagement:JwksPath is required.");

        // PersistPrivateKey note: no hard failure. Enforcement is implementation-specific.

        // --- GoogleOptions ---
        var g = options.Google;
        if (g.ClientIds is null || g.ClientIds.Length == 0)
            errors.Add("Identity:Google:ClientIds must contain at least one client id.");
        if (g.ClockSkewSeconds < 0)
            errors.Add("Identity:Google:ClockSkewSeconds cannot be negative.");
        // HostedDomain and RequireEmailVerified are optional/boolean by nature.

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }
}
