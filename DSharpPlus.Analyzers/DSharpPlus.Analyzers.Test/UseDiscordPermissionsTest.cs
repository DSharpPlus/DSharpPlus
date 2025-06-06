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
            Verifier.Diagnostic("DSP0009")
                .WithLocation(7, 9)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessage("Use 'DiscordPermissions' instead of operating on 'DiscordPermission'"));

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
            Verifier.Diagnostic("DSP0009")
                .WithLocation(7, 9)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessage("Use 'DiscordPermissions' instead of operating on 'DiscordPermission'"));

        await test.RunAsync();
    }

    [Test]
    public static async Task AndNotOperationTestAsync()
    {
        CSharpAnalyzerTest<UseDiscordPermissionsAnalyzer, DefaultVerifier> test =
            Utility.CreateAnalyzerTest<UseDiscordPermissionsAnalyzer>();

        test.TestCode = """
                        using DSharpPlus.Entities;

                        public class PermissionsUtil
                        {
                            public static void AddAdmin(DiscordPermission perm) 
                            {
                                perm = perm & ~DiscordPermission.Administrator;
                            }
                        }
                        """;

        test.ExpectedDiagnostics.Add(
            Verifier.Diagnostic("DSP0009")
                .WithLocation(7, 9)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessage("Use 'DiscordPermissions' instead of operating on 'DiscordPermission'"));

        await test.RunAsync();
    }

    [Test]
    public static async Task AndNotParenthesizedOperationTestAsync()
    {
        CSharpAnalyzerTest<UseDiscordPermissionsAnalyzer, DefaultVerifier> test =
            Utility.CreateAnalyzerTest<UseDiscordPermissionsAnalyzer>();

        test.TestCode = """
                        using DSharpPlus.Entities;

                        public class PermissionsUtil
                        {
                            public static void AddAdmin(DiscordPermission perm) 
                            {
                                perm = perm & (~DiscordPermission.Administrator);
                            }
                        }
                        """;

        test.ExpectedDiagnostics.Add(
            Verifier.Diagnostic("DSP0009")
                .WithLocation(7, 9)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessage("Use 'DiscordPermissions' instead of operating on 'DiscordPermission'"));

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
            Verifier.Diagnostic("DSP0009")
                .WithLocation(7, 9)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessage("Use 'DiscordPermissions' instead of operating on 'DiscordPermission'"));

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
            Verifier.Diagnostic("DSP0009")
                .WithLocation(7, 9)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessage("Use 'DiscordPermissions' instead of operating on 'DiscordPermission'"));

        await test.RunAsync();
    }

    [Test]
    public static async Task AndNotAssignmentTestAsync()
    {
        CSharpAnalyzerTest<UseDiscordPermissionsAnalyzer, DefaultVerifier> test =
            Utility.CreateAnalyzerTest<UseDiscordPermissionsAnalyzer>();

        test.TestCode = """
                        using DSharpPlus.Entities;

                        public class PermissionsUtil
                        {
                            public static void AddAdmin(DiscordPermission perm) 
                            {
                                perm &= ~DiscordPermission.Administrator;
                            }
                        }
                        """;

        test.ExpectedDiagnostics.Add(
            Verifier.Diagnostic("DSP0009")
                .WithLocation(7, 9)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessage("Use 'DiscordPermissions' instead of operating on 'DiscordPermission'"));

        await test.RunAsync();
    }

    [Test]
    public static async Task AndNotParenthesizedAssignmentTestAsync()
    {
        CSharpAnalyzerTest<UseDiscordPermissionsAnalyzer, DefaultVerifier> test =
            Utility.CreateAnalyzerTest<UseDiscordPermissionsAnalyzer>();

        test.TestCode = """
                        using DSharpPlus.Entities;

                        public class PermissionsUtil
                        {
                            public static void AddAdmin(DiscordPermission perm) 
                            {
                                perm &= (~DiscordPermission.Administrator);
                            }
                        }
                        """;

        test.ExpectedDiagnostics.Add(
            Verifier.Diagnostic("DSP0009")
                .WithLocation(7, 9)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessage("Use 'DiscordPermissions' instead of operating on 'DiscordPermission'"));

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
            Verifier.Diagnostic("DSP0009")
                .WithLocation(7, 16)
                .WithSeverity(DiagnosticSeverity.Warning)
                .WithMessage("Use 'DiscordPermissions' instead of operating on 'DiscordPermission'"));

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
            Verifier.Diagnostic("DSP0010")
                .WithLocation(7, 9)
                .WithSeverity(DiagnosticSeverity.Info)
                .WithMessage("Prefer using '+' instead of '|'"));

        await test.RunAsync();
    }

    [Test]
    public static async Task UsingBothAssignmentTestAsync()
    {
        CSharpAnalyzerTest<UseDiscordPermissionsAnalyzer, DefaultVerifier> test =
            Utility.CreateAnalyzerTest<UseDiscordPermissionsAnalyzer>();

        test.TestCode = """
                        using DSharpPlus.Entities;

                        public class PermissionsUtil
                        {
                            public static void AddAdmin(DiscordPermissions perm) 
                            {
                                perm |= DiscordPermission.Administrator;
                            }
                        }
                        """;

        test.ExpectedDiagnostics.Add(
            Verifier.Diagnostic("DSP0010")
                .WithLocation(7, 9)
                .WithSeverity(DiagnosticSeverity.Info)
                .WithMessage("Prefer using '+=' instead of '|='"));

        await test.RunAsync();
    }

    [Test]
    public static async Task UsingOnlyDiscordPermissionsTestAsync()
    {
        CSharpAnalyzerTest<UseDiscordPermissionsAnalyzer, DefaultVerifier> test =
            Utility.CreateAnalyzerTest<UseDiscordPermissionsAnalyzer>();

        test.TestCode = """
                        using DSharpPlus.Entities;

                        public class PermissionsUtil
                        {
                            public static void AddAdmin(DiscordPermissions perm1, DiscordPermissions perm2) 
                            {
                                perm1 |= perm2;
                            }
                        }
                        """;

        test.ExpectedDiagnostics.Add(
            Verifier.Diagnostic("DSP0010")
                .WithLocation(7, 9)
                .WithSeverity(DiagnosticSeverity.Info)
                .WithMessage("Prefer using '+=' instead of '|='"));

        await test.RunAsync();
    }

    [Test]
    public static async Task PlusAssignmentDiscordPermissionsTestAsync()
    {
        CSharpAnalyzerTest<UseDiscordPermissionsAnalyzer, DefaultVerifier> test =
            Utility.CreateAnalyzerTest<UseDiscordPermissionsAnalyzer>();

        test.TestCode = """
                        using DSharpPlus.Entities;

                        public class PermissionsUtil
                        {
                            public static void AddAdmin(DiscordPermissions perm) 
                            {
                                perm += DiscordPermission.Administrator;
                            }
                        }
                        """;

        await test.RunAsync();
    }

    [Test]
    public static async Task PlusExpressionDiscordPermissionsTestAsync()
    {
        CSharpAnalyzerTest<UseDiscordPermissionsAnalyzer, DefaultVerifier> test =
            Utility.CreateAnalyzerTest<UseDiscordPermissionsAnalyzer>();

        test.TestCode = """
                        using DSharpPlus.Entities;

                        public class PermissionsUtil
                        {
                            public static void AddAdmin(DiscordPermissions perm) 
                            {
                                perm = perm + DiscordPermission.Administrator;
                            }
                        }
                        """;

        await test.RunAsync();
    }
}
