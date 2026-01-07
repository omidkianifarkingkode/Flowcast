using SharedKernel;

namespace Identity.Application.Services;

public interface IGooglePlayGamesVerifier
{
    Task<Result<string>> VerifyAsync(string serverAuthCode, CancellationToken ct = default);
}