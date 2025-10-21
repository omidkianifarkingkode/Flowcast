using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using PlayerProgressStore.Application.Services;
using PlayerProgressStore.Domain;
using Shared.Application.Messaging;
using Shared.Application.Services;
using SharedKernel;

namespace PlayerProgressStore.Application.Commands;

public sealed record SaveProfileCommand(
    string PlayerId,
    IReadOnlyCollection<NamespaceWriteDto> Namespaces
) : ICommand<PlayerNamespace[]>;

public readonly record struct NamespaceWriteDto(
    string Namespace,
    JsonElement Document,
    long Progress,
    string? ClientVersion,
    string? ClientHash
);

public sealed class SaveProfileCommandHandler(
    IPlayerNamespaceRepository repo,
    INamespaceValidationPolicy namespacePolicy,
    ICanonicalJsonService canonicalJson,
    IContentHashService hashes,
    IVersionTokenService versions,
    IMergeResolverRegistry mergeResolvers,
    IDateTimeProvider clock,
    [FromKeyedServices("playerprogress")] IUnitOfWork unitOfWork)
        : ICommandHandler<SaveProfileCommand, PlayerNamespace[]>
{
    //for each write in batch:
    //  validate(write)
    //load current docs for all namespaces in batch
    //for each write:
    //  canonical = canonicalize(write.Document)
    //  hash = sha256(canonical)
    //  if no existing -> CREATE(canonical, hash)
    //  else:
    //    case Decide(write.Progress vs existing.Progress) :
    //      > : OVERWRITE(canonical, hash)
    //      = : MERGE(existing.Document, canonical) → canonicalize → hash → REPLACE
    //      < : REJECT(fail batch)
    //transaction {
    //  upsert all updated namespaces atomically
    //}
    //return authoritative copies
    public async Task<Result<PlayerNamespace[]>> Handle(
        SaveProfileCommand command,
        CancellationToken ct)
    {
        // 0) Empty batch → nothing to do.
        if (command.Namespaces is null || command.Namespaces.Count == 0)
            return Result.Failure<PlayerNamespace[]>(Error.Validation("save.empty_batch",
                "SaveProfile request must contain at least one namespace to save."));

        // 1) Validate namespaces up-front (fast fail).
        var namespaceValidation = ValidateNamespaces(command.Namespaces);
        if (namespaceValidation.IsFailure)
            return Result.Failure<PlayerNamespace[]>(namespaceValidation.Error);

        // 2) Load current server state only for the touched namespaces.
        var targetNames = command.Namespaces.Select(w => w.Namespace).Distinct().ToArray();
        var currentMapResult = await LoadCurrentAsMapAsync(command.PlayerId, targetNames, ct);
        if (currentMapResult.IsFailure)
            return Result.Failure<PlayerNamespace[]>(currentMapResult.Error);

        var currentByNameSpace = currentMapResult.Value;
        var now = clock.UtcNowOffset;

        // 3) Build updated aggregates (pure per-namespace transformation).
        var updatedListRes = await BuildUpdatedListAsync(
            playerId: command.PlayerId,
            writes: command.Namespaces,
            currentByNamespaces: currentByNameSpace,
            nowUtc: now,
            ct: ct);

        if (updatedListRes.IsFailure)
            return Result.Failure<PlayerNamespace[]>(updatedListRes.Error);

        var updatedList = updatedListRes.Value;

        // 4) Persist atomically and return authoritative copies from the repository.
        return await PersistAtomicallyAsync(command.PlayerId, updatedList, ct);
    }

    // ----------------------------
    // Extracted helpers
    // ----------------------------

    private Result ValidateNamespaces(IEnumerable<NamespaceWriteDto> writes)
    {
        foreach (var w in writes)
        {
            var namespaceCheck = namespacePolicy.Validate(w.Namespace);
            if (namespaceCheck.IsFailure)
                return namespaceCheck;
        }
        return Result.Success();
    }

    private async Task<Result<Dictionary<string, PlayerNamespace>>> LoadCurrentAsMapAsync(string playerId,
        IReadOnlyCollection<string> namespaces, CancellationToken ct)
    {
        var currentResult = await repo.LoadAsync(playerId, namespaces, ct);
        if (currentResult.IsFailure)
            return Result.Failure<Dictionary<string, PlayerNamespace>>(currentResult.Error);

        var map = currentResult.Value.ToDictionary(x => x.Namespace, StringComparer.Ordinal);
        return Result.Success(map);
    }

    private async Task<Result<List<PlayerNamespace>>> BuildUpdatedListAsync(string playerId,
        IReadOnlyCollection<NamespaceWriteDto> writes, Dictionary<string, PlayerNamespace> currentByNamespaces,
        DateTimeOffset nowUtc, CancellationToken ct)
    {
        var updated = new List<PlayerNamespace>(writes.Count);

        foreach (var write in writes)
        {
            var existing = currentByNamespaces.TryGetValue(write.Namespace, out var ns) ? ns : null;

            var buildResult = await BuildUpdatedAggregateAsync(
                playerId: playerId,
                write: write,
                existingOrNull: existing,
                nowUtc: nowUtc,
                ct: ct);

            if (buildResult.IsFailure)
                return Result.Failure<List<PlayerNamespace>>(buildResult.Error);

            updated.Add(buildResult.Value);
        }

        return Result.Success(updated);
    }

    private async Task<Result<PlayerNamespace[]>> PersistAtomicallyAsync(string playerId,
        IReadOnlyList<PlayerNamespace> updated, CancellationToken ct)
    {
        return await unitOfWork.ExecuteAsync(async token =>
        {
            var upserted = await repo.UpsertAtomicAsync(playerId, updated, token);
            if (upserted.IsFailure)
                return Result.Failure<PlayerNamespace[]>(upserted.Error);

            // Return authoritative copies from the repository
            return Result.Success(upserted.Value.ToArray());
        }, ct);
    }

    // ----------------------------
    // Core per-namespace decision
    // ----------------------------

    private async Task<Result<PlayerNamespace>> BuildUpdatedAggregateAsync(string playerId, NamespaceWriteDto write,
        PlayerNamespace? existingOrNull, DateTimeOffset nowUtc, CancellationToken ct)
    {
        // A) Normalize/validate client inputs (progress, version).
        var normalizeResult = NormalizeInputs(write);
        if (normalizeResult.IsFailure)
            return Result.Failure<PlayerNamespace>(normalizeResult.Error);

        (ProgressScore incomingProgress, VersionToken clientVersion) = normalizeResult.Value;

        // B) Canonicalize and hash the incoming document once.
        var canonHashRes = CanonicalizeAndHash(write.Document);
        if (canonHashRes.IsFailure)
            return Result.Failure<PlayerNamespace>(canonHashRes.Error);

        var (canonical, newHash) = canonHashRes.Value;

        // C) CREATE path (no existing document).
        if (existingOrNull is null)
            return CreateNewAggregate(playerId, write.Namespace, incomingProgress, canonical, newHash, nowUtc);

        // D) Existing doc → decide policy based on progress.
        var existing = existingOrNull;
        var decision = existing.Decide(incomingProgress);

        return decision switch
        {
            MergeDecision.ClientWinsOverwrite => OverwriteAggregate(existing, incomingProgress, canonical, newHash, nowUtc),
            MergeDecision.EqualProgress => await MergeEqualProgressAsync(existing, incomingProgress, canonical, nowUtc, ct),
            MergeDecision.ServerKeeps => Result.Failure<PlayerNamespace>(PlayerNamespaceErrors.ClientBehind),
            _ => Result.Failure<PlayerNamespace>(PlayerNamespaceErrors.UnknownDecision),
        };
    }

    // ----------------------------
    // Small focused operations
    // ----------------------------

    /// <summary>Validate/normalize progress & client version.</summary>
    private Result<(ProgressScore Progress, VersionToken ClientVersion)> NormalizeInputs(NamespaceWriteDto write)
    {
        var clientVersion = string.IsNullOrWhiteSpace(write.ClientVersion)
            ? VersionToken.None
            : new VersionToken(write.ClientVersion!.Trim());

        var progressResult = ProgressScore.From(write.Progress);
        if (progressResult.IsFailure)
            return Result.Failure<(ProgressScore, VersionToken)>(progressResult.Error);

        return Result.Success((progressResult.Value, clientVersion));
    }

    /// <summary>Canonicalize JSON string and compute content hash.</summary>
    private Result<(string Canonical, DocHash Hash)> CanonicalizeAndHash(JsonElement json)
    {
        var raw = json.ValueKind switch
        {
            JsonValueKind.Undefined => "{}",
            JsonValueKind.Null => "{}",
            _ => json.GetRawText(),
        };

        if (string.IsNullOrWhiteSpace(raw))
            raw = "{}";

        var canonicalResult = canonicalJson.Canonicalize(raw);
        if (canonicalResult.IsFailure)
            return Result.Failure<(string, DocHash)>(canonicalResult.Error);

        var canonical = canonicalResult.Value;
        var hash = hashes.Compute(canonical);
        return Result.Success((canonical, hash));
    }

    /// <summary>Create a new aggregate (first write).</summary>
    private Result<PlayerNamespace> CreateNewAggregate(string playerId, string @namespace, ProgressScore progress,
        string canonicalDoc, DocHash hash, DateTimeOffset nowUtc)
    {
        var newVersion = versions.Next(VersionToken.None); // first server version
        return PlayerNamespace.Create(playerId, @namespace, newVersion, progress, canonicalDoc, hash, nowUtc);
    }

    /// <summary>Overwrite existing aggregate when client is ahead.</summary>
    private Result<PlayerNamespace> OverwriteAggregate(PlayerNamespace existing, ProgressScore progress,
        string canonicalDoc, DocHash hash, DateTimeOffset nowUtc)
    {
        if (existing.Hash.Equals(hash))
            return Result.Success(existing);

        var nextVersion = versions.Next(existing.Version);
        return existing.WithReplaced(canonicalDoc, progress, nextVersion, hash, nowUtc);
    }

    /// <summary>Equal-progress merge using namespace-specific resolver.</summary>
    private Task<Result<PlayerNamespace>> MergeEqualProgressAsync(PlayerNamespace existing,
        ProgressScore incomingProgress, string clientCanonical, DateTimeOffset nowUtc, CancellationToken ct)
    {
        // 1) Resolve merge (server vs client).
        var resolver = mergeResolvers.Get(existing.Namespace);
        var mergedRes = resolver.Merge(serverJson: existing.Document, clientJson: clientCanonical);
        if (mergedRes.IsFailure)
            return Task.FromResult(Result.Failure<PlayerNamespace>(mergedRes.Error));

        // 2) Canonicalize merged output and recompute hash.
        var mergedCanonRes = canonicalJson.Canonicalize(mergedRes.Value);
        if (mergedCanonRes.IsFailure)
            return Task.FromResult(Result.Failure<PlayerNamespace>(mergedCanonRes.Error));

        var mergedCanonical = mergedCanonRes.Value;
        var mergedHash = hashes.Compute(mergedCanonical);

        // 3) Replace with merged doc and bump version.
        var nextVersion = versions.Next(existing.Version);
        return Task.FromResult(existing.TryMergeIfEqualProgress(
            incomingProgress,
            mergedCanonical,
            nextVersion,
            mergedHash,
            nowUtc));
    }
}
