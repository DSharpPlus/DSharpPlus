namespace DSharpPlus.Analyzers;

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MultipleOverwriteAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "DSP0002";
    public const string Category = "Usage";

    private static readonly LocalizableString Title = new LocalizableResourceString(
        nameof(Resources.DSP0002Title),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString Description = new LocalizableResourceString(
        nameof(Resources.DSP0002Description),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
        nameof(Resources.DSP0002MessageFormat),
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

    private readonly Dictionary<MethodDeclarationSyntax, HashSet<string>> _invocations = new();

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext ctx)
    {
        ctx.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        ctx.EnableConcurrentExecution();
        ctx.RegisterSyntaxNodeAction(this.Analyze, SyntaxKind.InvocationExpression);
    }

    private void Analyze(SyntaxNodeAnalysisContext ctx)
    {
        if (ctx.Node is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        if (memberAccess.Name.Identifier.ValueText != "AddOverwriteAsync")
        {
            return;
        }

        TypeInfo typeInfo = ctx.SemanticModel.GetTypeInfo(memberAccess.Expression);
        if (typeInfo.Type?.Name != "DiscordChannel" ||
            typeInfo.Type.ContainingNamespace.ToString() != "DSharpPlus.Entities")
        {
            return;
        }

        if (Utility.CheckIfSameTypeByNamespace(typeInfo, "DSharpPlus.Entities.DiscordChannel", ctx.Compilation))
        {
            return;
        }

        MethodDeclarationSyntax? method = this.FindMethodDecl(invocation);
        if (method is null)
        {
            return;
        }

        string memberText = memberAccess.GetText().ToString();
        if (!this._invocations.TryGetValue(method, out HashSet<string> hashSet))
        {
            this._invocations.Add(method, [memberText]);
            return;
        }

        if (hashSet.Add(memberText))
        {
            return;
        }

        Diagnostic diagnostic = Diagnostic.Create(
            Rule,
            invocation.GetLocation(),
            memberAccess.Expression
        );
        ctx.ReportDiagnostic(diagnostic);
    }

    private MethodDeclarationSyntax? FindMethodDecl(SyntaxNode? syntax)
    {
        if (syntax is null)
        {
            return null;
        }

        if (syntax is MethodDeclarationSyntax method)
        {
            return method;
        }

        return this.FindMethodDecl(syntax.Parent);
    }
}
