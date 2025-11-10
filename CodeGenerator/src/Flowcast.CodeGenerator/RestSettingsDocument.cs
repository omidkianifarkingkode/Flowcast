using System;
using System.IO;

namespace Flowcast.CodeGenerator;

/// <summary>
/// Represents a REST settings file together with its resolved location metadata.
/// </summary>
public sealed class RestSettingsDocument
{
    public RestSettingsDocument(RestSettings settings, string sourcePath)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        SourcePath = Path.GetFullPath(sourcePath ?? throw new ArgumentNullException(nameof(sourcePath)));
    }

    /// <summary>
    /// Gets the parsed settings.
    /// </summary>
    public RestSettings Settings { get; }

    /// <summary>
    /// Gets the absolute path to the source file.
    /// </summary>
    public string SourcePath { get; }

    /// <summary>
    /// Gets the directory that contains the settings file.
    /// </summary>
    public string BaseDirectory => Path.GetDirectoryName(SourcePath) ?? Environment.CurrentDirectory;

    /// <summary>
    /// Resolves a path relative to the settings document.
    /// </summary>
    /// <param name="relativePath">The path declared inside the configuration file.</param>
    /// <returns>The absolute path to the referenced file.</returns>
    public string ResolvePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("Relative path cannot be null or whitespace.", nameof(relativePath));
        }

        return Path.GetFullPath(Path.Combine(BaseDirectory, relativePath));
    }
}
