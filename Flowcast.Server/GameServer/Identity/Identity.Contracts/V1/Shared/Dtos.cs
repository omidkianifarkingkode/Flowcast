namespace Identity.Contracts.V1.Shared;

public static class Dtos
{
    public record IdentitySummary(
            Guid IdentityId,
            string Provider, // Google/Facebook/Apple
            string SubjectMasked,
            bool LoginAllowed,
            DateTimeOffset CreatedAtUtc,
            DateTimeOffset? LastSeenAtUtc,
            IReadOnlyDictionary<string, string>? LastMeta 
        );
}
