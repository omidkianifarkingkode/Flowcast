using System;
using System.IO;
using System.Text.Json;

namespace Flowcast.CodeGenerator;

/// <summary>
/// Provides helpers for loading <see cref="RestSettings"/> documents.
/// </summary>
public static class RestSettingsLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    /// <summary>
    /// Loads a settings document from the specified file path.
    /// </summary>
    /// <param name="path">The path to the configuration file.</param>
    /// <returns>The loaded settings document.</returns>
    public static RestSettingsDocument Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("The settings path cannot be null or whitespace.", nameof(path));
        }

        var absolutePath = Path.GetFullPath(path);
        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException($"Could not find REST settings at '{absolutePath}'.", absolutePath);
        }

        using var stream = File.OpenRead(absolutePath);
        var settings = JsonSerializer.Deserialize<RestSettings>(stream, Options) ?? new RestSettings();
        Normalize(settings);
        return new RestSettingsDocument(settings, absolutePath);
    }

    private static void Normalize(RestSettings settings)
    {
        if (settings.Documents.Count == 0)
        {
            throw new InvalidOperationException("The REST settings file does not reference any OpenAPI documents.");
        }
    }
}
