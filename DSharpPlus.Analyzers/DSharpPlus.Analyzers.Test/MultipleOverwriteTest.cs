using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using Verifier =
    Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
        DSharpPlus.Analyzers.MultipleOverwriteAnalyzer,
        Microsoft.CodeAnalysis.Testing.DefaultVerifier
    >;

namespace DSharpPlus.Analyzers.Test;

public static class MultipleOverwriteTest
{
    [Test]
    public static async Task MultipleOverwriteTest_DiagnosticAsync()
    {
        CSharpAnalyzerTest<MultipleOverwriteAnalyzer, DefaultVerifier> test =
            Utility.CreateAnalyzerTest<MultipleOverwriteAnalyzer>();
        test.TestCode = @"
using DSharpPlus.Entities;
using System.Threading.Tasks;

public class OverwriteTest
{
    public async Task AddOverwritesAsync(DiscordChannel channel, DiscordMember member, DiscordMember member2)
    {
        await channel.AddOverwriteAsync(member, DiscordPermissions.BanMembers);
        await channel.AddOverwriteAsync(member2, DiscordPermissions.KickMembers);
    }
}
";
        test.ExpectedDiagnostics.Add(
            Verifier.Diagnostic()
                .WithLocation(10, 15)
                .WithMessage("Use  one 'channel.ModifyAsync(..)' instead of multiple 'channel.AddOverwriteAsync(..)'")
        );
        await test.RunAsync();
    }
}
