using System.Threading.Tasks;
using DSharpPlus.Analyzers.Commands;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using Verifier =
    Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
        DSharpPlus.Analyzers.Commands.RegisterNestedClassesAnalyzer,
        Microsoft.CodeAnalysis.Testing.DefaultVerifier
    >;

namespace DSharpPlus.Analyzers.Test;

public static class ProcessorCheckTest
{
    [Test]
    public static async Task DiagnosticTestAsync()
    {
        CSharpAnalyzerTest<RegisterNestedClassesAnalyzer, DefaultVerifier> test
            = Utility.CreateAnalyzerTest<RegisterNestedClassesAnalyzer>();
        test.TestState.AdditionalReferences.Add(typeof(DSharpPlus.Commands.CommandContext).Assembly);

        test.TestCode = """
                        using System.Threading.Tasks;
                        using DSharpPlus.Commands.Trees.Metadata;
                        using DSharpPlus.Commands.Processors.TextCommands;
                        using DSharpPlus.Commands.Processors.SlashCommands;

                        public class Test 
                        {
                            [AllowedProcessors<SlashCommandProcessor>()]
                            public async Task Tester(TextCommandContext context)
                            {
                                await context.RespondAsync("Tester!");
                            }
                        }

                        """;

        test.ExpectedDiagnostics.Add(
            Verifier.Diagnostic()
                .WithLocation(9, 30)
                .WithSeverity(DiagnosticSeverity.Error)
                .WithMessage("Processor 'SlashCommandProcessor' does not support context 'TextCommandContext'")
        );

        await test.RunAsync();
    }
}
