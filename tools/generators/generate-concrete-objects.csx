#!/usr/bin/env dotnet-script

#nullable enable

#r "nuget:Microsoft.CodeAnalysis.CSharp, 4.8.0-2.final"
#r "nuget:Spectre.Console, 0.47.1-preview.0.26"

#load "../incremental-utility.csx"
#load "./parse-interface.csx"

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using Spectre.Console;

// Syntax:
// generate-concrete-objects [abstraction path] [output path]
if (Args is ["-h" or "--help" or "-?"])
{
    AnsiConsole.MarkupLine
    (
        """
        [plum1]DSharpPlus Concrete API Object Generator, v0.2.0[/]

          Usage: generate-concrete-objects.csx [[abstraction root path]] [[output root path]]
        """
    );

    return 0;
}

AnsiConsole.MarkupLine
(
    """
    [plum1]DSharpPlus Concrete API Object Generator, v0.1.0[/]
    """
);

string input, output;

// there are no args passed, proceed with default args:
// args[0] = src/core/DSharpPlus.Core.Abstractions.Models
// args[1] = src/core/DSharpPlus.Core.Models
if (Args.Count == 0)
{
    input = "src/core/DSharpPlus.Core.Abstractions.Models";
    output = "src/core/DSharpPlus.Core.Models";
}

// there are args passed, which override the given instructions
// validate the passed arguments are correct
else if (Args.Any(path => !Directory.Exists(path)))
{
    AnsiConsole.MarkupLine
    (
        """
        [red]The paths provided could not be found on the file system.[/]
        """
    );

    return 1;
}

// all args are fine
else
{
    input = Args[0];
    output = Args[1];
}

string[] files = Directory.GetFiles(input, "I*.cs", SearchOption.AllDirectories);

files = files
    .Select
    (
        path => 
        {
            FileInfo file = new(path);
            return file.FullName.Replace('\\', '/');
        }
    )
    .ToArray();

Changes changes = GetFileChanges("generate-concrete-objects", files);

if (!changes.Added.Any() && !changes.Modified.Any() && !changes.Removed.Any())
{
    AnsiConsole.MarkupLine
    (
        """
        [darkseagreen1_1]There were no changes to the interface definitions, exiting generate-concrete-objects.[/]
        """
    );

    return 0;
}

AnsiConsole.MarkupLine
(
    $"""
    {changes.Added.Count} added, {changes.Modified.Count} modified, {changes.Removed.Count} removed.
    """
);

if (changes.Removed.Any())
{
    AnsiConsole.MarkupLine("Deleting counterparts to removed files...");

    Parallel.ForEach
    (
        changes.Removed,
        path => 
        {
            int index = path.LastIndexOf('/');
            string deletePath = path.Remove(index + 1, 1).Replace(input, output);

            AnsiConsole.MarkupLine($"  Deleting '{deletePath}'...");

            if (File.Exists(deletePath))
            {
                File.Delete(deletePath);
            }
        }
    );
}

if (changes.Added.Any() || changes.Modified.Any())
{
    AnsiConsole.MarkupLine("Generating objects for modified/new definitions...");
}

IEnumerable<string> added = changes.Added.Select
(
    file =>
    {
        FileInfo info = new(file);
        return info.FullName.Replace('\\', '/');
    }
);

IEnumerable<string> modified = changes.Modified.Select
(
    file =>
    {
        FileInfo info = new(file);
        return info.FullName.Replace('\\', '/');
    }
);

IEnumerable<string> editedFiles = added.Concat(modified);
List<InterfaceMetadata> collectedMetadata = editedFiles
    .AsParallel()
    .Select(path => ParseInterface(path, input))
    .Where(meta => meta is not null)
    .Cast<InterfaceMetadata>()
    .AsEnumerable()
    .ToList();

for (int i = 0; i < collectedMetadata.Count; i++)
{
    InterfaceMetadata metadata = collectedMetadata[i];

    // make sure we emit the children of partials if the partial was edited
    if (metadata.IsPartial)
    {
        if (!files.Any(candidate => candidate.EndsWith(metadata.Name.Remove(1, 7))))
        {
            IEnumerable<string> candidateFiles = Directory.GetFiles(input, $"{metadata.Name.Remove(1, 7)}.cs", SearchOption.AllDirectories);

            if (candidateFiles.Any())
            {
                collectedMetadata.Add(ParseInterface(candidateFiles.First(), input)!);
            }
        }
    }

    int index = metadata.Path.LastIndexOf('/');
    string outputPath = metadata.Path.Remove(index + 1, 1).Replace(input, output);
    FileInfo info = new(outputPath);

    if (!Directory.Exists(info.DirectoryName!))
    {
        Directory.CreateDirectory(info.DirectoryName!);
    }

    StringBuilder writer = new();

    AnsiConsole.MarkupLine($"  Generating '{outputPath}'...");

    // emit a marker type for marker interfaces
    if (metadata.IsMarker)
    {
        writer.Append
        (
            $"""
            // This Source Code form is subject to the terms of the Mozilla Public
            // License, v. 2.0. If a copy of the MPL was not distributed with this
            // file, You can obtain one at https://mozilla.org/MPL/2.0/.

            using DSharpPlus.Core.Abstractions.Models;

            namespace DSharpPlus.Core.Models;

            /// <summary>
            /// Placeholder implementation of a marker interface. Please report spotting this to library developers.
            /// </summary>
            internal sealed record {metadata.Name[1..]} : {metadata.Name};
            """
        );

        File.WriteAllText(outputPath, writer.ToString());
        continue;
    }

    writer.AppendLine
    (
        """
        // This Source Code form is subject to the terms of the Mozilla Public
        // License, v. 2.0. If a copy of the MPL was not distributed with this
        // file, You can obtain one at https://mozilla.org/MPL/2.0/.

        """
    );

    // we specifically want the grouping here, that's how we group usings together
    // this doesn't currently handle using statics, but it doesn't need to just yet
    //
    // sorting system directives first is annoying, and not strictly necessary, but we want to do it anyway
    // we're going to employ a hack and return "!" as the key if it's a system using, which is higher than any
    // legal namespace name
    IEnumerable<IGrouping<string, string>> groupedUsings = metadata.UsingDirectives!
    .Append("using DSharpPlus.Core.Abstractions.Models;")
    .Distinct()
    .GroupBy
    (
        directive => 
        {
            int index = directive.IndexOf('.');
            int semicolonIndex = directive.IndexOf(';');
            return directive[..(index != -1 ? index : semicolonIndex)];
        }
    )
    .OrderBy
    (
        group =>
        {
            if (group.First().StartsWith("using System"))
            {
                return "!";
            }
            else
            {
                string directive = group.First();

                int index = directive.IndexOf('.');
                int semicolonIndex = directive.IndexOf(';');
                return directive[..(index != -1 ? index : semicolonIndex)];
            }
        },
        StringComparer.Ordinal
    );

    foreach(IGrouping<string, string> group in groupedUsings)
    {
        writer.AppendLine
        (
            $"""
            {string.Join('\n', group.OrderBy(name => name))}

            """
        );
    }

    writer.AppendLine
    (
        $$"""
        namespace DSharpPlus.Core.Models;

        /// <inheritdoc cref="{{metadata.Name}}" />
        public sealed record {{metadata.Name[1..]}} : {{metadata.Name}}
        {
        """
    );

    InterfaceMetadata principal = metadata.PartialParent ?? metadata;
    InterfaceMetadata? overwrites = metadata.PartialParent is not null ? metadata : null;

    foreach (PropertyMetadata property in principal.Properties!)
    {
        bool required = !property.IsOptional && !property.IsNullable;
        string type = property.Type;

        // strip optionality if we're being overwritten
        if (overwrites is not null && overwrites.Properties!.Any(candidate => candidate.IsOverwrite && candidate.Name == property.Name))
        {
            required = !property.IsNullable;
            type = type.EndsWith('?') ? $"{type[9..^2]}?" : type[9..^1];
        }

        writer.AppendLine
        (
            $$"""
                /// <inheritdoc/>
                public {{(required ? "required " : "")}}{{type}} {{property.Name}} { get; init; }

            """
        );
    }

    writer.Append('}');

    string code = writer.ToString();

    code = code.Remove(code.Length - Environment.NewLine.Length - 1, Environment.NewLine.Length);

    File.WriteAllText(outputPath, code);
}

return 0;