// Runtime/Core/Auth/IAuthProvider.cs
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Flowcast.Core.Auth
{
    public interface IAuthProvider
    {
        /// Returns an access token if available (null/empty if none).
        Task<string> GetAccessTokenAsync(CancellationToken ct);
    }

    /// KISS provider: you set the token manually. No refresh logic yet.
    public sealed class SimpleBearerAuthProvider : IAuthProvider
    {
        private string _token;
        public void SetToken(string token) => _token = token ?? string.Empty;
        public Task<string> GetAccessTokenAsync(CancellationToken ct) =>
            Task.FromResult(_token);
    }

    public sealed class BasicAuthProvider : IAuthProvider
    {
        private readonly string _header; // "Basic xxxxxx"
        public BasicAuthProvider(string username, string password)
        {
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
            _header = "Basic " + token;
        }
        public Task<string> GetAccessTokenAsync(CancellationToken ct) => Task.FromResult<string>(null);
        public string AuthorizationHeader => _header;
    }

    public sealed class ApiKeyAuthProvider : IAuthProvider
    {
        public string HeaderName { get; }
        public string HeaderValue { get; }
        public ApiKeyAuthProvider(string headerName, string headerValue) { HeaderName = headerName; HeaderValue = headerValue; }
        public Task<string> GetAccessTokenAsync(CancellationToken ct) => Task.FromResult<string>(null);
    }
}
