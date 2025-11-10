using System.Collections.Generic;

namespace Flowcast.CodeGenerator;

/// <summary>
/// Represents the outcome of a generation run.
/// </summary>
public sealed class GenerationResult
{
    public GenerationResult(IReadOnlyList<GeneratedFile> files, IReadOnlyList<string> warnings)
    {
        Files = files;
        Warnings = warnings;
    }

    /// <summary>
    /// Gets the generated files.
    /// </summary>
    public IReadOnlyList<GeneratedFile> Files { get; }

    /// <summary>
    /// Gets warnings emitted during generation.
    /// </summary>
    public IReadOnlyList<string> Warnings { get; }
}
