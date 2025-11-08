// Runtime/Core/Auth/IAuthProvider.cs
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
}
