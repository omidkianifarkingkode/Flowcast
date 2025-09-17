namespace Identity.API.Services;

public sealed record KeyMaterial(
    string Kid,
    string Algorithm,
    string PublicKeyPem,
    string? PrivateKeyPem,
    DateTime NotBeforeUtc,
    DateTime? ExpiresAtUtc);

public interface IKeyStore
{
    Task<KeyMaterial?> GetActiveAsync(CancellationToken ct);
    Task<IReadOnlyList<KeyMaterial>> GetValidationSetAsync(CancellationToken ct);
    Task<KeyMaterial> RotateAsync(string? overrideAlg, DateTime? notBeforeUtc, DateTime? expiresAtUtc, CancellationToken ct);
}
