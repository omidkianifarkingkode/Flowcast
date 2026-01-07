using System.Net;
using System.Text.Json;
using ErrorOr;
using Identity.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel;
using Error = SharedKernel.Error;
using Result = SharedKernel.Result;

namespace Identity.Infrastructure.Services;

public sealed class GoogleOAuthClient(
    HttpClient http,
    IOptions<IdentityOptions> opt,
    ILogger<GoogleOAuthClient> logger)
{
    private readonly GooglePlayGamesOptions _opt = opt.Value.GooglePlay;
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";

    public async Task<Result<(string accessToken, string? idToken)>> ExchangeCodeAsync(
        string code,
        CancellationToken ct)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = code,
            ["client"] = _opt.ClientId,
            ["client_secret"] = _opt.ClientSecret,
            ["grant_type"] = "authorization_code",
            ["redirect_url"] = ""
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
        
        var accessToken = json.TryGetProperty("access_token", out var at)
            ? at.GetString()
            : null;

        if (string.IsNullOrEmpty(accessToken))
        {
            logger.LogError("access_token is missing: {Body}", body);
            return Result.Failure<(string, string?)>(
                Error.Unauthorized("GPG.NoAccerssToken", "No access_token returend")
            );
        }
        var idToken = json.TryGetProperty("id_token", out var id)
            ? id.GetString()
            : null;
        return Result.Success((accessToken, idToken));
        
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
}