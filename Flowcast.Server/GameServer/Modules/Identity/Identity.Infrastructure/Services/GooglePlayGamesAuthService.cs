using Google.Apis.Auth;
using Identity.Application.Services;
using Identity.Infrastructure.Options;
using Microsoft.Extensions.Options;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Services;

public sealed class GooglePlayGamesVerifier(
    GoogleOAuthClient oauth,
    HttpClient http)
    : IGooglePlayGamesVerifier
{
    public async Task<Result<GooglePlayerIdentity>> VerifyAsync(
        string serverAuthCode,
        CancellationToken ct)
    {
        // 1️⃣ Exchange auth code
        var tokenRes = await oauth.ExchangeCodeAsync(serverAuthCode, ct);
        if (tokenRes.IsFailure)
            return Result.Failure<GooglePlayerIdentity>(tokenRes.Error);

        var (accessToken, idToken) = tokenRes.Value;

        // 2️⃣ Get Player ID
        var req = new HttpRequestMessage(
            HttpMethod.Get,
            "https://games.googleapis.com/games/v1/players/me");

        req.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var res = await http.SendAsync(req, ct);
        if (!res.IsSuccessStatusCode)
            return Result.Failure<GooglePlayerIdentity>(
                Error.Unauthorized("GPG.PlayerFetchFailed", "Failed to fetch player"));

        var json = await res.Content.ReadFromJsonAsync<JsonElement>(ct);
        var playerId = json.GetProperty("playerId").GetString();

        if (playerId is null)
            return Result.Failure<GooglePlayerIdentity>(
                Error.Problem("GPG.NoPlayerId", "PlayerId not returned"));

        // 3️⃣ Optional: parse id_token
        string? sub = null;
        string? email = null;

        if (!string.IsNullOrEmpty(idToken))
        {
            var payload = GoogleJsonWebSignature.ValidateAsync(idToken).Result;
            sub = payload.Subject;
            email = payload.Email;
        }

        return Result.Success(
            new GooglePlayerIdentity(playerId, sub, email));
    }
}

public sealed class GoogleOAuthClient(
    HttpClient http,
    IOptions<GooglePlayGamesOptions> options)
{
    private readonly GooglePlayGamesOptions _opt = options.Value;

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

        var res = await http.PostAsync(
            "https://oauth2.googleapis.com/token",
            content,
            ct);

        if (!res.IsSuccessStatusCode)
            return Result.Failure<(string, string?)>(
                Error.Unauthorized("GPG.TokenExchangeFailed", "Failed to exchange auth code"));

        var json = await res.Content.ReadFromJsonAsync<JsonElement>(ct);

        var accessToken = json.GetProperty("access_token").GetString();
        var idToken = json.TryGetProperty("id_token", out var id)
            ? id.GetString()
            : null;

        if (accessToken is null)
            return Result.Failure<(string, string?)>(
                Error.Unauthorized("GPG.NoAccessToken", "No access_token returned"));

        return (accessToken, idToken);
    }
}

