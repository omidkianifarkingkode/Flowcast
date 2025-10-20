using Identity.Domain.Shared;
using SharedKernel;

namespace Identity.Domain.Entities;

public sealed class Account
{
    private readonly List<IdentityEntity> _identities = new();

    public Guid AccountId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public string? DisplayName { get; private set; }
    public DateTime? LastLoginAtUtc { get; private set; }
    public string? LastLoginRegion { get; private set; } // ISO-2 like “SG”

    public IReadOnlyCollection<IdentityEntity> Identities => _identities.AsReadOnly();

    private Account() { } // ORM

    public static Account CreateNew(DateTime nowUtc) =>
        new() { AccountId = Guid.NewGuid(), CreatedAtUtc = nowUtc };

    public static Account Create(Guid id, DateTime nowUtc) =>
        new() { AccountId = id, CreatedAtUtc = nowUtc };

    public void SetDisplayName(string name) => DisplayName = name;
    public void ClearDisplayName() => DisplayName = null;

    public void TouchLastLogin(DateTime nowUtc, string? region)
    {
        LastLoginAtUtc = nowUtc;
        LastLoginRegion = region;
    }

    public Result<IdentityEntity> AddOrTouchDeviceIdentity(string deviceId, DateTime nowUtc, Dictionary<string, string>? meta)
    {
        deviceId = deviceId.Trim();
        if (string.IsNullOrWhiteSpace(deviceId) || deviceId.Length > 128)
            return Result.Failure<IdentityEntity>(DomainErrors.InvalidDeviceId);

        var existing = _identities.FirstOrDefault(i =>
            i.Provider == IdentityProvider.Device && i.Subject == deviceId);

        if (existing is not null)
        {
            if (!existing.LoginAllowed)
                return Result.Failure<IdentityEntity>(DomainErrors.DeviceLoginDisabled);

            existing.TouchSeen(nowUtc, meta);
            return existing;
        }

        var id = IdentityEntity.Create(Guid.NewGuid(), AccountId, IdentityProvider.Device, deviceId, nowUtc, loginAllowed: true);
        id.TouchSeen(nowUtc, meta);
        _identities.Add(id);
        return id;
    }

    public Result<IdentityEntity> LinkProvider(IdentityProvider provider, string subject, DateTime nowUtc)
    {
        if (provider == IdentityProvider.Device)
            return Result.Failure<IdentityEntity>(DomainErrors.InvalidProvider);

        // prevent duplicate (provider, subject)
        if (_identities.Any(x => x.Provider == provider && x.Subject == subject))
            return Result.Failure<IdentityEntity>(DomainErrors.AlreadyLinked);

        var linked = IdentityEntity.Create(Guid.NewGuid(), AccountId, provider, subject, nowUtc, loginAllowed: true);
        _identities.Add(linked);

        // disable all device logins once any non-device is linked
        foreach (var dev in _identities.Where(i => i.Provider == IdentityProvider.Device && i.LoginAllowed))
            dev.DisableLogin();

        return linked;
    }

    public Result EnsureDeviceLoginAllowed(Guid identityId)
    {
        var id = _identities.FirstOrDefault(i => i.IdentityId == identityId);
        if (id is null || id.AccountId != AccountId)
            return Result.Failure(DomainErrors.IdentityNotOwnedByAccount);

        if (id.Provider != IdentityProvider.Device)
            return Result.Failure(DomainErrors.InvalidProvider);

        return id.LoginAllowed ? Result.Success() : Result.Failure(DomainErrors.DeviceLoginDisabled);
    }

    public void AttachIdentities(IEnumerable<IdentityEntity> identities)
    {
        _identities.Clear();
        _identities.AddRange(identities);
    }
}
