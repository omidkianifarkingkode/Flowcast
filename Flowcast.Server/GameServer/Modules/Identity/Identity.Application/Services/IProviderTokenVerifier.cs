using SharedKernel;

namespace Identity.Application.Services;

public interface IProviderTokenVerifier
{
    Task<Result<string>> VerifyAsync(string idToken, ProviderVerifyHints? hints, CancellationToken ct);
}

public sealed class ProviderVerifyHints
{
    public string? Nonce { get; init; }          // if you issued one to the client
    public string? HostedDomain { get; init; }   // e.g., "yourcompany.com" to restrict GSuite
}
