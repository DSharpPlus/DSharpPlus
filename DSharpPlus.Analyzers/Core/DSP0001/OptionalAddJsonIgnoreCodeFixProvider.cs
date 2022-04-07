// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace DSharpPlus.Analyzers.Core
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OptionalAddJsonIgnoreAnalyzer)), Shared]
    public sealed class OptionalAddJsonIgnoreCodeFixProvider : CodeFixProvider
    {
        internal const string CodeFixTitle = $"Add {nameof(JsonIgnoreAttribute)}";
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(OptionalAddJsonIgnoreAnalyzer.Id);
        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer; // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            Diagnostic diagnostic = context.Diagnostics[0];
            TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;
            PropertyDeclarationSyntax declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<PropertyDeclarationSyntax>().First();
            context.RegisterCodeFix(CodeAction.Create(
                title: CodeFixTitle,
                createChangedDocument: c => AddJsonIgnoreAttributeAsync(context.Document, declaration, c),
                equivalenceKey: CodeFixTitle), diagnostic);
        }

        private static async Task<Document> AddJsonIgnoreAttributeAsync(Document document, PropertyDeclarationSyntax propertyDeclarationSyntax, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
            Document newDocument = AddAttributes(document, root, propertyDeclarationSyntax);
            return newDocument.WithSyntaxRoot(await AddUsings((await newDocument.GetSyntaxRootAsync(cancellationToken)).SyntaxTree.GetCompilationUnitRoot(cancellationToken)).GetRootAsync(cancellationToken));
        }

        private static Document AddAttributes(Document document, SyntaxNode root, PropertyDeclarationSyntax propertyDeclarationSyntax) => document.WithSyntaxRoot(root.ReplaceNode(propertyDeclarationSyntax, propertyDeclarationSyntax.WithAttributeLists(
            SyntaxFactory.SingletonList(
                SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(
                            SyntaxFactory.IdentifierName("JsonIgnore"))
                        .WithArgumentList(
                            SyntaxFactory.AttributeArgumentList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.AttributeArgument(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.IdentifierName("JsonIgnoreCondition"),
                                            SyntaxFactory.IdentifierName("WhenWritingDefault")))
                                    .WithNameEquals(
                                        SyntaxFactory.NameEquals(
                                            SyntaxFactory.IdentifierName("Condition"))))))))))));

        private static SyntaxTree AddUsings(CompilationUnitSyntax root)
        {
            UsingDirectiveSyntax jsonSerializer = SyntaxFactory.UsingDirective(
                SyntaxFactory.QualifiedName(
                    SyntaxFactory.QualifiedName(
                        SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName("System"), SyntaxFactory.IdentifierName("Text")),
                    SyntaxFactory.IdentifierName("Json")),
                SyntaxFactory.IdentifierName("Serialization")));

            return root.Usings.Any(x => x.Name.ToString() == jsonSerializer.Name.ToString()) ? root.SyntaxTree : root.AddUsings(jsonSerializer).SyntaxTree;
        }
    }
}
