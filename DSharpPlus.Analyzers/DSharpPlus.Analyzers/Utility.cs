namespace DSharpPlus.Analyzers;

using Microsoft.CodeAnalysis;

public static class Utility
{
    /// <summary>
    /// Checks if the type is equal to the name provided
    /// </summary>
    /// <param name="compilation">The compilation used to get types</param>
    /// <param name="typeInfo">The type the compare</param>
    /// <param name="fullyQualifiedName">The fully qualified name to the type</param>
    /// <returns></returns>
    public static bool CheckByName(this Compilation compilation, TypeInfo typeInfo, string fullyQualifiedName) => 
        typeInfo.Type?.Equals(compilation.GetTypeByMetadataName(fullyQualifiedName), SymbolEqualityComparer.Default) ?? false;
}
