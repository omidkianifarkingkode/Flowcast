using Identity.Domain.Shared;

namespace Identity.Domain.Entities;

public sealed class IdentityEntity
{
    public Guid IdentityId { get; private set; }
    public Guid AccountId { get; private set; }

    public IdentityProvider Provider { get; private set; }
    public string Subject { get; private set; } = string.Empty; // deviceId or provider-sub

    public bool LoginAllowed { get; private set; } // device gets disabled after link
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? LastSeenAtUtc { get; private set; }
    public Dictionary<string, string>? LastMeta { get; private set; }

    private IdentityEntity() { } // ORM

    public static IdentityEntity Create(Guid id, Guid accountId, IdentityProvider provider, string subject, DateTime nowUtc, bool loginAllowed = true)
        => new()
        {
            IdentityId = id,
            AccountId = accountId,
            Provider = provider,
            Subject = subject,
            LoginAllowed = loginAllowed,
            CreatedAtUtc = nowUtc
        };

    public static IdentityEntity Create(Guid accountId, IdentityProvider provider, string subject, DateTime nowUtc) =>
        Create(Guid.NewGuid(), accountId, provider, subject, nowUtc, true);

    public void DisableLogin() { LoginAllowed = false; }
    public void TouchSeen(DateTime nowUtc, Dictionary<string, string>? meta) { LastSeenAtUtc = nowUtc; LastMeta = meta; }
}
