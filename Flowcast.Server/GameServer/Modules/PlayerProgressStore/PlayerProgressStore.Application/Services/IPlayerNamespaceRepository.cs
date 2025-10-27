using System;
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
/// Computes stable content hashes (e.g., "sha256:abcd…") from raw document bytes.
/// </summary>
public interface IContentHashService
{
    DocHash Compute(ReadOnlySpan<byte> document);
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
    /// Returns merged document bytes owned by the resolver.
    /// </summary>
    Result<byte[]> Merge(byte[] serverDocument, byte[] clientDocument);
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