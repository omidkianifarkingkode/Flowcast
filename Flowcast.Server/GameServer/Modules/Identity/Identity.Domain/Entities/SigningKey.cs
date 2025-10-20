namespace Identity.Domain.Entities;

public sealed class SigningKey
{
    public Guid Id { get; private set; }
    public string KeyId { get; private set; } = default!;          // kid
    public string Algorithm { get; private set; } = "RS256";        // RS256/ES256/...
    public string PublicKeyPem { get; private set; } = default!;
    public string? PrivateKeyPem { get; private set; }              // null on validator-only nodes
    public DateTime NotBeforeUtc { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    private SigningKey() { } // ORM

    public static SigningKey CreateNew(
        string algorithm,
        string kid,
        string publicPem,
        string? privatePem,
        DateTime nowUtc,
        DateTime? expiresAtUtc = null,
        bool isActive = false)
        => new()
        {
            Id = Guid.NewGuid(),
            KeyId = kid,
            Algorithm = algorithm,
            PublicKeyPem = publicPem,
            PrivateKeyPem = privatePem,
            NotBeforeUtc = nowUtc,
            ExpiresAtUtc = expiresAtUtc,
            IsActive = isActive
        };

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
