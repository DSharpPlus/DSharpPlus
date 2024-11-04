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

    private static readonly IReadOnlyDictionary<string, HashSet<string>> allowedContexts
        = new Dictionary<string, HashSet<string>>()
        {
            {
                "DSharpPlus.Commands.Processors.TextCommands.CommandContext",
                ["UserCommandProcessor", "SlashCommandProcessor", "TextCommandProcessor"]
            },
            {
                "DSharpPlus.Commands.Processors.SlashCommands.SlashCommandContext",
                ["UserCommandProcessor", "SlashCommandProcessor"]
            },
            { "DSharpPlus.Commands.Processors.UserCommands.UserCommandContext", ["UserCommandProcessor"] },
            { "DSharpPlus.Commands.Processors.TextCommands.TextCommandContext", ["TextCommandProcessor"] },
        };

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

        List<ITypeSymbol> types = [];
        foreach (AttributeSyntax attribute in attributes)
        {
            TypeInfo typeInfo = ctx.SemanticModel.GetTypeInfo(attribute);

            if (typeInfo.Type is not INamedTypeSymbol namedTypeSymbol)
            {
                continue;
            }

            if (namedTypeSymbol.Name != "AllowedProcessorsAttribute")
            {
                continue;
            }

            if (namedTypeSymbol.ContainingNamespace.ToDisplayString() != "DSharpPlus.Commands.Trees.Metadata")
            {
                continue;
            }

            foreach (ITypeSymbol typeArgument in namedTypeSymbol.TypeArguments)
            {
                types.Add(typeArgument);
            }

            break;
        }

        if (types.Count <= 0)
        {
            return;
        }

        if (methodDecl.ParameterList.Parameters.Count <= 0)
        {
            return;
        }

        ParameterSyntax contextParam = methodDecl.ParameterList.Parameters.First();
        TypeInfo contextType = ctx.SemanticModel.GetTypeInfo(contextParam.Type!);

        if (contextType.Type is null)
        {
            return;
        }

        if (!allowedContexts.TryGetValue(
                $"{contextType.Type.ContainingNamespace.ToDisplayString()}.{contextType.Type.MetadataName}",
                out HashSet<string>? set))
        {
            return;
        }

        bool containsAnyProcessor = false;
        foreach (ITypeSymbol? t in types)
        {
            if (set.Contains(t.Name))
            {
                containsAnyProcessor = true;
                break;
            }
        }

        if (containsAnyProcessor)
        {
            return;
        }

        Diagnostic diagnostic = Diagnostic.Create(rule,
            contextParam.Type?.GetLocation(),
            contextType.Type.Name
        );
        ctx.ReportDiagnostic(diagnostic);
    }
}
