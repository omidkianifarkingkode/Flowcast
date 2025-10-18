using PlayerProgressStore.Domain;
using SharedKernel;

namespace PlayerProgressStore.Application.Services;

/// <summary>
/// Persistence access for PlayerNamespace aggregates.
/// No serialization details here; infra implements this.
/// </summary>
public interface IPlayerNamespaceRepository
{
    /// <summary>Load namespaces for a player. If namespaces is null or empty, returns all.</summary>
    Task<Result<IReadOnlyList<PlayerNamespace>>> LoadAsync(
        string playerId,
        IReadOnlyCollection<string>? namespaces,
        CancellationToken ct);

    /// <summary>
    /// Upsert a batch atomically. Infra must guarantee atomicity and idempotency at this boundary.
    /// Returns authoritative copies as stored.
    /// </summary>
    Task<Result<IReadOnlyList<PlayerNamespace>>> UpsertAtomicAsync(
        string playerId,
        IReadOnlyList<PlayerNamespace> updated,
        CancellationToken ct);
}

/// <summary>
/// Computes stable content hashes (e.g., "sha256:abcd…") from canonical JSON text.
/// </summary>
public interface IContentHashService
{
    DocHash Compute(string canonicalJson);
}

/// <summary>
/// Produces a canonical JSON string (stable key ordering, trimmed whitespace) for hashing and caching.
/// </summary>
public interface ICanonicalJsonService
{
    /// <summary>Input may be any JSON text; output must be stable for semantically-equal inputs.</summary>
    Result<string> Canonicalize(string json);
}


/// <summary>
/// Generates new server-owned version tokens (opaque, monotonic per namespace).
/// </summary>
public interface IVersionTokenService
{
    VersionToken Next(VersionToken current);
}

/// <summary>
/// Equal-progress merge strategy per namespace.
/// Implementation lives outside the Domain.
/// </summary>
public interface IMergeResolver
{
    /// <summary>The logical namespace this resolver handles (e.g., "playerStats").</summary>
    string Namespace { get; }

    /// <summary>
    /// Merge current server document with client document (equal progress).
    /// Returns merged JSON string (canonical or raw; hashing happens separately).
    /// </summary>
    Result<string> Merge(string serverJson, string clientJson);
}

/// <summary>
/// Looks up a resolver for a given namespace; returns a default pass-through if none registered.
/// </summary>
public interface IMergeResolverRegistry
{
    IMergeResolver Get(string @namespace);
}

/// <summary>
/// Central place to validate namespace naming/length/character rules at the app boundary.
/// </summary>
public interface INamespaceValidationPolicy
{
    Result Validate(string @namespace);
}