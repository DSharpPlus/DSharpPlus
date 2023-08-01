// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Microsoft.CodeAnalysis;

namespace DSharpPlus.SourceGenerators.Common.Extensions;

/// <summary>
/// Contains convenience extensions on ITypeSymbol
/// </summary>
public static class TypeSymbolExtensions
{
    /// <summary>
    /// Returns all public properties of a type, including those inherited from base types. If this type is an
    /// interface, this returns all public properties of any base interfaces.
    /// </summary>
    /// <remarks>
    /// WARNING: This method comes with a drastic performance penalty.
    /// </remarks>
    public static IEnumerable<IPropertySymbol> GetPublicProperties
    (
        this ITypeSymbol type
    )
    {
        IEnumerable<IPropertySymbol> symbols = type.GetMembers()
        .Where
        (
            xm => xm is IPropertySymbol
            {
                DeclaredAccessibility: Accessibility.Public
            }
        )
        .Cast<IPropertySymbol>();

        if (type.BaseType is not null)
        {
            symbols = symbols.Concat
            (
                type.BaseType.GetPublicProperties()
            );
        }

        if (type is { TypeKind: TypeKind.Interface, Interfaces: not { IsDefaultOrEmpty: true } })
        {
            foreach (INamedTypeSymbol baseInterface in type.Interfaces)
            {
                symbols = symbols.Concat
                (
                    baseInterface.GetPublicProperties()
                );
            }
        }

        return symbols.Distinct(SymbolEqualityComparer.IncludeNullability).Cast<IPropertySymbol>();
    }
}
