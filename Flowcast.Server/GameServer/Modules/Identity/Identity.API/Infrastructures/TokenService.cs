using Identity.API.Options;
using Identity.API.Services;
using Identity.API.Shared;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedKernel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Identity.API.Infrastructures;

public class TokenService : ITokenService
{
    private readonly IdentityOptions _options;
    private readonly IKeyStore _keys;
    private readonly JwtSecurityTokenHandler _handler = new();

    public TokenService(IOptions<IdentityOptions> options, IKeyStore keys)
    {
        _options = options.Value;
        _keys = keys;
        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
    }

    // Issue new access and refresh tokens
    public async Task<(string access, string refresh, DateTime expiresAtUtc)> IssueAsync(Guid accountId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var accessExp = now.AddMinutes(_options.TokenOptions.AccessTokenExpiryMinutes);
        var refreshExp = now.AddDays(_options.TokenOptions.RefreshTokenExpiryDays);

        var access = await CreateJwtAsync(accountId, "access", accessExp, ct);
        var refresh = await CreateJwtAsync(accountId, "refresh", refreshExp, ct);

        return (access, refresh, accessExp);
    }

    // Refresh the access and refresh tokens using the provided refresh token
    public async Task<(bool ok, Guid accountId, string access, string refresh, DateTime expiresAtUtc)> RefreshAsync(string refreshToken, CancellationToken ct)
    {
        var res = await ValidateJwtAsync(refreshToken, "refresh", validateLifetime: true, ct);
        if (res.IsFailure) return (false, Guid.Empty, "", "", DateTime.MinValue);

        var principal = res.Value;
        var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
               ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(sub, out var accountId))
            return (false, Guid.Empty, "", "", DateTime.MinValue);

        // TODO: reuse detection + family rotation checks before issuing new tokens
        var now = DateTime.UtcNow;
        var accessExp = now.AddMinutes(_options.TokenOptions.AccessTokenExpiryMinutes);
        var refreshExp = now.AddDays(_options.TokenOptions.RefreshTokenExpiryDays);

        var access = await CreateJwtAsync(accountId, "access", accessExp, ct);
        var refresh = await CreateJwtAsync(accountId, "refresh", refreshExp, ct);

        return (true, accountId, access, refresh, accessExp);
    }

    // Revoke refresh token
    public Task<bool> RevokeAsync(string refreshToken, CancellationToken ct)
    {
        // TODO: Persist refresh token families and mark as revoked,
        // and on reuse detection, revoke the whole family.
        return Task.FromResult(true);
    }

    // ----------------- helpers -----------------

    private async Task<string> CreateJwtAsync(Guid accountId, string tokenUse, DateTime expiresAtUtc, CancellationToken ct)
    {
        var km = await _keys.GetActiveAsync(ct);
        if (km is null || string.IsNullOrWhiteSpace(km.PrivateKeyPem))
            throw new InvalidOperationException("No active signing key with private key available.");

        var creds = BuildSigningCredentials(km);

        var now = DateTime.UtcNow;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, accountId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, ((long)(now - DateTime.UnixEpoch).TotalSeconds).ToString(), ClaimValueTypes.Integer64),
            new("token_use", tokenUse)
        };

        var jwt = new JwtSecurityToken(
            issuer: _options.TokenOptions.Issuer,
            audience: _options.TokenOptions.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAtUtc,
            signingCredentials: creds);

        // ensure kid is present (SigningCredentials.Key.KeyId should add it, but set explicitly)
        jwt.Header["kid"] = km.Kid;

        return _handler.WriteToken(jwt);
    }

    private SigningCredentials BuildSigningCredentials(KeyMaterial km)
    {
        var alg = km.Algorithm.ToUpperInvariant();

        if (alg.StartsWith("RS"))
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(km.PrivateKeyPem!.ToCharArray());
            var key = new RsaSecurityKey(rsa) { KeyId = km.Kid };

            var sigAlg = alg switch
            {
                "RS256" => SecurityAlgorithms.RsaSha256,
                "RS384" => SecurityAlgorithms.RsaSha384,
                "RS512" => SecurityAlgorithms.RsaSha512,
                _ => SecurityAlgorithms.RsaSha256
            };
            return new SigningCredentials(key, sigAlg);
        }

        if (alg.StartsWith("ES"))
        {
            var ec = ECDsa.Create();
            ec.ImportFromPem(km.PrivateKeyPem!.ToCharArray());
            var key = new ECDsaSecurityKey(ec) { KeyId = km.Kid };

            var sigAlg = alg switch
            {
                "ES256" => SecurityAlgorithms.EcdsaSha256,
                "ES384" => SecurityAlgorithms.EcdsaSha384,
                "ES512" => SecurityAlgorithms.EcdsaSha512,
                _ => SecurityAlgorithms.EcdsaSha256
            };
            return new SigningCredentials(key, sigAlg);
        }

        throw new NotSupportedException($"Algorithm '{km.Algorithm}' not supported.");
    }

    private async Task<Result<ClaimsPrincipal>> ValidateJwtAsync(string token, string expectedUse, bool validateLifetime, CancellationToken ct)
    {
        var materials = await _keys.GetValidationSetAsync(ct);
        if (materials.Count == 0)
            return Result.Failure<ClaimsPrincipal>(TokenErrors.InvalidSignatureOrClaims);

        var keys = new List<SecurityKey>(materials.Count);
        foreach (var km in materials)
        {
            var alg = km.Algorithm.ToUpperInvariant();
            if (alg.StartsWith("RS"))
            {
                var rsa = RSA.Create();
                rsa.ImportFromPem(km.PublicKeyPem.ToCharArray());
                keys.Add(new RsaSecurityKey(rsa) { KeyId = km.Kid });
            }
            else if (alg.StartsWith("ES"))
            {
                var ec = ECDsa.Create();
                ec.ImportFromPem(km.PublicKeyPem.ToCharArray());
                keys.Add(new ECDsaSecurityKey(ec) { KeyId = km.Kid });
            }
        }

        var p = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _options.TokenOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = _options.TokenOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = keys,
            RequireSignedTokens = true,
            ValidateLifetime = validateLifetime,
            ClockSkew = TimeSpan.FromSeconds(Math.Max(0, _options.TokenOptions.ClockSkewSeconds))
        };

        try
        {
            var principal = _handler.ValidateToken(token, p, out var validated);
            if (validated is not JwtSecurityToken jwt)
                return Result.Failure<ClaimsPrincipal>(TokenErrors.InvalidFormat);

            var alg = jwt.Header.Alg ?? "";
            var expectedAlg = _options.TokenOptions.Algorithm?.ToUpperInvariant() ?? "RS256";
            if (!alg.Equals(expectedAlg, StringComparison.OrdinalIgnoreCase))
                return Result.Failure<ClaimsPrincipal>(TokenErrors.InvalidAlgorithm);

            var use = principal.FindFirst("token_use")?.Value;
            if (!string.Equals(use, expectedUse, StringComparison.Ordinal))
                return Result.Failure<ClaimsPrincipal>(TokenErrors.WrongTokenType);

            return principal;
        }
        catch (SecurityTokenExpiredException)
        {
            return Result.Failure<ClaimsPrincipal>(TokenErrors.Expired);
        }
        catch
        {
            return Result.Failure<ClaimsPrincipal>(TokenErrors.InvalidSignatureOrClaims);
        }
    }
}
