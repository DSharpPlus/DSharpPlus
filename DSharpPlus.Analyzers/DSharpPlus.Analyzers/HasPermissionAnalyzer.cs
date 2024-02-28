namespace DSharpPlus.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class HasPermissionAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "DSP0001";

    public const string Category = "Usage";

    private static readonly LocalizableString Title = new LocalizableResourceString(
        nameof(Resources.DSP0001Title),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString Description = new LocalizableResourceString(
        nameof(Resources.DSP0001Description),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
        nameof(Resources.DSP0001MessageFormat),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        true,
        Description
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

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
        if (typeInfo.Type?.Name != "Permissions" || 
            Utility.CheckIfSameTypeByNamespace(typeInfo, "DSharpPlus.Permissions", ctx.Compilation))
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
            Rule,
            binaryExpression.GetLocation(),
            leftBinary.Left.GetText().ToString().Trim(),
            leftBinary.Right.GetText().ToString().Trim());
        ctx.ReportDiagnostic(diagnostic);
    }
}
