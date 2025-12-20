using Google.Apis.Auth;
using Identity.Application.Services;
using Identity.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Services;

public sealed class GooglePlayGamesVerifier(
    GoogleOAuthClient oauth,
    HttpClient http,
    ILogger<GooglePlayGamesVerifier> logger
) : IGooglePlayGamesVerifier
{
    private const string PlayerMeEndpoint =
        "https://games.googleapis.com/games/v1/players/me";

    public async Task<Result<GooglePlayerIdentity>> VerifyAsync(
        string serverAuthCode,
        CancellationToken ct)
    {
        var tokenResult = await oauth.ExchangeCodeAsync(serverAuthCode, ct);
        if (tokenResult.IsFailure)
            return Result.Failure<GooglePlayerIdentity>(tokenResult.Error);

        var (accessToken, idToken) = tokenResult.Value;

        var playerResult = await FetchPlayerIdAsync(accessToken, ct);
        if (playerResult.IsFailure)
            return Result.Failure<GooglePlayerIdentity>(playerResult.Error);

        //var (sub, email) = await TryValidateIdTokenAsync(idToken);

        return Result.Success(
            new GooglePlayerIdentity(
                playerResult.Value
            )
        );
    }

    private async Task<Result<string>> FetchPlayerIdAsync(
        string accessToken,
        CancellationToken ct)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, PlayerMeEndpoint);
        req.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        using var res = await http.SendAsync(req, ct);

        if (!res.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "players/me failed: {Status}",
                res.StatusCode
            );

            return Result.Failure<string>(
                Error.Unauthorized(
                    "GPG.PlayerFetchFailed",
                    "Failed to fetch player profile"
                )
            );
        }

        JsonElement json;
        try
        {
            json = await res.Content.ReadFromJsonAsync<JsonElement>(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to parse players/me response");
            return Result.Failure<string>(
                Error.Problem(
                    "GPG.PlayerParseFailed",
                    "Invalid player response format"
                )
            );
        }
        logger.LogWarning(
    "PlayerMeEndpoint returned {StatusCode}. Body: {json}",
    res.StatusCode,
    json.ToString()
);
        if (!json.TryGetProperty("playerId", out var id) ||
            id.ValueKind != JsonValueKind.String)
        {
            logger.LogWarning(
                "players/me response missing playerId: {Json}",
                json.ToString()
            );

            return Result.Failure<string>(
                Error.Problem(
                    "GPG.NoPlayerId",
                    "PlayerId not returned by Google"
                )
            );
        }

        return Result.Success(id.GetString()!);
    }

    private async Task<(string? sub, string? email)> TryValidateIdTokenAsync(
        string? idToken)
    {
        if (string.IsNullOrWhiteSpace(idToken))
            return default;

        logger.LogInformation(
"into TryValidateIdTokenAsync"
);
        try
        {
            logger.LogInformation(
"before GoogleJsonWebSignature"
);
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
            logger.LogInformation(
"after GoogleJsonWebSignature"
);
            logger.LogInformation(
    "Player email {Email}",
    payload.Email
);
            return (payload.Subject, payload.Email);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "ID token validation failed");
            return default;
        }
    }
}


public sealed class GoogleOAuthClient(
    HttpClient http,
    IOptions<GooglePlayGamesOptions> options,
    ILogger<GoogleOAuthClient> logger)
{
    private readonly GooglePlayGamesOptions _opt = options.Value;

    private const string TokenEndpoint =
        "https://oauth2.googleapis.com/token";

    public async Task<Result<(string accessToken, string? idToken)>> ExchangeCodeAsync(
        string code,
        CancellationToken ct)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _opt.ClientId,
            ["client_secret"] = _opt.ClientSecret,
            ["grant_type"] = "authorization_code"
        });

        HttpResponseMessage res;

        try
        {
            res = await http.PostAsync(TokenEndpoint, content, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to call Google token endpoint");
            return Result.Failure<(string, string?)>(
                Error.Problem("GPG.HttpFailure", "Token endpoint unreachable")
            );
        }

        var body = await res.Content.ReadAsStringAsync(ct);

        if (!res.IsSuccessStatusCode)
            return FailTokenExchange(res.StatusCode, body);

        var json = JsonDocument.Parse(body).RootElement;

        var accessToken =
            json.TryGetProperty("access_token", out var at)
                ? at.GetString()
                : null;

        if (string.IsNullOrEmpty(accessToken))
        {
            logger.LogWarning("access_token missing: {Body}", body);
            return Result.Failure<(string, string?)>(
                Error.Unauthorized("GPG.NoAccessToken", "No access_token returned")
            );
        }

        var idToken =
            json.TryGetProperty("id_token", out var id)
                ? id.GetString()
                : null;

        return Result.Success((accessToken, idToken));
    }
    public async Task<Result<(string accessToken, string? idToken, string? rawGoogleTokenResponse)>> ExchangeCodeTestAsync(
        string code,
        CancellationToken ct)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _opt.ClientId,
            ["client_secret"] = _opt.ClientSecret,
            ["grant_type"] = "authorization_code"
        });

        HttpResponseMessage res;

        try
        {
            res = await http.PostAsync(
                new Uri("https://oauth2.googleapis.com/token"),
                content,
                ct
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to call Google token endpoint");
            return Result.Failure<(string, string?, string?)>(
                Error.Problem("GPG.HttpFailure", "Token endpoint unreachable")
            );
        }

        var body = await res.Content.ReadAsStringAsync(ct);

        if (!res.IsSuccessStatusCode)
            return FailTokenTestExchange(res.StatusCode, body);

        var json = JsonDocument.Parse(body).RootElement;

        var accessToken =
            json.TryGetProperty("access_token", out var at)
                ? at.GetString()
                : null;

        if (string.IsNullOrEmpty(accessToken))
        {
            logger.LogWarning("access_token missing: {Body}", json.ToString());
            return Result.Failure<(string, string?, string?)>(
                Error.Unauthorized("GPG.NoAccessToken", "No access_token returned")
            );
        }

        var idToken =
            json.TryGetProperty("id_token", out var id)
                ? id.GetString()
                : null;
        logger.LogInformation(
    "Google token response received. Length={Len}, HasAccessToken={HasToken}",
    body.Length,
    !string.IsNullOrEmpty(accessToken)
);

        logger.LogInformation(
    "Google token endpoint returned {StatusCode}. Body: {Body}",
    res.StatusCode,
    body
);

        return Result.Success((accessToken, idToken, body.Length.ToString()));
    }

    private Result<(string, string?)> FailTokenExchange(
        HttpStatusCode status,
        string body)
    {
        try
        {
            var json = JsonDocument.Parse(body).RootElement;
            var err = json.GetProperty("error").GetString();
            var desc = json.TryGetProperty("error_description", out var d)
                ? d.GetString()
                : null;

            logger.LogWarning(
                "Token exchange failed: {Status} {Error} {Desc}",
                status, err, desc
            );

            return Result.Failure<(string, string?)>(
                Error.Unauthorized(
                    "GPG.TokenExchangeFailed",
                    $"{err} {desc}"
                )
            );
        }
        catch
        {
            logger.LogWarning(
                "Token exchange failed: {Status} non-json response",
                status
            );

            return Result.Failure<(string, string?)>(
                Error.Unauthorized(
                    "GPG.TokenExchangeFailed",
                    "Invalid token response"
                )
            );
        }
    }

    private Result<(string, string?, string?)> FailTokenTestExchange(
        HttpStatusCode status,
        string body)
    {
        try
        {
            var json = JsonDocument.Parse(body).RootElement;
            var err = json.GetProperty("error").GetString();
            var desc = json.TryGetProperty("error_description", out var d)
                ? d.GetString()
                : null;

            logger.LogWarning(
                "Token exchange failed: {Status} {Error} {Desc}",
                status, err, desc
            );

            return Result.Failure<(string, string?, string?)>(
                Error.Unauthorized(
                    "GPG.TokenExchangeFailed",
                    $"{err} {desc}"
                )
            );
        }
        catch
        {
            logger.LogWarning(
                "Token exchange failed: {Status} non-json response",
                status
            );

            return Result.Failure<(string, string?, string?)>(
                Error.Unauthorized(
                    "GPG.TokenExchangeFailed",
                    "Invalid token response"
                )
            );
        }
    }
}