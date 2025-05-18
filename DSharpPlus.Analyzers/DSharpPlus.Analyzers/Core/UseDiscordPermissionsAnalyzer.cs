using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DSharpPlus.Analyzers.Core;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseDiscordPermissionsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString titleDsp0009 = new LocalizableResourceString
    (
        nameof(Resources.DSP0009Title),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString descriptionDsp0009 = new LocalizableResourceString
    (
        nameof(Resources.DSP0009Description),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString messageFormatDsp0009 = new LocalizableResourceString
    (
        nameof(Resources.DSP0009MessageFormat),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString titleDsp0010 = new LocalizableResourceString
    (
        nameof(Resources.DSP0010Title),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString descriptionDsp0010 = new LocalizableResourceString
    (
        nameof(Resources.DSP0010Description),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString messageFormatDsp0010 = new LocalizableResourceString
    (
        nameof(Resources.DSP0010MessageFormat),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly DiagnosticDescriptor ruleDsp0009 = new(
        "DSP0009",
        titleDsp0009,
        messageFormatDsp0009,
        "Usage",
        DiagnosticSeverity.Warning,
        true,
        descriptionDsp0009,
        helpLinkUri: $"{Utility.BaseDocsUrl}/articles/analyzers/core.html#usage-warning-dsp0009"
    );

    private static readonly DiagnosticDescriptor ruleDsp0010 = new(
        "DSP0010",
        titleDsp0010,
        messageFormatDsp0010,
        "Usage",
        DiagnosticSeverity.Info,
        true,
        descriptionDsp0010,
        helpLinkUri: $"{Utility.BaseDocsUrl}/articles/analyzers/core.html#usage-warning-dsp0010"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
        = ImmutableArray.Create(ruleDsp0009, ruleDsp0010);

    public override void Initialize(AnalysisContext ctx)
    {
        ctx.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        ctx.EnableConcurrentExecution();
        ctx.RegisterSyntaxNodeAction(
            AnalyzeBitOp,
            SyntaxKind.BitwiseAndExpression,
            SyntaxKind.BitwiseOrExpression,
            SyntaxKind.ExclusiveOrExpression,
            SyntaxKind.AddExpression,
            SyntaxKind.SubtractExpression,
            SyntaxKind.MultiplyExpression,
            SyntaxKind.DivideExpression,
            SyntaxKind.ModuloExpression,
            SyntaxKind.LeftShiftExpression,
            SyntaxKind.RightShiftAssignmentExpression,
            SyntaxKind.UnsignedRightShiftExpression);
        ctx.RegisterSyntaxNodeAction(
            AnalyzeAssignment,
            SyntaxKind.AndAssignmentExpression,
            SyntaxKind.OrAssignmentExpression,
            SyntaxKind.ExclusiveOrAssignmentExpression,
            SyntaxKind.AddAssignmentExpression,
            SyntaxKind.SubtractAssignmentExpression,
            SyntaxKind.MultiplyAssignmentExpression,
            SyntaxKind.DivideAssignmentExpression,
            SyntaxKind.ModuloAssignmentExpression,
            SyntaxKind.LeftShiftAssignmentExpression,
            SyntaxKind.RightShiftAssignmentExpression,
            SyntaxKind.UnsignedRightShiftAssignmentExpression);
    }

    private static void AnalyzeBitOp(SyntaxNodeAnalysisContext ctx)
    {
        if (ctx.Node is not BinaryExpressionSyntax binaryExpression)
        {
            return;
        }

        Location location;
        if (binaryExpression.Parent is AssignmentExpressionSyntax assignment)
        {
            location = assignment.GetLocation();
        }
        else
        {
            location = binaryExpression.GetLocation();
        }

        TypeInfo leftTypeInfo = ctx.SemanticModel.GetTypeInfo(binaryExpression.Left);
        TypeInfo rightTypeInfo = ctx.SemanticModel.GetTypeInfo(binaryExpression.Right);

        bool leftTypeIsDiscordPermission
            = ctx.Compilation.CheckByName(leftTypeInfo, "DSharpPlus.Entities.DiscordPermission");
        bool rightTypeIsDiscordPermission
            = ctx.Compilation.CheckByName(rightTypeInfo, "DSharpPlus.Entities.DiscordPermission");
        bool leftTypeIsDiscordPermissions
            = ctx.Compilation.CheckByName(leftTypeInfo, "DSharpPlus.Entities.DiscordPermissions");
        bool rightTypeIsDiscordPermissions
            = ctx.Compilation.CheckByName(rightTypeInfo, "DSharpPlus.Entities.DiscordPermissions");

        Diagnostic diagnostic;
        if (leftTypeIsDiscordPermissions || rightTypeIsDiscordPermissions)
        {
            if (binaryExpression.Kind() != SyntaxKind.BitwiseAndExpression &&
                binaryExpression.Kind() != SyntaxKind.BitwiseOrExpression &&
                binaryExpression.Kind() != SyntaxKind.ExclusiveOrExpression)
            {
                return;
            }

            if (binaryExpression.Kind() == SyntaxKind.AndAssignmentExpression &&
                !GetNotOperation(binaryExpression.Right))
            {
                return;
            }

            string equivalence = GetDiscordPermissionsEquivalence(binaryExpression.Kind());
            diagnostic = Diagnostic.Create(ruleDsp0010, location, equivalence,
                binaryExpression.OperatorToken.ToString());
            ctx.ReportDiagnostic(diagnostic);
            return;
        }

        if (!leftTypeIsDiscordPermission && !rightTypeIsDiscordPermission)
        {
            return;
        }

        diagnostic
            = Diagnostic.Create(ruleDsp0009, location);
        ctx.ReportDiagnostic(diagnostic);
    }

    private static void AnalyzeAssignment(SyntaxNodeAnalysisContext ctx)
    {
        if (ctx.Node is not AssignmentExpressionSyntax assignmentExpression)
        {
            return;
        }

        TypeInfo leftTypeInfo = ctx.SemanticModel.GetTypeInfo(assignmentExpression.Left);
        TypeInfo rightTypeInfo = ctx.SemanticModel.GetTypeInfo(assignmentExpression.Right);

        bool leftTypeIsDiscordPermission
            = ctx.Compilation.CheckByName(leftTypeInfo, "DSharpPlus.Entities.DiscordPermission");
        bool rightTypeIsDiscordPermission
            = ctx.Compilation.CheckByName(rightTypeInfo, "DSharpPlus.Entities.DiscordPermission");
        bool leftTypeIsDiscordPermissions
            = ctx.Compilation.CheckByName(leftTypeInfo, "DSharpPlus.Entities.DiscordPermissions");
        bool rightTypeIsDiscordPermissions
            = ctx.Compilation.CheckByName(rightTypeInfo, "DSharpPlus.Entities.DiscordPermissions");

        Diagnostic diagnostic;
        if (leftTypeIsDiscordPermissions || rightTypeIsDiscordPermissions)
        {
            if (assignmentExpression.Kind() != SyntaxKind.AndAssignmentExpression &&
                assignmentExpression.Kind() != SyntaxKind.OrAssignmentExpression &&
                assignmentExpression.Kind() != SyntaxKind.ExclusiveOrAssignmentExpression)
            {
                return;
            }

            if (assignmentExpression.Kind() == SyntaxKind.AndAssignmentExpression &&
                !GetNotOperation(assignmentExpression.Right))
            {
                return;
            }

            string equivalence = GetDiscordPermissionsEquivalence(assignmentExpression.Kind());
            diagnostic = Diagnostic.Create(ruleDsp0010, assignmentExpression.GetLocation(), equivalence,
                assignmentExpression.OperatorToken.ToString());
            ctx.ReportDiagnostic(diagnostic);
            return;
        }

        if (!leftTypeIsDiscordPermission &&
            !rightTypeIsDiscordPermission)
        {
            return;
        }

        diagnostic = Diagnostic.Create(ruleDsp0009, assignmentExpression.GetLocation());
        ctx.ReportDiagnostic(diagnostic);
    }

    private static bool GetNotOperation(ExpressionSyntax expression)
    {
        return expression switch
        {
            ParenthesizedExpressionSyntax { Expression: PrefixUnaryExpressionSyntax unaryExpression } =>
                unaryExpression.Kind() == SyntaxKind.BitwiseNotExpression,
            PrefixUnaryExpressionSyntax unaryExpression2 => unaryExpression2.Kind() == SyntaxKind.BitwiseNotExpression,
            _ => false
        };
    }

    private static string GetDiscordPermissionsEquivalence(SyntaxKind syntaxKind)
    {
        return syntaxKind switch
        {
            SyntaxKind.BitwiseAndExpression => "-",
            SyntaxKind.BitwiseOrExpression => "+",
            SyntaxKind.ExclusiveOrExpression => "DiscordPermissions#Toggle",
            SyntaxKind.AndAssignmentExpression => "-=",
            SyntaxKind.OrAssignmentExpression => "+=",
            SyntaxKind.ExclusiveOrAssignmentExpression => "DiscordPermissions#Toggle",
            _ => throw new ArgumentException("Syntax kind does not have a DiscordPermissions equivalence")
        };
    }
}
