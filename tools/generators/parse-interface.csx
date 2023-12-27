#!/usr/bin/env dotnet-script

#nullable enable

#r "nuget:Microsoft.CodeAnalysis.CSharp, 4.8.0-2.final"
#r "nuget:Spectre.Console, 0.47.1-preview.0.26"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Spectre.Console;

// this script aims to provide a quick way for all object generators to obtain metadata about the interface they're working with,
// and thus covers all use-cases of these object generators.

/// <summary>
/// Represents metadata about an interface definition.
/// </summary>
public sealed record InterfaceMetadata
{
    /// <summary>
    /// The name of this interface.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The file path of this interface.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Indicates whether this interface is a marker interface.
    /// </summary>
    public required bool IsMarker { get; init; }

    /// <summary>
    /// Indicates whether this interface is a partial interface, as in, represents a partially populated discord object.
    /// </summary>
    public required bool IsPartial { get; init; }

    /// <summary>
    /// The properties of this interface. <c>null</c> if <seealso cref="IsMarker"/> is true.
    /// </summary>
    public IReadOnlyList<PropertyMetadata>? Properties { get; init; }

    /// <summary>
    /// Parsed metadata about all parent interfaces, except partial parents.
    /// </summary>
    public IReadOnlyList<InterfaceMetadata>? ParentInterfaces { get; init; }

    /// <summary>
    /// The partial parent interface, if present.
    /// </summary>
    public InterfaceMetadata? PartialParent { get; init; }

    /// <summary>
    /// The using directives used by these files, necessary for referencing their property types.
    /// </summary>
    public IReadOnlyList<string>? UsingDirectives { get; init; }
}

/// <summary>
/// Represents metadata about one specific property.
/// </summary>
public readonly record struct PropertyMetadata
{
    /// <summary>
    /// The declared type of this property.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// The declared name of this property.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Indicates whether this property is optional.
    /// </summary>
    public required bool IsOptional { get; init; }

    /// <summary>
    /// Indicates whether this property is nullable.
    /// </summary>
    public required bool IsNullable { get; init; }

    /// <summary>
    /// Indicates whether this property overwrites a property in a partial parent, using the <c>new</c> keyword.
    /// </summary>
    /// <value></value>
    public required bool IsOverwrite { get; init; }
}

/// <summary>
/// Parses an interface from a specified file, returning a metadata object if an interface was present.
/// </summary>
public InterfaceMetadata? ParseInterface(string filename, string baseDirectory)
{
    CompilationUnitSyntax root = SyntaxFactory.ParseCompilationUnit(File.ReadAllText(filename));

    List<string> usings = new();

    foreach (UsingDirectiveSyntax @using in root.Usings)
    {
        usings.Add(@using.ToString()!);
    }

    if
    (
        root.Members.First() is not FileScopedNamespaceDeclarationSyntax fileScopedNamespace 
        || fileScopedNamespace.Members.First() is not InterfaceDeclarationSyntax interfaceSyntax
    )
    {
        AnsiConsole.MarkupLine($"  No interface detected in '{filename}', abandoning...");

        return null;
    }

    IEnumerable<SyntaxToken> tokens = interfaceSyntax.ChildTokens();
    SyntaxToken name = default;

    foreach (SyntaxToken token in tokens)
    {
        if (token is { RawKind: (int)SyntaxKind.IdentifierToken })
        {
            name = token;
        }

        if (token is { RawKind: (int)SyntaxKind.SemicolonToken })
        {
            AnsiConsole.MarkupLine($"  Marker interface detected in '{filename}'.");

            return new()
            {
                Name = name.Text,
                Path = filename,
                IsMarker = true,
                IsPartial = false
            };
        }
    }

    // look at parent interfaces and potential partials
    BaseListSyntax? baseInterfaceList = (BaseListSyntax?)interfaceSyntax.ChildNodes().FirstOrDefault(node => node is BaseListSyntax);

    string? partialParent = null;
    List<string> parents = new();

    if (baseInterfaceList is not null)
    {
        foreach (SyntaxNode node in baseInterfaceList.ChildNodes())
        {
            if (node is not SimpleBaseTypeSyntax candidate)
            {
                continue;
            }

            if (node.ChildNodes().First() is not IdentifierNameSyntax identifier)
            {
                continue;
            }

            foreach (SyntaxToken token in identifier.ChildTokens())
            {
                if (token is { RawKind: (int)SyntaxKind.IdentifierToken })
                {
                    if (token.Text.StartsWith("IPartial"))
                    {
                        partialParent = token.Text;
                        break;
                    }

                    parents.Add(token.Text);
                    break;
                }
            }
        }
    }

    // start extracting properties. Nullability/optionality checks are very primitive, but they're accurate enough for our use-case
    // (which is basically just understanding when to put `required` on an emitted property)

    List<PropertyMetadata> properties = new();

    foreach (MemberDeclarationSyntax member in interfaceSyntax.Members)
    {
        if (member is not PropertyDeclarationSyntax property)
        {
            continue;
        }
            
        string type = property.Type.ToString();

        properties.Add
        (
            new()
            {
                Type = type,
                Name = property.Identifier.ToString(),
                IsOptional = type.StartsWith("Optional<"),
                IsNullable = type.EndsWith('?') || (type.StartsWith("Optional<") && type.EndsWith("?>")),
                IsOverwrite = property.ChildTokens().Any(token => token.RawKind == (int)SyntaxKind.NewKeyword)
            }
        );
    }

    List<InterfaceMetadata> parentMetadata = new();

    foreach (string interfaceName in parents)
    {
        IEnumerable<string> path = Directory.GetFiles(baseDirectory, $"{interfaceName}.cs", SearchOption.AllDirectories);

        if (!path.Any())
        {
            continue;
        }

        InterfaceMetadata? potentialParent = ParseInterface(path.First(), baseDirectory);

        if (potentialParent is not null)
        {
            parentMetadata.Add(potentialParent);
        }
    }

    InterfaceMetadata? partialParentMetadata = null;

    if (partialParent is not null)
    {
        IEnumerable<string> path = Directory.GetFiles(baseDirectory, $"{partialParent}.cs", SearchOption.AllDirectories);

        if (path.Any())
        {
            partialParentMetadata = ParseInterface(path.First(), baseDirectory);
        }
    }

    // the most painful part of all: consolidate usings
    if (partialParentMetadata?.UsingDirectives is not null)
    {
        usings.AddRange(partialParentMetadata.UsingDirectives);
    }

    foreach (InterfaceMetadata i in parentMetadata)
    {
        if (i.UsingDirectives is not null)
        {
            usings.AddRange(i.UsingDirectives);
        }
    }

    return new()
    {
        Name = name.Text,
        Path = filename,
        IsMarker = false,
        IsPartial = name.Text.StartsWith("IPartial"),
        Properties = properties,
        ParentInterfaces = parentMetadata,
        PartialParent = partialParentMetadata,
        UsingDirectives = usings.Distinct().ToArray()
    };
}
