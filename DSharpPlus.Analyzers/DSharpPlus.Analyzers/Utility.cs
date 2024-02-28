namespace DSharpPlus.Analyzers;

using Microsoft.CodeAnalysis;

public static class Utility
{
    public static bool CheckIfSameTypeByNamespace(TypeInfo typeInfo, string @namespace, Compilation compilation) => 
        typeInfo.Type?.Equals(compilation.GetTypeByMetadataName(@namespace), SymbolEqualityComparer.Default) ?? false;
}
