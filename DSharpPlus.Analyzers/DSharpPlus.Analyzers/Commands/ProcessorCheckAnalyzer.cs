using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DSharpPlus.Analyzers.Commands;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ProcessorCheckAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "DSP1003";
    public const string Category = "Usage";

    private static readonly LocalizableString title = new LocalizableResourceString(
        nameof(Resources.DSP1003Title),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString description = new LocalizableResourceString(
        nameof(Resources.DSP1003Description),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString messageFormat = new LocalizableResourceString(
        nameof(Resources.DSP1003MessageFormat),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly DiagnosticDescriptor rule = new DiagnosticDescriptor(
        DiagnosticId,
        title,
        messageFormat,
        Category,
        DiagnosticSeverity.Error,
        true,
        description
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

    public override void Initialize(AnalysisContext ctx)
    {
        ctx.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        ctx.EnableConcurrentExecution();
        ctx.RegisterSyntaxNodeAction(Analyze, SyntaxKind.MethodDeclaration);
    }

    public void Analyze(SyntaxNodeAnalysisContext ctx)
    {
        if (ctx.Node is not MethodDeclarationSyntax methodDecl)
        {
            return;
        }

        IEnumerable<AttributeSyntax> attributes = methodDecl.AttributeLists.SelectMany(al => al.Attributes);
        foreach (AttributeSyntax attribute in attributes)
        {
            TypeInfo typeInfo = ctx.SemanticModel.GetTypeInfo(attribute);
            typeInfo.ToString();
        }
    }
}
