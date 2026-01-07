using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Identity.Application.Services;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Identity.Infrastructure.Services;

public sealed class GooglePlayGamesVerifier(
    GoogleOAuthClient oauth,
    HttpClient http,
    ILogger<GooglePlayGamesVerifier> logger
) : IGooglePlayGamesVerifier
{
    private const string PlayerMeEndpoint = "https://games.googleapis.com/games/v1/players/me";
    public async Task<Result<string>> VerifyAsync(string serverAuthCode, CancellationToken ct = default)
    {
        //try exchange server authCode
        var tokenResult = await oauth.ExchangeCodeAsync(serverAuthCode, ct);
        //failure check I used Error check with Result class on SharedKernel
        if(tokenResult.IsFailure) return Result.Failure<string>(tokenResult.Error);
        
        //catch the access token 
        var (accessToken, _) = tokenResult.Value;
        
        //fetch player result
        var playerResult = await FetchPlayerIdAsync(accessToken, ct);
        if (playerResult.IsFailure)
            return Result.Failure<string>(playerResult.Error);

        return Result.Success(playerResult.Value);
        
        
    }

    private async Task<Result<string>> FetchPlayerIdAsync(
        string accessToken,
        CancellationToken ct)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, PlayerMeEndpoint);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var res = await http.SendAsync(req, ct);

        if (!res.IsSuccessStatusCode)
        {
            logger.LogWarning("players/me failed: {Status}", res.StatusCode);
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

        if (!json.TryGetProperty("playerId", out var id) ||
            id.ValueKind != JsonValueKind.String)
        {
            logger.LogWarning("players/me response missing playerId: {Json}", json.ToString());
            return Result.Failure<string>(
                Error.Problem(
                    "GPG.NoPlayerId",
                    "PlayerId not returned by Google"
                )
            );
        }

        return Result.Success(id.GetString()!);
    }
}