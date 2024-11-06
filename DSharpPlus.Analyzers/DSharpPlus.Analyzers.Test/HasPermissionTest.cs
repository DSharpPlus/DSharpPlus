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
    public static async Task HasPermission_DiagnosticAsync()
    {
        CSharpAnalyzerTest<HasPermissionAnalyzer, DefaultVerifier> test =
            Utility.CreateAnalyzerTest<HasPermissionAnalyzer>();
        
        test.TestCode = """
                        using DSharpPlus.Entities;

                        public class DoesIt
                        {
                            public static bool HaveAdmin(DiscordPermissions perm) 
                            {
                                if ((perm & DiscordPermissions.Administrator) != 0) 
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
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessage("Use 'perm.HasPermission(DiscordPermissions.Administrator)' instead")
        );

        await test.RunAsync();
    }
}
