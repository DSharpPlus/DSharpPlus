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
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80
        };
        test.TestState.AdditionalReferences.Add(typeof(DiscordClient).Assembly);
        return test;
    }
}
