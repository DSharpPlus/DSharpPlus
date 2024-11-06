using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DSharpPlus.Analyzers.Commands;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RegisterNestedClassesAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "DSP1002";
    public const string Category = "Usage";

    private static readonly LocalizableString title = new LocalizableResourceString(
        nameof(Resources.DSP1002Title),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString description = new LocalizableResourceString(
        nameof(Resources.DSP1002Description),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString messageFormat = new LocalizableResourceString(
        nameof(Resources.DSP1002MessageFormat),
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
        helpLinkUri: $"{Utility.BaseDocsUrl}/articles/analyzers/rules.html#usage-warning-dsp1002"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

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

        if (memberAccess.Name.Identifier.ValueText != "AddCommands")
        {
            return;
        }

        TypeInfo typeInfo = ctx.SemanticModel.GetTypeInfo(memberAccess.Expression);
        if (!ctx.Compilation.CheckByName(typeInfo, "DSharpPlus.Commands.CommandsExtension"))
        {
            return;
        }

        SymbolInfo invocationSymbolInfo = ctx.SemanticModel.GetSymbolInfo(invocation);
        if (invocationSymbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        INamedTypeSymbol? enumerableType
            = ctx.Compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1");
        INamedTypeSymbol? typeType = ctx.Compilation.GetTypeByMetadataName("System.Type");
        INamedTypeSymbol enumerableTypesType = enumerableType!.Construct(typeType!);
        
        bool isEnumerableTypesType
            = SymbolEqualityComparer.Default.Equals(methodSymbol.Parameters.FirstOrDefault()?.Type,
                enumerableTypesType);
        if (!isEnumerableTypesType)
        {
            return;
        }

        ArgumentSyntax? firstArgument = invocation.ArgumentList.Arguments.FirstOrDefault();
        if (firstArgument is null)
        {
            return;
        }

        List<TypeSyntax> typeInfos = GetTypesFromArgument(ctx, firstArgument.Expression);

        foreach (TypeSyntax type in typeInfos)
        {
            TypeInfo collectionTypeInfo = ctx.SemanticModel.GetTypeInfo(type);

            if (collectionTypeInfo.Type?.ContainingSymbol is INamedTypeSymbol)
            {
                Diagnostic diagnostic = Diagnostic.Create(rule,
                    type.GetLocation(),
                    collectionTypeInfo.Type.Name,
                    collectionTypeInfo.Type.ContainingSymbol.Name
                );
                ctx.ReportDiagnostic(diagnostic);
            }
        }
    }

    private List<TypeSyntax> GetTypesFromArgument(SyntaxNodeAnalysisContext ctx, ExpressionSyntax expression)
    {
        if (expression is CollectionExpressionSyntax collection)
        {
            List<TypeSyntax> predefinedTypes = [];
            foreach (CollectionElementSyntax elemenet in collection.Elements)
            {
                if (elemenet is ExpressionElementSyntax
                    {
                        Expression: TypeOfExpressionSyntax typeOf
                    })
                {
                    predefinedTypes.Add(typeOf.Type);
                }
                else if (elemenet is ExpressionElementSyntax expressionElement)
                {
                    TypeSyntax? syntax = GetTypeInfoFromVar(ctx, expressionElement.Expression);
                    if (syntax is null)
                    {
                        continue;
                    }

                    predefinedTypes.Add(syntax);
                }
            }

            return predefinedTypes;
        }
        else if (expression is IdentifierNameSyntax identifierName)
        {
            SyntaxNode? declaringSyntax = GetDeclaringSyntax(ctx, identifierName);

            InitializerExpressionSyntax? initializer = declaringSyntax switch
            {
                ObjectCreationExpressionSyntax o => o.Initializer,
                ImplicitObjectCreationExpressionSyntax io => io.Initializer,
                ArrayCreationExpressionSyntax a => a.Initializer,
                ImplicitArrayCreationExpressionSyntax ia => ia.Initializer,
                _ => null,
            };

            if (initializer is null)
            {
                return [];
            }

            List<TypeSyntax> types = [];
            foreach (ExpressionSyntax element in initializer.Expressions)
            {
                if (element is TypeOfExpressionSyntax typeOf)
                {
                    types.Add(typeOf.Type);
                }
                else
                {
                    TypeSyntax? typeSyntax = GetTypeInfoFromVar(ctx, element);
                    if (typeSyntax is not null)
                    {
                        types.Add(typeSyntax);
                    }
                }
            }

            return types;
        }

        return [];
    }

    private TypeSyntax? GetTypeInfoFromVar(SyntaxNodeAnalysisContext ctx, ExpressionSyntax expression)
    {
        if (expression is not IdentifierNameSyntax identifierName)
        {
            return null;
        }

        SyntaxNode? declaringSyntax = GetDeclaringSyntax(ctx, identifierName);
        if (declaringSyntax is not TypeOfExpressionSyntax typeOf)
        {
            return null;
        }

        return typeOf.Type;
    }

    private SyntaxNode? GetDeclaringSyntax(SyntaxNodeAnalysisContext ctx, IdentifierNameSyntax identifierName)
    {
        SymbolInfo symbolInfo = ctx.SemanticModel.GetSymbolInfo(identifierName);
        SyntaxNode declaringSyntax;
        if (symbolInfo.Symbol is IFieldSymbol field)
        {
            declaringSyntax = field.DeclaringSyntaxReferences.First().GetSyntax();
        }
        else if (symbolInfo.Symbol is ILocalSymbol local)
        {
            declaringSyntax = local.DeclaringSyntaxReferences.First().GetSyntax();
        }
        else
        {
            return null;
        }

        if (declaringSyntax is not VariableDeclaratorSyntax declarator)
        {
            return null;
        }

        return declarator.Initializer?.Value;
    }
}
