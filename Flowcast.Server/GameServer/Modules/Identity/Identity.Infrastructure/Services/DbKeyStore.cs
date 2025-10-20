using Identity.Application.Repositories;
using Identity.Application.Services;
using Identity.Domain.Entities;
using Identity.Infrastructure.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Application.Services;

namespace Identity.Infrastructure.Services;

public sealed class DbKeyStore(IKeyRepository repo, [FromKeyedServices("identity")] IUnitOfWork uow, IMemoryCache cache, IOptions<IdentityOptions> opt) : IKeyStore
{
    private readonly IdentityOptions _opt = opt.Value;

    private const string ActiveCacheKey = "keys:active";
    private const string ValidationCacheKey = "keys:validation";

    public async Task<KeyMaterial?> GetActiveAsync(CancellationToken ct)
    {
        if (cache.TryGetValue(ActiveCacheKey, out KeyMaterial? cached))
            return cached;

        var row = await repo.GetActiveAsync(ct);
        if (row is null) return null;

        var material = ToMaterial(row);
        cache.Set(ActiveCacheKey, material, TimeSpan.FromMinutes(1));
        return material;
    }

    public async Task<IReadOnlyList<KeyMaterial>> GetValidationSetAsync(CancellationToken ct)
    {
        if (cache.TryGetValue(ValidationCacheKey, out IReadOnlyList<KeyMaterial>? cached))
            return cached;

        var rows = await repo.GetValidForValidationAsync(DateTime.UtcNow, ct);
        var list = rows.Select(ToMaterial).ToList().AsReadOnly();
        cache.Set(ValidationCacheKey, list, TimeSpan.FromMinutes(5));
        return list;
    }

    public async Task<KeyMaterial> RotateAsync(string? overrideAlg, DateTime? notBeforeUtc, DateTime? expiresAtUtc, CancellationToken ct)
    {
        var alg = string.IsNullOrWhiteSpace(overrideAlg) ? _opt.TokenOptions.Algorithm : overrideAlg!;
        var (pub, priv) = CryptoPem.Generate(alg);
        var kid = CryptoPem.ComputeKidFromPublicPem(pub);
        var now = notBeforeUtc ?? DateTime.UtcNow;

        await repo.DeactivateAllAsync(ct);
        var key = SigningKey.CreateNew(
            algorithm: alg,
            kid: kid,
            publicPem: pub,
            privatePem: priv,
            nowUtc: now,
            expiresAtUtc: expiresAtUtc,
            isActive: true);

        await repo.AddAsync(key, ct);
        await uow.SaveChangesAsync(ct);

        // bust caches
        cache.Remove(ActiveCacheKey);
        cache.Remove(ValidationCacheKey);

        return ToMaterial(key);
    }

    private static KeyMaterial ToMaterial(SigningKey k) =>
        new(k.KeyId, k.Algorithm, k.PublicKeyPem, k.PrivateKeyPem, k.NotBeforeUtc, k.ExpiresAtUtc);
}
