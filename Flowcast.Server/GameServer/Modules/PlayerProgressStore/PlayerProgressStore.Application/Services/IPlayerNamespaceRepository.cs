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