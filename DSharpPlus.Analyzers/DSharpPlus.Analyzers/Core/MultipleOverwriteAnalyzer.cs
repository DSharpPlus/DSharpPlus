using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DSharpPlus.Analyzers.Core;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MultipleOverwriteAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "DSP0007";
    public const string Category = "Design";

    private static readonly LocalizableString title = new LocalizableResourceString(
        nameof(Resources.DSP0007Title),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString description = new LocalizableResourceString(
        nameof(Resources.DSP0007Description),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString messageFormat = new LocalizableResourceString(
        nameof(Resources.DSP0007MessageFormat),
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
        helpLinkUri: $"{Utility.BaseDocsUrl}/articles/analyzers/core.html#usage-error-dsp0007"
    );


    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

    public override void Initialize(AnalysisContext ctx)
    {
        ctx.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        ctx.EnableConcurrentExecution();
        ctx.RegisterCompilationStartAction(CompilationStart);
    }

    private void CompilationStart(CompilationStartAnalysisContext ctx)
    {
        ConcurrentDictionary<BlockSyntax, HashSet<ISymbol>> invocations = new();

        ctx.RegisterSyntaxNodeAction((c) => Analyze(c, invocations), SyntaxKind.InvocationExpression);
    }

    private void Analyze(SyntaxNodeAnalysisContext ctx, ConcurrentDictionary<BlockSyntax, HashSet<ISymbol>> invocations)
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

        SymbolInfo symbolInfo = ctx.SemanticModel.GetSymbolInfo(memberAccess.Expression);
        if (symbolInfo.Symbol is null)
        {
            return;
        }

        BlockSyntax? block = FindBlock(invocation);
        if (block is null)
        {
            return;
        }

        bool recursivelyCalled = false;


        ISymbol? localSymbol = symbolInfo.Symbol as ILocalSymbol ??
                               symbolInfo.Symbol as IParameterSymbol ??
                               (ISymbol?)FindLocalSymbol(memberAccess.Expression, ctx.SemanticModel);

        SyntaxReference? declaringSyntax = symbolInfo.Symbol.DeclaringSyntaxReferences.FirstOrDefault();
        if (IsInLoop(invocation) && declaringSyntax is not null && localSymbol is not null)
        {
            if (block.Parent is ForEachStatementSyntax forEachStmnt)
            {
                recursivelyCalled = forEachStmnt != declaringSyntax.GetSyntax();
            }
            else
            {
                DataFlowAnalysis? dataFlow = ctx.SemanticModel.AnalyzeDataFlow(block);
                if (dataFlow is not null)
                {
                    recursivelyCalled =
                        !dataFlow.VariablesDeclared
                            .Any(v => SymbolEqualityComparer.Default.Equals(symbolInfo.Symbol, v));
                }
            }
        }

        BlockSyntax? previousBlock = FindBlock(block.Parent);
        while (previousBlock is not null)
        {
            if (invocations.TryGetValue(previousBlock, out HashSet<ISymbol> previousBlockSet) &&
                previousBlockSet.Contains(symbolInfo.Symbol))
            {
                Diagnostic previousInvocationDiagnostic = Diagnostic.Create(
                    rule,
                    invocation.GetLocation(),
                    memberAccess.Expression
                );

                ctx.ReportDiagnostic(previousInvocationDiagnostic);
                break;
            }

            previousBlock = FindBlock(previousBlock.Parent);
        }

        if (!invocations.TryGetValue(block, out HashSet<ISymbol> hashSet))
        {
            HashSet<ISymbol> newSet = new([symbolInfo.Symbol], SymbolEqualityComparer.Default);

            invocations.TryAdd(block, newSet);
            if (recursivelyCalled)
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

        if (hashSet.Add(symbolInfo.Symbol))
        {
            if (recursivelyCalled)
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

    private BlockSyntax? FindBlock(SyntaxNode? syntax)
    {
        if (syntax is null)
        {
            return null;
        }

        if (syntax is BlockSyntax block)
        {
            return block;
        }

        return FindBlock(syntax.Parent);
    }

    private ILocalSymbol? FindLocalSymbol(ExpressionSyntax? expression, SemanticModel model)
    {
        if (expression is null)
        {
            return null;
        }

        SymbolInfo symbolInfo = model.GetSymbolInfo(expression);
        if (symbolInfo.Symbol is ILocalSymbol localSymbol)
        {
            return localSymbol;
        }

        if (expression.Parent is ExpressionSyntax parentExpression)
        {
            return FindLocalSymbol(parentExpression, model);
        }

        return null;
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
