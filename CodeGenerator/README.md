# Flowcast Code Generator

The **Flowcast Code Generator** is a standalone .NET solution that turns OpenAPI specifications referenced from a `rest-settings` configuration into strongly-typed C# stubs. The project lives outside the Unity client so that generated code can be produced without exposing package internals to customers.

## Projects

- `Flowcast.CodeGenerator` – reusable library that understands the configuration format, parses OpenAPI definitions, and produces the generated source files.
- `Flowcast.CodeGenerator.Cli` – command-line front end that can be run inside build automation or from local terminals.

## Configuration

Create a JSON file (for example `Rest/rest-settings.json`) with the following structure:

```json
{
  "documents": [
    "Apis/players.json",
    "Apis/store.json"
  ],
  "namespace": "Flowcast.Generated",
  "outputDirectory": "Assets/Generated/Rest"
}
```

- `documents` – array of OpenAPI file paths relative to the configuration file.
- `namespace` *(optional)* – namespace for the generated REST surface. The tool automatically adds a `.Models` suffix for DTOs.
- `outputDirectory` *(optional)* – output folder relative to the configuration file. Defaults to `Generated` next to the configuration file.

## Command-line usage

```
dotnet run --project src/Flowcast.CodeGenerator.Cli -- <settings-file> [options]
```

Options:

- `-o|--output <path>` – override the output directory.
- `-n|--namespace <name>` – override the root namespace for generated code.
- `-h|--help` – display usage information.

The CLI prints each generated file relative to the output directory. Warnings from the OpenAPI reader or generation process are written to `stderr`.

## Generated output

The generator produces:

1. `FlowcastRest.g.cs` – a partial static class exposing REST endpoints as strongly-typed methods returning `Result` or `Result<T>`.
2. `Result.g.cs` – helper result types that unify success/failure handling.
3. `Models.g.cs` – DTOs inferred from the OpenAPI components (`schemas`) section when available.

Each method is generated as a stub throwing `NotImplementedException`, allowing the Unity package to provide the actual HTTP implementation while keeping the source tree private to customers.

## Building

```
dotnet build Flowcast.CodeGenerator.sln
```

The resulting DLLs can be distributed alongside the Unity tooling once compiled in the build environment.
