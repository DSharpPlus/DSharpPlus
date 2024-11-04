namespace DSharpPlus.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class HasPermissionAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "DSP0005";

    public const string Category = "Usage";

    private static readonly LocalizableString title = new LocalizableResourceString(
        nameof(Resources.DSP0005Title),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString description = new LocalizableResourceString(
        nameof(Resources.DSP0005Description),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString messageFormat = new LocalizableResourceString(
        nameof(Resources.DSP0005MessageFormat),
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
        description
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

    public override void Initialize(AnalysisContext ctx)
    {
        ctx.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        ctx.EnableConcurrentExecution();
        ctx.RegisterSyntaxNodeAction(Analyze, SyntaxKind.NotEqualsExpression);
    }

    private static void Analyze(SyntaxNodeAnalysisContext ctx)
    {
        if (ctx.Node is not BinaryExpressionSyntax binaryExpression)
        {
            return;
        }

        if (binaryExpression.Kind() != SyntaxKind.NotEqualsExpression)
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

        TypeInfo typeInfo = ctx.SemanticModel.GetTypeInfo(leftBinary.Left);
        if (typeInfo.Type?.Name != "DiscordPermissions" || 
            !ctx.Compilation.CheckByName(typeInfo, "DSharpPlus.Entities.DiscordPermissions"))
        {
            return;
        }

        if (binaryExpression.Right is not LiteralExpressionSyntax literal)
        {
            return;
        }

        if (literal.Kind() != SyntaxKind.NumericLiteralExpression)
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
