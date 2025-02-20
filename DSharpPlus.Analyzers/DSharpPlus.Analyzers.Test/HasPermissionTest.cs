using System.Threading.Tasks;
using DSharpPlus.Analyzers.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    DSharpPlus.Analyzers.Core.HasPermissionAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier
>;

namespace DSharpPlus.Analyzers.Test;

public class HasPermissionTest
{
    /// <summary>
    /// Unit test to see if HasPermissionAnalyzer reports
    /// </summary>
    [Test]
    public static async Task HasPermissionNotEquals_DiagnosticAsync()
    {
        CSharpAnalyzerTest<HasPermissionAnalyzer, DefaultVerifier> test =
            Utility.CreateAnalyzerTest<HasPermissionAnalyzer>();
        
        test.TestCode = """
                        using DSharpPlus.Entities;

                        public class DoesIt
                        {
                            public static bool HaveAdmin(DiscordPermission perm) 
                            {
                                if ((perm & DiscordPermission.Administrator) != 0) 
                                {
                                    return true;
                                }
                                return false;
                            }
                        }
                        """;
        
        test.ExpectedDiagnostics.Add
        (
            Verifier.Diagnostic()
                .WithLocation(7, 13)
                .WithSeverity(DiagnosticSeverity.Hidden)
                .WithMessage("Use 'perm.HasPermission(DiscordPermission.Administrator)' instead")
        );

        await test.RunAsync();
    }

    [Test]
    public async Task HasPermissionEquals_DiagnosticAsync()
    {
        CSharpAnalyzerTest<HasPermissionAnalyzer, DefaultVerifier> test =
            Utility.CreateAnalyzerTest<HasPermissionAnalyzer>();
        
        test.TestCode = """
                        using DSharpPlus.Entities;

                        public class DoesIt
                        {
                            public static bool HaveNoAdmin(DiscordPermission perm) 
                            {
                                if ((perm & DiscordPermission.Administrator) == 0) 
                                {
                                    return true;
                                }
                                return false;
                            }
                        }

                        """;
        
        test.ExpectedDiagnostics.Add
        (
            Verifier.Diagnostic()
                .WithLocation(7, 13)
                .WithSeverity(DiagnosticSeverity.Hidden)
                .WithMessage("Use 'perm.HasPermission(DiscordPermission.Administrator)' instead")
        );

        await test.RunAsync();
    }
}
