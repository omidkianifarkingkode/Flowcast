// Runtime/Core/Auth/OAuth2RefreshingAuthProvider.cs
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Flowcast.Rest.Client;
using Flowcast.Rest.Transport;

namespace Flowcast.Core.Auth
{
    public sealed class OAuth2RefreshingAuthProvider : IRefreshingAuthProvider
    {
        private readonly Uri _tokenEndpoint;
        private readonly string _clientId;
        private readonly string _clientSecret; // optional
        private string _accessToken;
        private string _refreshToken;
        private DateTimeOffset _expiresAtUtc;

        private readonly ITransport _transport; // raw transport to avoid pipeline recursion

        public OAuth2RefreshingAuthProvider(
            string tokenEndpoint,
            string clientId,
            string clientSecret,
            string initialAccessToken = null,
            string initialRefreshToken = null,
            ITransport transport = null)
        {
            _tokenEndpoint = new Uri(tokenEndpoint);
            _clientId = clientId;
            _clientSecret = clientSecret;
            _accessToken = initialAccessToken ?? "";
            _refreshToken = initialRefreshToken ?? "";
            _transport = transport ?? new UnityWebRequestTransport();
            _expiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(5); // initial grace
        }

        public Task<string> GetAccessTokenAsync(CancellationToken ct)
            => Task.FromResult(_accessToken);

        public async Task<bool> RefreshAsync(CancellationToken ct)
        {
            if (string.IsNullOrEmpty(_refreshToken))
                return false;

            var body = $"grant_type=refresh_token&refresh_token={Uri.EscapeDataString(_refreshToken)}&client_id={Uri.EscapeDataString(_clientId)}";
            if (!string.IsNullOrEmpty(_clientSecret))
                body += $"&client_secret={Uri.EscapeDataString(_clientSecret)}";

            var req = new ApiRequest
            {
                Method = "POST",
                Url = _tokenEndpoint,
                BodyBytes = Encoding.UTF8.GetBytes(body),
                MediaType = "application/x-www-form-urlencoded"
            };

            var resp = await _transport.SendAsync(req, ct).ConfigureAwait(false);
            if (resp.Status != 200 || resp.BodyBytes == null || resp.BodyBytes.Length == 0) return false;

            // minimal JSON parse (no dependency): extract access_token, refresh_token, expires_in
            var json = Encoding.UTF8.GetString(resp.BodyBytes);
            string Get(string key)
            {
                var i = json.IndexOf($"\"{key}\"", StringComparison.Ordinal);
                if (i < 0) return null;
                i = json.IndexOf(':', i) + 1;
                while (i < json.Length && (json[i] == ' ' || json[i] == '\"')) i++;
                var j = i;
                if (key == "expires_in")
                {
                    while (j < json.Length && char.IsDigit(json[j])) j++;
                    return json.Substring(i, j - i);
                }
                while (j < json.Length && json[j] != '\"') j++;
                return json.Substring(i, j - i);
            }

            var at = Get("access_token");
            if (string.IsNullOrEmpty(at)) return false;

            _accessToken = at;
            var rt = Get("refresh_token");
            if (!string.IsNullOrEmpty(rt)) _refreshToken = rt;
            var expStr = Get("expires_in");
            if (int.TryParse(expStr, out var secs)) _expiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(Math.Max(30, secs));

            return true;
        }

        public bool IsExpired() => DateTimeOffset.UtcNow >= _expiresAtUtc;
    }
}
