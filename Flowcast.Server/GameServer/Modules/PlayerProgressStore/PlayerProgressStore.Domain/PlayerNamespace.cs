using SharedKernel;
using System;

namespace PlayerProgressStore.Domain;

/// <summary>
/// Aggregate representing one namespaced document of a player's profile.
/// Minimal domain: identity + state + basic invariants + progress comparison.
/// </summary>
public sealed class PlayerNamespace
{
    public string PlayerId { get; private set; }
    public string Namespace { get; private set; } = default!;
    public VersionToken Version { get; private set; } = VersionToken.None;
    public ProgressScore Progress { get; private set; } = ProgressScore.Zero;
    public byte[] Document { get; private set; } = Array.Empty<byte>();
    public DocumentMetadata Metadata { get; private set; } = DocumentMetadata.JsonUtf8();
    public DocHash Hash { get; private set; } = DocHash.Empty;   // server-computed content hash
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    private PlayerNamespace() { }

    private PlayerNamespace(
        string playerId,
        string @namespace,
        VersionToken version,
        ProgressScore progress,
        byte[] document,
        DocumentMetadata metadata,
        DocHash hash,
        DateTimeOffset updatedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(@namespace))
            throw new ArgumentException("Namespace cannot be null or whitespace.", nameof(@namespace));

        PlayerId = playerId;
        Namespace = @namespace;
        Version = version;
        Progress = progress;
        Document = OwnDocument(document);
        Metadata = NormalizeMetadata(metadata);
        Hash = hash;
        UpdatedAtUtc = updatedAtUtc;
    }

    /// <summary>
    /// Factory to create a new aggregate instance. Validates inputs and returns Result.
    /// </summary>
    public static Result<PlayerNamespace> Create(
        string playerId,
        string @namespace,
        VersionToken version,
        ProgressScore progress,
        byte[] document,
        DocumentMetadata metadata,
        DocHash hash,
        DateTimeOffset updatedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(playerId))
            return Result.Failure<PlayerNamespace>(PlayerNamespaceErrors.EmptyPlayerId);

        if (string.IsNullOrWhiteSpace(@namespace))
            return Result.Failure<PlayerNamespace>(PlayerNamespaceErrors.NamespaceRequired);

        if (progress.Value < 0)
            return Result.Failure<PlayerNamespace>(PlayerNamespaceErrors.InvalidProgress);

        var agg = new PlayerNamespace(
            playerId,
            @namespace,
            version,
            progress,
            document,
            metadata,
            hash,
            updatedAtUtc);

        return Result.Success(agg);
    }

    /// <summary>
    /// Compare incoming progress to decide high-level merge policy.
    /// - ClientWinsOverwrite: accept incoming document wholly.
    /// - EqualProgress: caller may perform field-level merge outside the domain layer.
    /// - ServerKeeps: reject/ignore incoming state.
    /// </summary>
    public MergeDecision Decide(ProgressScore incomingProgress)
    {
        var cmp = incomingProgress.CompareTo(Progress);
        if (cmp > 0) return MergeDecision.ClientWinsOverwrite;
        if (cmp == 0) return MergeDecision.EqualProgress;
        return MergeDecision.ServerKeeps;
    }

    /// <summary>
    /// Return a copy with authoritative state replaced (used after overwrite or after an external merge).
    /// </summary>
    public Result<PlayerNamespace> WithReplaced(
        byte[] newDocument,
        DocumentMetadata metadata,
        ProgressScore newProgress,
        VersionToken newVersion,
        DocHash newHash,
        DateTimeOffset nowUtc)
    {
        if (newProgress.Value < 0)
            return Result.Failure<PlayerNamespace>(PlayerNamespaceErrors.InvalidProgress);

        var updated = new PlayerNamespace(
            PlayerId,
            Namespace,
            newVersion,
            newProgress,
            newDocument,
            metadata,
            newHash,
            nowUtc);

        return Result.Success(updated);
    }

    /// <summary>
    /// Convenience helper: attempt full overwrite if client is ahead.
    /// Returns (updated aggregate) or ClientBehind error.
    /// </summary>
    public Result<PlayerNamespace> TryOverwriteIfClientAhead(
        ProgressScore incomingProgress,
        byte[] incomingDocument,
        DocumentMetadata metadata,
        VersionToken newVersion,
        DocHash newHash,
        DateTimeOffset nowUtc)
    {
        var decision = Decide(incomingProgress);
        if (decision == MergeDecision.ServerKeeps)
            return Result.Failure<PlayerNamespace>(PlayerNamespaceErrors.ClientBehind);

        if (decision == MergeDecision.EqualProgress)
            return Result.Failure<PlayerNamespace>(Error.Conflict(
                "progress.equal_requires_merge",
                "Equal progress requires merge; call TryMergeIfEqualProgress."));

        return WithReplaced(incomingDocument, metadata, incomingProgress, newVersion, newHash, nowUtc);
    }

    /// <summary>
    /// Equal-progress path: the caller supplies the externally-merged document + derived hash/progress/version.
    /// </summary>
    public Result<PlayerNamespace> TryMergeIfEqualProgress(
        ProgressScore incomingProgress,
        byte[] mergedDocument,
        DocumentMetadata metadata,
        VersionToken newVersion,
        DocHash newHash,
        DateTimeOffset nowUtc)
    {
        if (Decide(incomingProgress) != MergeDecision.EqualProgress)
            return Result.Failure<PlayerNamespace>(PlayerNamespaceErrors.UnknownDecision);

        // Typically progress stays equal after an equal-merge, but we accept caller-provided progress for flexibility
        return WithReplaced(mergedDocument, metadata, incomingProgress, newVersion, newHash, nowUtc);
    }

    private static byte[] OwnDocument(byte[]? document)
    {
        if (document is null || document.Length == 0)
            return Array.Empty<byte>();

        var copy = new byte[document.Length];
        Buffer.BlockCopy(document, 0, copy, 0, document.Length);
        return copy;
    }

    private static DocumentMetadata NormalizeMetadata(DocumentMetadata metadata)
    {
        if (metadata == default)
            return DocumentMetadata.JsonUtf8();

        return metadata.Normalize();
    }
}
