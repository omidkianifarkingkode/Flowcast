// Runtime/Core/Auth/IRefreshingAuthProvider.cs
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Flowcast.Core.Auth
{
    /// Extends IAuthProvider with a refresh capability.
    public interface IRefreshingAuthProvider : IAuthProvider
    {
        /// Try to refresh the token (single-flight friendly at behavior layer).
        /// Returns true if a new usable access token is now available.
        Task<bool> RefreshAsync(CancellationToken ct);
    }

    /// Minimal provider:
    /// - You set an initial token (optional)
    /// - You supply a refresh function that returns a NEW token string
    public sealed class SimpleRefreshingAuthProvider : IRefreshingAuthProvider
    {
        private readonly Func<CancellationToken, Task<string>> _refreshFunc;
        private string _token;

        public SimpleRefreshingAuthProvider(Func<CancellationToken, Task<string>> refreshFunc, string initialToken = "")
        {
            _refreshFunc = refreshFunc ?? throw new ArgumentNullException(nameof(refreshFunc));
            _token = initialToken ?? string.Empty;
        }

        public void SetToken(string token) => _token = token ?? string.Empty;

        public Task<string> GetAccessTokenAsync(CancellationToken ct)
            => Task.FromResult(_token);

        public async Task<bool> RefreshAsync(CancellationToken ct)
        {
            var newToken = await _refreshFunc(ct).ConfigureAwait(false);
            if (string.IsNullOrEmpty(newToken)) return false;
            _token = newToken;
            return true;
        }
    }
}
