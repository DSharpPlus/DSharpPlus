using System.IO;

using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace DSharpPlus.Analyzers.Test;

public static class Utility
{
    public static CSharpAnalyzerTest<T, DefaultVerifier> CreateAnalyzerTest<T>()
    where T : DiagnosticAnalyzer, new()
    {
        CSharpAnalyzerTest<T, DefaultVerifier> test = new()
        {
            ReferenceAssemblies = new ReferenceAssemblies
            (
                targetFramework: "net11.0",
                referenceAssemblyPackage: new PackageIdentity("Microsoft.NETCore.App.Ref", "11.0.0"),
                referenceAssemblyPath: Path.Combine("ref", "net11.0")
            )
        };
        test.TestState.AdditionalReferences.Add(typeof(DiscordClient).Assembly);
        return test;
    }
}
