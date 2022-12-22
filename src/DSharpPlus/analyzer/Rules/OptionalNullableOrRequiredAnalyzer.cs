using System.Collections.Immutable;
using System.Linq;

using DSharpPlus.Analyzers.Extensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DSharpPlus.Analyzers.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OptionalNullableOrRequiredAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor = new
    (
        AnalyzerIds.OptionalNullableOrRequiredAnalyzer,
        title: "A property inside the internal DTOs must be optional, nullable or required",
        messageFormat: "A property inside the internal DTOs must be optional, nullable or required",
        AnalyzerCategories.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSymbolAction(AnalyzePropertyDeclaration, SymbolKind.Property);
    }

    private void AnalyzePropertyDeclaration(SymbolAnalysisContext context)
    {
        IPropertySymbol declaration = (context.Symbol as IPropertySymbol)!;

        // only apply to internal DTOs
        if(declaration.ContainingType.ContainingNamespace.GetFullNamespace() != "DSharpPlus.Entities.Internal")
        {
            return;
        }

        if (declaration.IsRequired)
        {
            return;
        }

        if (declaration.NullableAnnotation == NullableAnnotation.Annotated)
        {
            return;
        }

        // check for our own optional type
        if (declaration.Type is INamedTypeSymbol { Arity: 1, MetadataName: "Optional`1" } optionalType
            && optionalType.ContainingNamespace.GetFullNamespace() == "DSharpPlus.Entities")
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(descriptor, context.Symbol.Locations.First()));
    }
}
