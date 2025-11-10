namespace Flowcast.CodeGenerator;

/// <summary>
/// Represents a generated file.
/// </summary>
/// <param name="RelativePath">The path relative to the output directory.</param>
/// <param name="Content">The generated file contents.</param>
public sealed record GeneratedFile(string RelativePath, string Content);
