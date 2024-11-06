using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DSharpPlus.Analyzers.Commands;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ValidGuildInstallablesAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "DSP1001";
    public const string Category = "Usage";

    private static readonly LocalizableString title = new LocalizableResourceString
    (
        nameof(Resources.DSP1001Title),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString description = new LocalizableResourceString
    (
        nameof(Resources.DSP1001Description),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString messageFormat = new LocalizableResourceString
    (
        nameof(Resources.DSP1001MessageFormat),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly DiagnosticDescriptor rule = new
    (
        DiagnosticId,
        title,
        messageFormat,
        Category,
        DiagnosticSeverity.Error,
        true,
        description,
        helpLinkUri: $"{Utility.BaseDocsUrl}/articles/analyzers/rules.html#usage-error-dsp1001"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

    public override void Initialize(AnalysisContext ctx)
    {
        ctx.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        ctx.EnableConcurrentExecution();
        ctx.RegisterSyntaxNodeAction(Analyze, SyntaxKind.MethodDeclaration);
    }
    
    private void Analyze(SyntaxNodeAnalysisContext ctx)
    {
        if (ctx.Node is not MethodDeclarationSyntax methodDecl)
        {
            return;
        }

        IEnumerable<AttributeSyntax> attributes = methodDecl.AttributeLists.SelectMany(a => a.Attributes);

        AttributeSyntax? registerToGuild = null;
        AttributeSyntax? interactionInstallType = null;
        foreach (AttributeSyntax attributeSyntax in attributes)
        {
            TypeInfo typeInfo = ctx.SemanticModel.GetTypeInfo(attributeSyntax);
            if (ctx.Compilation.CheckByName(typeInfo, "DSharpPlus.Commands.RegisterToGuildsAttribute"))
            {
                registerToGuild = attributeSyntax;
            }
            else if (ctx.Compilation.CheckByName(typeInfo,
                         "DSharpPlus.Commands.Processors.SlashCommands.Metadata.InteractionInstallTypeAttribute"))

            {
                interactionInstallType = attributeSyntax;
            }
        }

        if (registerToGuild is null || interactionInstallType is null)
        {
            return;
        }

        bool hasGuildInstall = false;
        foreach (AttributeArgumentSyntax a in interactionInstallType.ArgumentList?.Arguments ?? [])
        {
            Optional<object?> constantValue = ctx.SemanticModel.GetConstantValue(a.Expression);
            if (constantValue.HasValue)
            {
                if (constantValue.Value is not null && (int)constantValue.Value == 0)
                {
                    hasGuildInstall = true;
                    break;
                }
            }
        }

        if (hasGuildInstall)
        {
            return;
        }

        Diagnostic diagnostic = Diagnostic.Create(
            rule,
            methodDecl.Identifier.GetLocation(),
            methodDecl.Identifier
        );
        ctx.ReportDiagnostic(diagnostic);
    }
}
