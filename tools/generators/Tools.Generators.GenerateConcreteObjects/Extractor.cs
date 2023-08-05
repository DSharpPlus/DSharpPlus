// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tools.Generators.GenerateConcreteObjects;

/// <summary>
/// Provides a method to extract data from an interface declaration syntax
/// </summary>
internal static class Extractor
{
    public static IReadOnlyList<(string, string)> ExtractDefault
    (
        InterfaceDeclarationSyntax declaration
    )
    {
        List<(string, string)> properties = new();

        foreach (MemberDeclarationSyntax member in declaration.Members)
        {
            if (member is not PropertyDeclarationSyntax property)
            {
                continue;
            }

            properties.Add((property.Type.ToString(), property.Identifier.ToString()));
        }

        return properties;
    }

    public static IReadOnlyList<string> ExtractOverwrites
    (
        InterfaceDeclarationSyntax declaration
    )
    {
        List<string> overwrites = new();

        foreach (MemberDeclarationSyntax member in declaration.Members)
        {
            if (member is not PropertyDeclarationSyntax property)
            {
                continue;
            }

            if (!property.ChildTokens().Any(token => token.RawKind == (int)SyntaxKind.NewKeyword))
            {
                continue;
            }

            overwrites.Add(property.Identifier.ToString());
        }

        return overwrites;
    }
}
