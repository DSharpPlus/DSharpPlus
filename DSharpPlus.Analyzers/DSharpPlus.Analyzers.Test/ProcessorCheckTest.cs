using System.Threading.Tasks;
using DSharpPlus.Analyzers.Commands;
using DSharpPlus.Commands;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    DSharpPlus.Analyzers.Commands.ProcessorCheckAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier
>;

namespace DSharpPlus.Analyzers.Test;

public static class ProcessorCheckTest
{
    [Test]
    public static async Task DiagnosticTestAsync()
    {
        CSharpAnalyzerTest<ProcessorCheckAnalyzer, DefaultVerifier> test
            = Utility.CreateAnalyzerTest<ProcessorCheckAnalyzer>();
        test.TestState.AdditionalReferences.Add(typeof(CommandContext).Assembly);

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
                .WithMessage("All provided processors does not support context 'TextCommandContext'")
        );

        await test.RunAsync();
    }
}
