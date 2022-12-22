using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace DSharpPlus.Analyzers.Extensions;

internal static class NamespaceSymbolExtensions
{
    public static string GetFullNamespace(this INamespaceSymbol symbol)
    {
        INamespaceSymbol @namespace = symbol;

        List<string> names = new()
        {
            @namespace.Name
        };

        while (!@namespace.ContainingNamespace.IsGlobalNamespace)
        {
            @namespace = @namespace.ContainingNamespace;
            names.Add(@namespace.Name);
        }

        names.Reverse();
        return string.Join(".", names);
    }
}
