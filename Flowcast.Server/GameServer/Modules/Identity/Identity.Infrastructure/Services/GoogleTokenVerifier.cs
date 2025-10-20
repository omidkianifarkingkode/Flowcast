using Google.Apis.Auth;
using Identity.Application.Services;
using Identity.Infrastructure.Options;
using Microsoft.Extensions.Options;
using SharedKernel;

namespace Identity.Infrastructure.Services;

public sealed class GoogleTokenVerifier(IOptions<IdentityOptions> opt) : IProviderTokenVerifier
{
    private readonly GoogleOptions _opt = opt.Value.Google;

    public async Task<Result<string>> VerifyAsync(string idToken, ProviderVerifyHints? hints, CancellationToken ct)
    {
        if (_opt.ClientIds is null || _opt.ClientIds.Length == 0)
            return Result.Failure<string>(Error.Problem("Provider.Misconfigured", "Provider misconfigured: Google.ClientIds"));

        var skew = TimeSpan.FromSeconds(Math.Max(0, _opt.ClockSkewSeconds));

        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = _opt.ClientIds,
            HostedDomain = hints?.HostedDomain ?? _opt.HostedDomain,
            IssuedAtClockTolerance = skew,
            ExpirationTimeClockTolerance = skew
        };

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            if (!string.IsNullOrEmpty(hints?.Nonce) &&
                !string.Equals(payload.Nonce, hints.Nonce, StringComparison.Ordinal))
                return Result.Failure<string>(Error.Failure("Provider.InvalidNonce", "The ID token nonce does not match the expected value."));

            if (_opt.RequireEmailVerified && payload.EmailVerified != true)
                return Result.Failure<string>(Error.Unauthorized("Provider.EmailNotVerified", "The user's email address is not verified."));

            return Result.Success(payload.Subject);
        }
        catch (InvalidJwtException ex)
        {
            return Result.Failure<string>(Error.Unauthorized("Provider.InvalidToken", $"Invalid ID token: {ex.Message}"));
        }
        catch
        {
            return Result.Failure<string>(Error.Problem("Provider.Unknown", "Unknown provider verification error."));
        }
    }
}
