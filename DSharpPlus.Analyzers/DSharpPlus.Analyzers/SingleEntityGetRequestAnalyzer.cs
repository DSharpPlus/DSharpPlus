using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DSharpPlus.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SingleEntityGetRequestAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "DSP0007";
    public const string Category = "Usage";

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
        DiagnosticSeverity.Info,
        true,
        description
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

    private static readonly IReadOnlyDictionary<string, string> methods = new Dictionary<string, string>()
    {
        { "GetMessageAsync", "DSharpPlus.Entities.DiscordChannel" }
    };

    private static readonly IReadOnlyDictionary<string, string> preferedMethods = new Dictionary<string, string>()
    {
        { "GetMessageAsync", "GetMessagesAsync" }
    };

    public override void Initialize(AnalysisContext ctx)
    {
        ctx.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        ctx.EnableConcurrentExecution();
        ctx.RegisterSyntaxNodeAction(Analyze, SyntaxKind.InvocationExpression);
    }

    public void Analyze(SyntaxNodeAnalysisContext ctx)
    {
        if (ctx.Node is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }
        
        string methodName = memberAccess.Name.Identifier.ValueText;
        if (!methods.TryGetValue(methodName, out string? typeName))
        {
            return;
        }

        TypeInfo typeInfo = ctx.SemanticModel.GetTypeInfo(memberAccess.Expression);
        if (!ctx.Compilation.CheckByName(typeInfo, typeName))
        {
            return;
        }

        StatementSyntax? syntax = FindClosestLoopStatement(invocation);
        if (syntax is null)
        {
            return;
        }

        Diagnostic diagnostic = Diagnostic.Create(
            rule,
            invocation.GetLocation(),
            $"{memberAccess.Expression}.{preferedMethods[methodName]}()",
            invocation
        );
        
        ctx.ReportDiagnostic(diagnostic);
    }

    private StatementSyntax? FindClosestLoopStatement(SyntaxNode? syntax)
    {
        if (syntax is null)
        {
            return null;
        }

        if (syntax is ForEachStatementSyntax forEachStatement)
        {
            return forEachStatement.Statement;
        }

        if (syntax is ForStatementSyntax forStatement)
        {
            return forStatement.Statement;
        }

        return FindClosestLoopStatement(syntax.Parent);
    }
}
