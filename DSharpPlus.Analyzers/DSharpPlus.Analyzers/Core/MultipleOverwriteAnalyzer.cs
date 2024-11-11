using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DSharpPlus.Analyzers.Core;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MultipleOverwriteAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "DSP0006";
    public const string Category = "Design";

    private static readonly LocalizableString title = new LocalizableResourceString
    (
        nameof(Resources.DSP0006Title),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString description = new LocalizableResourceString
    (
        nameof(Resources.DSP0006Description),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString messageFormat = new LocalizableResourceString
    (
        nameof(Resources.DSP0006MessageFormat),
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
        helpLinkUri: $"{Utility.BaseDocsUrl}/articles/analyzers/commands.html#usage-error-dsp1001"
    );

    // This might need to be a concurrent dictionary cause of line 51
    private readonly Dictionary<MethodDeclarationSyntax, HashSet<string>> invocations = new();

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

    public override void Initialize(AnalysisContext ctx)
    {
        ctx.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        ctx.EnableConcurrentExecution();
        ctx.RegisterSyntaxNodeAction(Analyze, SyntaxKind.InvocationExpression);
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
        if (!ctx.Compilation.CheckByName(typeInfo, "DSharpPlus.Entities.DiscordChannel"))
        {
            return;
        }

        MethodDeclarationSyntax? method = FindMethodDecl(invocation);
        if (method is null)
        {
            return;
        }

        string memberText = memberAccess.GetText().ToString();
        bool isInLoop = IsInLoop(invocation);
        if (!this.invocations.TryGetValue(method, out HashSet<string> hashSet))
        {
            this.invocations.Add(method, [memberText]);
            if (isInLoop)
            {
                Diagnostic loopDiagnostic = Diagnostic.Create(
                    rule,
                    invocation.GetLocation(),
                    memberAccess.Expression
                );

                ctx.ReportDiagnostic(loopDiagnostic);
            }

            return;
        }

        if (hashSet.Add(memberText))
        {
            if (isInLoop)
            {
                Diagnostic loopDiagnostic = Diagnostic.Create(
                    rule,
                    invocation.GetLocation(),
                    memberAccess.Expression
                );

                ctx.ReportDiagnostic(loopDiagnostic);
            }
            
            return;
        }

        Diagnostic diagnostic = Diagnostic.Create(
            rule,
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

        return FindMethodDecl(syntax.Parent);
    }

    private bool IsInLoop(SyntaxNode? syntax)
    {
        if (syntax is null)
        {
            return false;
        }

        if (syntax is ForEachStatementSyntax)
        {
            return true;
        }

        if (syntax is ForStatementSyntax)
        {
            return true;
        }

        if (syntax is WhileStatementSyntax)
        {
            return true;
        }
        
        return IsInLoop(syntax.Parent);
    }
}
