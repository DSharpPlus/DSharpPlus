using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DSharpPlus.Analyzers.Core;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseDiscordPermissionsAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "DSP0009";
    public const string Category = "Usage";

    private static readonly LocalizableString title = new LocalizableResourceString
    (
        nameof(Resources.DSP0009Title),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString description = new LocalizableResourceString
    (
        nameof(Resources.DSP0009Description),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString messageFormat = new LocalizableResourceString
    (
        nameof(Resources.DSP0009MessageFormat),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly DiagnosticDescriptor rule = new(
        DiagnosticId,
        title,
        messageFormat,
        Category,
        DiagnosticSeverity.Warning,
        true,
        description,
        helpLinkUri: $"{Utility.BaseDocsUrl}/articles/analyzers/core.html#usage-warning-dsp0009"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

    public override void Initialize(AnalysisContext ctx)
    {
        ctx.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        ctx.EnableConcurrentExecution();
        ctx.RegisterSyntaxNodeAction(
            AnalyzeBitOp,
            SyntaxKind.BitwiseAndExpression,
            SyntaxKind.BitwiseOrExpression,
            SyntaxKind.ExclusiveOrExpression);
        ctx.RegisterSyntaxNodeAction(
            AnalyzeAssignment,
            SyntaxKind.AndAssignmentExpression,
            SyntaxKind.OrAssignmentExpression,
            SyntaxKind.ExclusiveOrAssignmentExpression);
    }

    private static void AnalyzeBitOp(SyntaxNodeAnalysisContext ctx)
    {
        if (ctx.Node is not BinaryExpressionSyntax binaryExpression)
        {
            return;
        }

        if (binaryExpression.Kind() != SyntaxKind.BitwiseAndExpression &&
            binaryExpression.Kind() != SyntaxKind.BitwiseOrExpression &&
            binaryExpression.Kind() != SyntaxKind.ExclusiveOrExpression)
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
        if (!ctx.Compilation.CheckByName(leftTypeInfo, "DSharpPlus.Entities.DiscordPermission") &&
            !ctx.Compilation.CheckByName(rightTypeInfo, "DSharpPlus.Entities.DiscordPermission"))
        {
            return;
        }

        Diagnostic diagnostic
            = Diagnostic.Create(rule, location);
        ctx.ReportDiagnostic(diagnostic);
    }

    private static void AnalyzeAssignment(SyntaxNodeAnalysisContext ctx)
    {
        if (ctx.Node is not AssignmentExpressionSyntax assignmentExpression)
        {
            return;
        }

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


        TypeInfo leftTypeInfo = ctx.SemanticModel.GetTypeInfo(assignmentExpression.Left);
        TypeInfo rightTypeInfo = ctx.SemanticModel.GetTypeInfo(assignmentExpression.Right);
        if (!ctx.Compilation.CheckByName(leftTypeInfo, "DSharpPlus.Entities.DiscordPermission") &&
            !ctx.Compilation.CheckByName(rightTypeInfo, "DSharpPlus.Entities.DiscordPermission"))
        {
            return;
        }

        Diagnostic diagnostic = Diagnostic.Create(rule, assignmentExpression.GetLocation());
        ctx.ReportDiagnostic(diagnostic);
    }

    private static bool GetNotOperation(ExpressionSyntax expression)
    {
        if (expression is ParenthesizedExpressionSyntax { Expression: PrefixUnaryExpressionSyntax unaryExpression })
        {
            if (unaryExpression.Kind() == SyntaxKind.BitwiseNotExpression)
            {
                return true;
            }

            return false;
        }
        else if (expression is PrefixUnaryExpressionSyntax unaryExpression2)
        {
            if (unaryExpression2.Kind() == SyntaxKind.BitwiseNotExpression)
            {
                return true;
            }

            return false;
        }

        return false;
    }
}
