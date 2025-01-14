using System.Threading.Tasks;
using DSharpPlus.Analyzers.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    DSharpPlus.Analyzers.Core.MultipleOverwriteAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier
>;

namespace DSharpPlus.Analyzers.Test;

/// <summary>
/// 
/// </summary>
public static class MultipleOverwriteTest
{
    /// <summary>
    /// Single diagnostic report for multiple overwrite analyzer
    /// </summary>
    [Test]
    public static async Task DiagnosticAsync()
    {
        CSharpAnalyzerTest<MultipleOverwriteAnalyzer, DefaultVerifier> test =
            Utility.CreateAnalyzerTest<MultipleOverwriteAnalyzer>();

        test.TestCode = """
                        using DSharpPlus.Entities;
                        using System.Threading.Tasks;

                        public class OverwriteTest
                        {
                            public async Task AddOverwritesAsync(DiscordChannel channel, DiscordMember member, DiscordMember member2)
                            {
                                await channel.AddOverwriteAsync(member, DiscordPermission.BanMembers);
                                await channel.AddOverwriteAsync(member2, DiscordPermission.KickMembers);
                            }
                        }
                        """;

        test.ExpectedDiagnostics.Add
        (
            Verifier.Diagnostic()
                .WithLocation(9, 15)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessage("Use one 'channel.ModifyAsync(..)' instead of multiple 'channel.AddOverwriteAsync(..)'")
        );
        await test.RunAsync();
    }

    [Test]
    public static async Task MultipleErrorsScenarioTestAsync()
    {
        CSharpAnalyzerTest<MultipleOverwriteAnalyzer, DefaultVerifier> test =
            Utility.CreateAnalyzerTest<MultipleOverwriteAnalyzer>();

        test.TestCode = """
                        using DSharpPlus.Entities;
                        using System.Threading.Tasks;

                        public class OverwriteTest
                        {
                            public async Task AddOverwritesAsync(DiscordChannel channel, DiscordMember member, DiscordMember member2)
                            {
                                await channel.AddOverwriteAsync(member, DiscordPermission.BanMembers);
                                await channel.AddOverwriteAsync(member2, DiscordPermission.KickMembers);
                                await channel.AddOverwriteAsync(member2, DiscordPermission.Administrator);
                            }
                        }
                        """;

        test.ExpectedDiagnostics.Add
        (
            Verifier.Diagnostic()
                .WithLocation(9, 15)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessage("Use one 'channel.ModifyAsync(..)' instead of multiple 'channel.AddOverwriteAsync(..)'")
        );

        test.ExpectedDiagnostics.Add
        (
            Verifier.Diagnostic()
                .WithLocation(10, 15)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessage("Use one 'channel.ModifyAsync(..)' instead of multiple 'channel.AddOverwriteAsync(..)'")
        );

        await test.RunAsync();
    }

    [Test]
    public static async Task ForEachLoopDiagnosticAsync()
    {
        CSharpAnalyzerTest<MultipleOverwriteAnalyzer, DefaultVerifier> test
            = Utility.CreateAnalyzerTest<MultipleOverwriteAnalyzer>();

        test.TestCode = """
                        using DSharpPlus.Entities;
                        using System.Threading.Tasks;
                        using System.Collections.Generic;

                        public class OverwriteTest
                        {
                            public async Task AddOverwritesAsync(DiscordChannel channel, List<DiscordMember> members)
                            {
                                foreach (DiscordMember member in members) 
                                {
                                    await channel.AddOverwriteAsync(member, DiscordPermission.BanMembers);
                                }
                            }
                        }
                        """;

        test.ExpectedDiagnostics.Add
        (
            Verifier.Diagnostic()
                .WithLocation(11, 19)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessage("Use one 'channel.ModifyAsync(..)' instead of multiple 'channel.AddOverwriteAsync(..)'")
        );

        await test.RunAsync();
    }

    [Test]
    public static async Task ForLoopDiagnosticAsync()
    {
        CSharpAnalyzerTest<MultipleOverwriteAnalyzer, DefaultVerifier> test
            = Utility.CreateAnalyzerTest<MultipleOverwriteAnalyzer>();

        test.TestCode = """
                        using DSharpPlus.Entities;
                        using System.Threading.Tasks;
                        using System.Collections.Generic;

                        public class OverwriteTest
                        {
                            public async Task AddOverwritesAsync(DiscordChannel channel, List<DiscordMember> members)
                            {
                                for (int i = 0; i < members.Count; i++)
                                {
                                    await channel.AddOverwriteAsync(members[i], DiscordPermission.BanMembers);
                                }
                            }
                        }
                        """;

        test.ExpectedDiagnostics.Add
        (
            Verifier.Diagnostic()
                .WithLocation(11, 19)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessage("Use one 'channel.ModifyAsync(..)' instead of multiple 'channel.AddOverwriteAsync(..)'")
        );

        await test.RunAsync();
    }

    [Test]
    public static async Task WhileLoopDiagnosticAsync()
    {
        CSharpAnalyzerTest<MultipleOverwriteAnalyzer, DefaultVerifier> test
            = Utility.CreateAnalyzerTest<MultipleOverwriteAnalyzer>();

        test.TestCode = """
                        using DSharpPlus.Entities;
                        using System.Threading.Tasks;
                        using System.Collections.Generic;

                        public class OverwriteTest
                        {
                            public async Task AddOverwritesAsync(DiscordChannel channel, List<DiscordMember> members)
                            {
                                int i = 0;
                                while (i < members.Count)
                                {
                                    await channel.AddOverwriteAsync(members[i], DiscordPermission.BanMembers);
                                    i++;
                                }
                            }
                        }
                        """;

        test.ExpectedDiagnostics.Add
        (
            Verifier.Diagnostic()
                .WithLocation(12, 19)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessage("Use one 'channel.ModifyAsync(..)' instead of multiple 'channel.AddOverwriteAsync(..)'")
        );

        await test.RunAsync();
    }
}
