using System.Threading.Tasks;
using DSharpPlus.Analyzers.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    DSharpPlus.Analyzers.Core.UseDiscordPermissionsAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier
>;

namespace DSharpPlus.Analyzers.Test;

public class UseDiscordPermissionsTest
{
    [Test]
    public static async Task OrOperationTestAsync()
    {
        CSharpAnalyzerTest<UseDiscordPermissionsAnalyzer, DefaultVerifier> test =
            Utility.CreateAnalyzerTest<UseDiscordPermissionsAnalyzer>();

        test.TestCode = """
                        using DSharpPlus.Entities;
                        
                        public class PermissionsUtil
                        {
                            public static void AddAdmin(DiscordPermission perm) 
                            {
                                perm = perm | DiscordPermission.Administrator;
                            }
                        }
                        """;
        
        test.ExpectedDiagnostics.Add(
            Verifier.Diagnostic()
                .WithLocation(7, 9)
                .WithSeverity(DiagnosticSeverity.Error)
                .WithMessage("Use 'perm = perm + DiscordPermission.Administrator' instead"));
        
        await test.RunAsync();
    }

    [Test]
    public static async Task ExclusiveOrOperationTestAsync()
    {
        CSharpAnalyzerTest<UseDiscordPermissionsAnalyzer, DefaultVerifier> test =
            Utility.CreateAnalyzerTest<UseDiscordPermissionsAnalyzer>();

        test.TestCode = """
                        using DSharpPlus.Entities;

                        public class PermissionsUtil
                        {
                            public static void AddAdmin(DiscordPermission perm) 
                            {
                                perm = perm ^ DiscordPermission.Administrator;
                            }
                        }
                        """;
        
        test.ExpectedDiagnostics.Add(
            Verifier.Diagnostic()
                .WithLocation(7, 9)
                .WithSeverity(DiagnosticSeverity.Error)
                .WithMessage("Use 'perm = perm.Toggle(DiscordPermission.Administrator)' instead"));
        
        await test.RunAsync();
    }
    
    [Test]
    public static async Task AndOperationTestAsync()
    {
        CSharpAnalyzerTest<UseDiscordPermissionsAnalyzer, DefaultVerifier> test =
            Utility.CreateAnalyzerTest<UseDiscordPermissionsAnalyzer>();

        test.TestCode = """
                        using DSharpPlus.Entities;

                        public class PermissionsUtil
                        {
                            public static void AddAdmin(DiscordPermission perm) 
                            {
                                perm = perm & DiscordPermission.Administrator;
                            }
                        }
                        """;
        
        test.ExpectedDiagnostics.Add(
            Verifier.Diagnostic()
                .WithLocation(7, 9)
                .WithSeverity(DiagnosticSeverity.Error)
                .WithMessage("Use 'perm = perm - DiscordPermission.Administrator' instead"));
        
        await test.RunAsync();
    }
    
        [Test]
    public static async Task OrAssignmentTestAsync()
    {
        CSharpAnalyzerTest<UseDiscordPermissionsAnalyzer, DefaultVerifier> test =
            Utility.CreateAnalyzerTest<UseDiscordPermissionsAnalyzer>();

        test.TestCode = """
                        using DSharpPlus.Entities;
                        
                        public class PermissionsUtil
                        {
                            public static void AddAdmin(DiscordPermission perm) 
                            {
                                perm |= DiscordPermission.Administrator;
                            }
                        }
                        """;
        
        test.ExpectedDiagnostics.Add(
            Verifier.Diagnostic()
                .WithLocation(7, 9)
                .WithSeverity(DiagnosticSeverity.Error)
                .WithMessage("Use 'perm += DiscordPermission.Administrator' instead"));
        
        await test.RunAsync();
    }

    [Test]
    public static async Task ExclusiveOrAssignmentTestAsync()
    {
        CSharpAnalyzerTest<UseDiscordPermissionsAnalyzer, DefaultVerifier> test =
            Utility.CreateAnalyzerTest<UseDiscordPermissionsAnalyzer>();

        test.TestCode = """
                        using DSharpPlus.Entities;

                        public class PermissionsUtil
                        {
                            public static void AddAdmin(DiscordPermission perm) 
                            {
                                perm ^= DiscordPermission.Administrator;
                            }
                        }
                        """;
        
        test.ExpectedDiagnostics.Add(
            Verifier.Diagnostic()
                .WithLocation(7, 9)
                .WithSeverity(DiagnosticSeverity.Error)
                .WithMessage("Use 'perm = perm.Toggle(DiscordPermission.Administrator)' instead"));
        
        await test.RunAsync();
    }
    
    [Test]
    public static async Task AndAssignmentTestAsync()
    {
        CSharpAnalyzerTest<UseDiscordPermissionsAnalyzer, DefaultVerifier> test =
            Utility.CreateAnalyzerTest<UseDiscordPermissionsAnalyzer>();

        test.TestCode = """
                        using DSharpPlus.Entities;

                        public class PermissionsUtil
                        {
                            public static void AddAdmin(DiscordPermission perm) 
                            {
                                perm &= DiscordPermission.Administrator;
                            }
                        }
                        """;
        
        test.ExpectedDiagnostics.Add(
            Verifier.Diagnostic()
                .WithLocation(7, 9)
                .WithSeverity(DiagnosticSeverity.Error)
                .WithMessage("Use 'perm -= DiscordPermission.Administrator' instead"));
        
        await test.RunAsync();
    }

    [Test]
    public static async Task UsingBothTestAsync()
    {
        CSharpAnalyzerTest<UseDiscordPermissionsAnalyzer, DefaultVerifier> test =
            Utility.CreateAnalyzerTest<UseDiscordPermissionsAnalyzer>();

        test.TestCode = """
                        using DSharpPlus.Entities;

                        public class PermissionsUtil
                        {
                            public static void AddAdmin(DiscordPermissions perm) 
                            {
                                perm = perm | DiscordPermission.Administrator;
                            }
                        }
                        """;
        
        test.ExpectedDiagnostics.Add(
            Verifier.Diagnostic()
                .WithLocation(7, 9)
                .WithSeverity(DiagnosticSeverity.Error)
                .WithMessage("Use 'perm = perm + DiscordPermission.Administrator' instead"));
        
        await test.RunAsync();
    }

    [Test]
    public static async Task NoAssignmentTestAsync()
    {
        CSharpAnalyzerTest<UseDiscordPermissionsAnalyzer, DefaultVerifier> test =
            Utility.CreateAnalyzerTest<UseDiscordPermissionsAnalyzer>();

        test.TestCode = """
                        using DSharpPlus.Entities;

                        public class PermissionsUtil
                        {
                            public static DiscordPermission AddAdmin(DiscordPermission perm) 
                            {
                                return perm | DiscordPermission.Administrator;
                            }
                        }
                        """;
        
        test.ExpectedDiagnostics.Add(
            Verifier.Diagnostic()
                .WithLocation(7, 16)
                .WithSeverity(DiagnosticSeverity.Error)
                .WithMessage("Use 'perm + DiscordPermission.Administrator' instead"));
        
        await test.RunAsync();
    }
}
