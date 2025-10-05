namespace PlayerProgressStore.Domain;

/// <summary>
/// Outcome of comparing incoming vs current progress for a namespace.
/// </summary>
public enum MergeDecision
{
    /// <summary>Incoming progress is strictly greater → client overwrites entire document.</summary>
    ClientWinsOverwrite = 1,

    /// <summary>Incoming progress equals current → safe to run an equal-progress merge (outside domain).</summary>
    EqualProgress = 2,

    /// <summary>Incoming progress is behind → keep server document (reject incoming).</summary>
    ServerKeeps = 3
}
