using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Flowcast.CodeGenerator;

namespace Flowcast.CodeGenerator.Cli;

internal static class Program
{
    private const string DefaultOutputFolderName = "Generated";

    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        string? settingsPath = null;
        string? outputDirectory = null;
        string? namespaceOverride = null;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            switch (arg)
            {
                case "-o":
                case "--output":
                    if (!TryGetNextValue(args, ref index, out outputDirectory))
                    {
                        Console.Error.WriteLine("Missing value for --output option.");
                        return 1;
                    }

                    break;
                case "-n":
                case "--namespace":
                    if (!TryGetNextValue(args, ref index, out namespaceOverride))
                    {
                        Console.Error.WriteLine("Missing value for --namespace option.");
                        return 1;
                    }

                    break;
                case "-h":
                case "--help":
                    PrintUsage();
                    return 0;
                default:
                    if (arg.StartsWith('-', StringComparison.Ordinal))
                    {
                        Console.Error.WriteLine($"Unknown option '{arg}'.");
                        return 1;
                    }

                    if (settingsPath is not null)
                    {
                        Console.Error.WriteLine("Multiple settings files specified. Only one is supported.");
                        return 1;
                    }

                    settingsPath = arg;
                    break;
            }
        }

        if (settingsPath is null)
        {
            Console.Error.WriteLine("A path to the REST settings file must be provided.");
            return 1;
        }

        try
        {
            var document = RestSettingsLoader.Load(settingsPath);
            var outputPath = DetermineOutputDirectory(document, outputDirectory);
            var targetNamespace = !string.IsNullOrWhiteSpace(namespaceOverride)
                ? namespaceOverride!
                : document.Settings.Namespace ?? "Flowcast.Generated";

            var options = new GenerationOptions
            {
                Namespace = targetNamespace,
                OutputDirectory = outputPath
            };

            var generator = new OpenApiCodeGenerator();
            var result = generator.Generate(document, options);

            Directory.CreateDirectory(options.OutputDirectory);

            foreach (var file in result.Files)
            {
                var path = Path.Combine(options.OutputDirectory, file.RelativePath);
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(path, file.Content, Encoding.UTF8);
                Console.WriteLine($"generated {Path.GetRelativePath(options.OutputDirectory, path)}");
            }

            foreach (var warning in result.Warnings)
            {
                Console.Error.WriteLine($"warning: {warning}");
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static string DetermineOutputDirectory(RestSettingsDocument document, string? overrideDirectory)
    {
        if (!string.IsNullOrWhiteSpace(overrideDirectory))
        {
            return Path.GetFullPath(overrideDirectory);
        }

        if (!string.IsNullOrWhiteSpace(document.Settings.OutputDirectory))
        {
            return Path.GetFullPath(Path.Combine(document.BaseDirectory, document.Settings.OutputDirectory));
        }

        return Path.Combine(document.BaseDirectory, DefaultOutputFolderName);
    }

    private static bool TryGetNextValue(IReadOnlyList<string> args, ref int index, out string? value)
    {
        if (index + 1 >= args.Count)
        {
            value = null;
            return false;
        }

        value = args[index + 1];
        index++;
        return true;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Flowcast Code Generator");
        Console.WriteLine("Usage: flowcast-codegen <settings-file> [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -o|--output <path>      Output directory for generated files.");
        Console.WriteLine("  -n|--namespace <name>   Namespace for generated code.");
        Console.WriteLine("  -h|--help               Display this help text.");
    }
}
