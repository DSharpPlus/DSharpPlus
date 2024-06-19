#!/usr/bin/env dotnet-script

#nullable enable

#r "nuget:Spectre.Console, 0.47.1-preview.0.26"

#load "../incremental-utility.csx"

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using Spectre.Console;

// Syntax:
// copy-concrete-implementations [path to meta file]
if (Args is ["-h" or "--help" or "-?"])
{
    AnsiConsole.MarkupLine
    (
        """
        [plum1]DSharpPlus Concrete Implementation Theft, v0.1.0[/]

          Usage: copy-concrete-implementations.csx [[path to meta file]]
          Extracts the required concrete types for DSharpPlus.Extensions.Internal.Builders to remove dependency on concrete implementations.
        
        """
    );

    return 0;
}

AnsiConsole.MarkupLine
(
    """
    [plum1]DSharpPlus Concrete Implementation Theft, v0.1.0[/]
    """
);

string meta;
string basePath = "./src/extensions/DSharpPlus.Extensions.Internal.Builders/Implementations/";

if (Args.Count == 0)
{
    meta = "./meta/builder-concrete-types.json";
}

// there are args passed, which override the given instructions
// validate the passed arguments are correct
else if (Args.Any(path => !Directory.Exists(path)))
{
    AnsiConsole.MarkupLine
    (
        """
        [red]The provided path could not be found on the file system.[/]
        """
    );

    return 1;
}

// all args are fine
else
{
    meta = Args[0];
}

string[] files = JsonSerializer.Deserialize<string[]>(File.ReadAllText(meta))!;

Changes changes = GetFileChanges("copy-concrete-implementations", files);

if (!changes.Added.Any() && !changes.Modified.Any() && !changes.Removed.Any())
{
    AnsiConsole.MarkupLine
    (
        """
        [darkseagreen1_1]There were no changes to the interface definitions, exiting copy-concrete-implementations.[/]
        """
    );

    return 0;
}

AnsiConsole.MarkupLine
(
    """

      Extracts the required concrete types for DSharpPlus.Extensions.Internal.Builders to remove dependency on concrete implementations.
    
    """
)

if (changes.Removed.Any())
{
    AnsiConsole.MarkupLine
    (
        """
        [red]Some concrete types were deleted. Please update the meta file and the corresponding builder code, if applicable:[/]
        """
    );

    foreach (string removed in changes.Removed)
    {
        Console.WriteLine($"  {removed}");
    }

    return 1;
}

if (changes.Added.Any() || changes.Modified.Any())
{
    AnsiConsole.MarkupLine("Extracting added and modified objects...");
}

IEnumerable<string> toExtract = changes.Added.Concat(changes.Modified);

bool success = toExtract.AsParallel()
    .All
    (
        path => 
        {
            Console.WriteLine($"  Extracting {path}");

            try
            {
                string text = File.ReadAllText(path);

                int typeIndex = text.IndexOf("record");

                text = text.Insert(typeIndex + 7, "Built");

                int namespaceIndex = text.IndexOf("namespace");
                int endOfNamespaceLine = text.IndexOf('\n', namespaceIndex);

                text = text.Remove(namespaceIndex, endOfNamespaceLine - namespaceIndex);
                text = text.Insert(namespaceIndex, "namespace DSharpPlus.Extensions.Internal.Builders.Implementations;");

                text = text.Replace("public sealed record", "internal sealed record");

                string filename = path.Split('/').Last();
                string outPath = basePath + "Built" + filename;

                File.WriteAllText(outPath, text);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}: {e.Message}\n{e.StackTrace}");
                return false;
            }

            return true;
        }
    );

return success ? 0 : 1;