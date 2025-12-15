using SharedKernel;

namespace Identity.Application.Services
{
    public sealed record GooglePlayerIdentity(
    string PlayerId,
    string? GoogleSub,
    string? Email);

    public interface IGooglePlayGamesVerifier
    {
        Task<Result<GooglePlayerIdentity>> VerifyAsync(
            string serverAuthCode,
            CancellationToken ct
        );
    }

}
