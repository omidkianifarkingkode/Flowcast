using Identity.API.Options;
using Identity.API.Persistence.Entities;
using Identity.API.Services;
using Identity.API.Services.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Identity.API.Infrastructures;

public sealed class DbKeyStore : IKeyStore
{
    private readonly IKeyRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly IMemoryCache _cache;
    private readonly IdentityOptions _opt;

    private const string ActiveCacheKey = "keys:active";
    private const string ValidationCacheKey = "keys:validation";

    public DbKeyStore(IKeyRepository repo, IUnitOfWork uow, IMemoryCache cache, IOptions<IdentityOptions> opt)
    {
        _repo = repo;
        _uow = uow;
        _cache = cache;
        _opt = opt.Value;
    }

    public async Task<KeyMaterial?> GetActiveAsync(CancellationToken ct)
    {
        if (_cache.TryGetValue(ActiveCacheKey, out KeyMaterial? cached))
            return cached;

        var row = await _repo.GetActiveAsync(ct);
        if (row is null) return null;

        var material = ToMaterial(row);
        _cache.Set(ActiveCacheKey, material, TimeSpan.FromMinutes(1));
        return material;
    }

    public async Task<IReadOnlyList<KeyMaterial>> GetValidationSetAsync(CancellationToken ct)
    {
        if (_cache.TryGetValue(ValidationCacheKey, out IReadOnlyList<KeyMaterial>? cached))
            return cached;

        var rows = await _repo.GetValidForValidationAsync(DateTime.UtcNow, ct);
        var list = rows.Select(ToMaterial).ToList().AsReadOnly();
        _cache.Set(ValidationCacheKey, list, TimeSpan.FromMinutes(5));
        return list;
    }

    public async Task<KeyMaterial> RotateAsync(string? overrideAlg, DateTime? notBeforeUtc, DateTime? expiresAtUtc, CancellationToken ct)
    {
        var alg = string.IsNullOrWhiteSpace(overrideAlg) ? _opt.TokenOptions.Algorithm : overrideAlg!;
        var (pub, priv) = CryptoPem.Generate(alg);
        var kid = CryptoPem.ComputeKidFromPublicPem(pub);
        var now = notBeforeUtc ?? DateTime.UtcNow;

        await _repo.DeactivateAllAsync(ct);
        var key = SigningKey.CreateNew(
            algorithm: alg,
            kid: kid,
            publicPem: pub,
            privatePem: priv,
            nowUtc: now,
            expiresAtUtc: expiresAtUtc,
            isActive: true);

        await _repo.AddAsync(key, ct);
        await _uow.SaveChangesAsync(ct);

        // bust caches
        _cache.Remove(ActiveCacheKey);
        _cache.Remove(ValidationCacheKey);

        return ToMaterial(key);
    }

    private static KeyMaterial ToMaterial(SigningKey k) =>
        new(k.KeyId, k.Algorithm, k.PublicKeyPem, k.PrivateKeyPem, k.NotBeforeUtc, k.ExpiresAtUtc);
}
