using SharedKernel;

namespace Identity.Application.Services
{
    public sealed record GooglePlayerIdentity(string PlayerId);
    //string PlayerId,
    //string? Subject,
    //string? Email);


    public interface IGooglePlayGamesVerifier
    {
        Task<Result<GooglePlayerIdentity>> VerifyAsync(
            string serverAuthCode,
            CancellationToken ct
        );
    }

}
