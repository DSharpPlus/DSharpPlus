using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DSharpPlus.Analyzers.Core;

// This should get redesigned when #2152 gets merged
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class HasPermissionAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "DSP0005";

    public const string Category = "Usage";

    private static readonly LocalizableString title = new LocalizableResourceString
    (
        nameof(Resources.DSP0005Title),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString description = new LocalizableResourceString
    (
        nameof(Resources.DSP0005Description),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString messageFormat = new LocalizableResourceString
    (
        nameof(Resources.DSP0005MessageFormat),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly DiagnosticDescriptor rule = new
    (
        DiagnosticId,
        title,
        messageFormat,
        Category,
        DiagnosticSeverity.Hidden,
        true,
        description,
        helpLinkUri: $"{Utility.BaseDocsUrl}/articles/analyzers/core.html#usage-hidden-dsp0005"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

    public override void Initialize(AnalysisContext ctx)
    {
        ctx.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        ctx.EnableConcurrentExecution();
        ctx.RegisterSyntaxNodeAction(Analyze, SyntaxKind.NotEqualsExpression, SyntaxKind.EqualsExpression);
    }

    private static void Analyze(SyntaxNodeAnalysisContext ctx)
    {
        if (ctx.Node is not BinaryExpressionSyntax binaryExpression)
        {
            return;
        }

        if (binaryExpression.Kind() != SyntaxKind.NotEqualsExpression &&
            binaryExpression.Kind() != SyntaxKind.EqualsExpression)
        {
            return;
        }

        if (binaryExpression.Left is not ParenthesizedExpressionSyntax p ||
            p.Expression is not BinaryExpressionSyntax leftBinary)
        {
            return;
        }

        if (leftBinary.Kind() != SyntaxKind.BitwiseAndExpression)
        {
            return;
        }

        TypeInfo leftTypeInfo = ctx.SemanticModel.GetTypeInfo(leftBinary.Left);
        if (!ctx.Compilation.CheckByName(leftTypeInfo, "DSharpPlus.Entities.DiscordPermission"))
        {
            return;
        }

        TypeInfo rightTypeInfo = ctx.SemanticModel.GetTypeInfo(leftBinary.Right);
        if (!ctx.Compilation.CheckByName(rightTypeInfo, "DSharpPlus.Entities.DiscordPermission"))
        {
            return;
        }

        Diagnostic diagnostic = Diagnostic.Create(
            rule,
            binaryExpression.GetLocation(),
            leftBinary.Left.GetText().ToString().Trim(),
            leftBinary.Right.GetText().ToString().Trim());
        ctx.ReportDiagnostic(diagnostic);
    }
}
