namespace Identity.Contracts.V1;

public static class Dtos
{
    public record IdentitySummary(
            Guid IdentityId,
            string Provider, // Google/Facebook/Apple
            string SubjectMasked,
            bool LoginAllowed,
            DateTime CreatedAtUtc,
            DateTime? LastSeenAtUtc,
            Dictionary<string, string>? LastMeta 
        );
}
