using System;

namespace Flowcast.CodeGenerator;

/// <summary>
/// Represents a generated source file.
/// </summary>
public sealed class GeneratedFile
{
    public GeneratedFile(string relativePath, string content)
    {
        RelativePath = relativePath ?? throw new ArgumentNullException(nameof(relativePath));
        Content = content ?? throw new ArgumentNullException(nameof(content));
    }

    /// <summary>
    /// Gets the relative path of the file inside the output directory.
    /// </summary>
    public string RelativePath { get; }

    /// <summary>
    /// Gets the generated file contents.
    /// </summary>
    public string Content { get; }
}
