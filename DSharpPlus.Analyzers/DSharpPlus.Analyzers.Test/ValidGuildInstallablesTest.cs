using System.Threading.Tasks;
using DSharpPlus.Analyzers.Commands;
using DSharpPlus.Commands;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using Verifier
    = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
        DSharpPlus.Analyzers.Commands.ValidGuildInstallablesAnalyzer,
        Microsoft.CodeAnalysis.Testing.DefaultVerifier
    >;

namespace DSharpPlus.Analyzers.Test;

public static class ValidGuildInstallablesTest
{
    [Test]
    public static async Task DiagnoistTestAsync()
    {
        CSharpAnalyzerTest<ValidGuildInstallablesAnalyzer, DefaultVerifier> test
            = Utility.CreateAnalyzerTest<ValidGuildInstallablesAnalyzer>();
        test.TestState.AdditionalReferences.Add(typeof(CommandContext).Assembly);

        test.TestCode = """
                        using System.Threading.Tasks;
                        using DSharpPlus;
                        using DSharpPlus.Entities;
                        using DSharpPlus.Commands;
                        using DSharpPlus.Commands.Processors.SlashCommands.Metadata;

                        public class PingCommand 
                        {
                            [Command("ping"), RegisterToGuilds(379378609942560770)] 
                            [InteractionInstallType(DiscordApplicationIntegrationType.UserInstall)]
                            public static async ValueTask ExecuteAsync(CommandContext ctx) 
                            {
                                await ctx.RespondAsync("Pong!");
                            }
                        }
                        """;

        test.ExpectedDiagnostics.Add
        (
            Verifier.Diagnostic()
                .WithLocation(11, 35)
                .WithSeverity(DiagnosticSeverity.Error)
                .WithMessage(
                    "Cannot register 'ExecuteAsync' to the specified guilds as its installable context does not contain 'GuildInstall'"
                )
        );

        await test.RunAsync();
    }
}
