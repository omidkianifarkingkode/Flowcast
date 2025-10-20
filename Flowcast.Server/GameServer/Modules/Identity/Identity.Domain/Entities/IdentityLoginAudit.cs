using System.Globalization;

namespace Identity.Domain.Entities;

public sealed class IdentityLoginAudit
{
    public long Id { get; private set; }               // db-generated (kept as long)
    public Guid IdentityId { get; private set; }
    public Guid AccountId { get; private set; }
    public DateTime LoginAtUtc { get; private set; }

    public string? Ip { get; private set; }
    public string? Region { get; private set; }
    public string? UserAgent { get; private set; }
    public string? DeviceOs { get; private set; }
    public string? DeviceModel { get; private set; }
    public string? DeviceLanguage { get; private set; }
    public string? AppVersion { get; private set; }
    public int? TzOffsetMinutes { get; private set; }
    public DateTime? ClientTimeUtc { get; private set; }

    private IdentityLoginAudit() { }

    public static IdentityLoginAudit FromMeta(Guid identityId, Guid accountId, DateTime nowUtc, Dictionary<string, string>? meta)
    {
        string? Get(string key)
        {
            if (meta is null) return null;
            return meta.TryGetValue(key, out var v) ? v : null;
        }

        DateTime? GetDate(string key)
        {
            var s = Get(key);
            if (s is null) return null;
            if (DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dto))
                return dto.UtcDateTime;
            return null;
        }

        int? GetInt(string key) => int.TryParse(Get(key), out var i) ? i : null;

        return new IdentityLoginAudit
        {
            IdentityId = identityId,
            AccountId = accountId,
            LoginAtUtc = nowUtc,
            Ip = Get("ip"),
            Region = Get("region"),
            UserAgent = Get("ua"),
            DeviceOs = Get("os"),
            DeviceModel = Get("model"),
            DeviceLanguage = Get("lang"),
            AppVersion = Get("app"),
            TzOffsetMinutes = GetInt("tz"),
            ClientTimeUtc = GetDate("clientUtc")
        };
    }
}
