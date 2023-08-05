// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Spectre.Console;

using Tools.IncrementalUtility;

namespace Tools.Generators.GenerateConcreteObjects;

public static class Program
{
    // SYNTAX:
    // generate-concrete-objects [abstraction path] [output path]
    // it is important to pass the path to the abstraction ROOT, not to the directories containing
    // the abstractions files directly
    public static int Main(string[] args)
    {
        // primitive help command
        if (args is ["-h" or "--help" or "-?"])
        {
            AnsiConsole.MarkupLine
            (
                """
                DSharpPlus Concrete API Object Generator, v0.1.0

                  [plum1]Usage: generate-concrete-objects path/to/abstractions/root output/path[/]
                """
            );

            return 0;
        }

        string input, output;

        // there are no args passed, proceed with default args:
        // args[0] = src/core/DSharpPlus.Core.Abstractions.Models
        // args[1] = src/core/DSharpPlus.Core.Models
        if (args.Length == 0)
        {
            input = "src/core/DSharpPlus.Core.Abstractions.Models";
            output = "src/core/DSharpPlus.Core.Models";
        }
        // there are args passed, which override the given instructions
        // validate the passed arguments are correct
        else if (args.Any(path => !Directory.Exists(path)))
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
            input = args[0];
            output = args[1];
        }

        MetadataCollection collection = new("generate-concrete-objects");

        string[] files = Directory.GetFiles(input, "I*.cs", SearchOption.AllDirectories);

        Changes changes = collection.CalculateDifferences(files);

        if (!changes.Added.Any() && !changes.Modified.Any() && !changes.Removed.Any())
        {
            AnsiConsole.MarkupLine
            (
                """
                [darkseagreen1_1]There were no changes to the interface definitions, exiting generator.[/]
                """
            );

            return 0;
        }
        else
        {
            AnsiConsole.MarkupLine
            (
                $"""
                {changes.Added.Count()} added, {changes.Modified.Count()} modified, {changes.Removed.Count()} removed.
                Generating concrete objects...
                """
            );
        }

        AnsiConsole.MarkupLine
        (
            """
            Deleting counterparts to removed files...
            """
        );

        foreach (string path in changes.Removed)
        {
            string deletePath = string.Concat(output, path.AsSpan(input.Length));

            AnsiConsole.MarkupLine
            (
                $"""
                  Deleting '{deletePath.Replace('\\', '/')}'
                """
            );

            if (File.Exists(deletePath))
            {
                File.Delete(deletePath);
            }
        }

        AnsiConsole.MarkupLine
        (
            """
            Generating objects for modified/new definitions...
            """
        );

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

        List<string> emittedFiles = new();

        foreach (string path in added.Concat(modified))
        {
            CompilationUnitSyntax root = SyntaxFactory.ParseCompilationUnit
            (
                File.ReadAllText(path)
            );

            int index = path.LastIndexOf("/");
            string outPath = path.Remove(index + 1, 1).Replace(input, output);

            StringBuilder writer = new();

            AnsiConsole.MarkupLine
            (
                $"""
                  Generating {outPath}
                """
            );

            /*writer.WriteLine
            (
"""
// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

"""
            );*/

            foreach (UsingDirectiveSyntax @using in root.Usings)
            {
                writer.Append(@using.ToFullString());
            }

            writer.Append
            (
"""

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;


"""
            );

            if
            (
                root.Members.First() is not FileScopedNamespaceDeclarationSyntax fileScopedNamespace ||
                fileScopedNamespace.Members.First() is not InterfaceDeclarationSyntax interfaceSyntax
            )
            {
                AnsiConsole.MarkupLine
                (
                    $"""
                        No interface detected, abandoning generation.
                    """
                );

                continue;
            }

            IEnumerable<SyntaxToken> tokens = interfaceSyntax.ChildTokens();
            SyntaxToken name = default;
            bool found = false;
            bool marker = false;

            foreach (SyntaxToken token in tokens)
            {
                if (token is { RawKind: (int)SyntaxKind.IdentifierToken })
                {
                    name = token;
                    found = true;
                }

                // detect a marker
                if (found && token is { RawKind: (int)SyntaxKind.SemicolonToken })
                {
                    AnsiConsole.MarkupLine
                    (
                        $"""
                            Marker interface detected, abandoning generation.
                        """
                    );
                    marker = true;
                }
            }

            if (marker)
            {
                continue;
            }

            writer.AppendLine
            (
$$"""
/// <inheritdoc cref="{{name.Text}}" />
public sealed record {{name.Text[1..]}} : {{name.Text}}
{
"""
            );

            BaseListSyntax? interfaceList = (BaseListSyntax?)interfaceSyntax.ChildNodes()
            .FirstOrDefault
            (
                node => node is BaseListSyntax
            );

            if (interfaceList is not null)
            {
                string? partialCandidate = interfaceList.ChildNodes()
                .Where
                (
                    node =>
                    {
                        if (node is not SimpleBaseTypeSyntax candidate)
                        {
                            return false;
                        }

                        if (node.ChildNodes().First() is not IdentifierNameSyntax identifier)
                        {
                            return false;
                        }

                        foreach (SyntaxToken token in identifier.ChildTokens())
                        {
                            if (token is { RawKind: (int)SyntaxKind.IdentifierToken })
                            {
                                if (token.Text.StartsWith("IPartial"))
                                {
                                    return true;
                                }
                            }
                        }

                        return false;
                    }
                )
                .Select
                (
                    node =>
                    {
                        foreach (SyntaxToken token in node.ChildNodes().First().ChildTokens())
                        {
                            if (token is { RawKind: (int)SyntaxKind.IdentifierToken })
                            {
                                if (token.Text.StartsWith("IPartial"))
                                {
                                    return token.Text;
                                }
                            }
                        }

                        // unreachable
                        return null!;
                    }
                )
                .FirstOrDefault();

                Console.WriteLine($"    Partial candidate: {partialCandidate ?? "none"}");

                if (partialCandidate is not null)
                {
                    // we're dealing with a partial-extending here
                    string partialPath = path.Replace(interfaceSyntax.Identifier.Text, partialCandidate);
                    CompilationUnitSyntax partialRoot = SyntaxFactory.ParseCompilationUnit
                    (
                        File.ReadAllText(partialPath)
                    );

                    if
                    (
                        partialRoot.Members.First() is not FileScopedNamespaceDeclarationSyntax partialFileScopedNamespace ||
                        partialFileScopedNamespace.Members.First() is not InterfaceDeclarationSyntax partialInterfaceSyntax
                    )
                    {
                        AnsiConsole.MarkupLine
                        (
                            $"""
                                No partial base interface detected, abandoning generation.
                            """
                        );

                        continue;
                    }

                    Emitter.Emit
                    (
                        writer,
                        Extractor.ExtractDefault(partialInterfaceSyntax),
                        Extractor.ExtractOverwrites(interfaceSyntax)
                    );
                }
                else
                {
                    Emitter.Emit
                    (
                        writer,
                        Extractor.ExtractDefault(interfaceSyntax)
                    );
                }
            }
            else
            {
                Emitter.Emit
                (
                    writer,
                    Extractor.ExtractDefault(interfaceSyntax)
                );
            }

            writer.Append('}');

            // ensure the directory at least exists
            FileInfo outInfo = new(outPath);
            if (!Directory.Exists(outInfo.DirectoryName!))
            {
                Directory.CreateDirectory(outInfo.DirectoryName!);
            }

            string code = writer.ToString();

            // remove the last newline before the final }
            // the windows conditional is because of CRLF taking up two characters, vs LF taking up one
            code = Environment.NewLine == "\r\n" 
                ? code.Remove(code.Length - 3, 2) 
                : code.Remove(code.Length - 2, 1);

            File.WriteAllText(outPath, code);

            emittedFiles.Add(outPath);
        }

        return 0;
    }
}
