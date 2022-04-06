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

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DSharpPlus.Analyzers.Core
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class OptionalAddJsonIgnoreAnalyzer : DiagnosticAnalyzer
    {
        // TODO: Use localizable strings.
        public const string Id = "DSP0001";
        internal const string Title = $"Optional<T> requires {nameof(JsonIgnoreAttribute)} with {nameof(JsonIgnoreCondition.WhenWritingDefault)} applied.";
        internal const string MessageFormat = $"Property {{0}} requires a {nameof(JsonIgnoreAttribute)}.";
        internal const string Description = $"Property {{0}} is of type {{1}} and does not have a {nameof(JsonIgnoreAttribute)}. The {nameof(JsonIgnoreAttribute)} is required for Optional<T> properties so that System.Text.Json (STJ) can correctly serialize or skip over the property depending on the value, as the Discord API requires.";
        internal const string Category = "DSharpPlus.Analyzers.Core";
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(new DiagnosticDescriptor(Id, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description));

        // Avoids having to call typeof(T) every time we do a type comparison. I believe this is faster, despite the slight overhead.
        private static readonly Type OptionalType = typeof(DSharpPlus.Core.Entities.Optional<>);
        private static readonly Type JsonIgnoreAttributeType = typeof(JsonIgnoreAttribute);
        private static readonly Type JsonIgnoreConditionType = typeof(JsonIgnoreCondition);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze); // This should analyze generated code and automatically apply code fixes defined by other analyzers.
            context.RegisterSymbolAction(TryWarningMissingJsonIgnoreAttribute, SymbolKind.Property);
        }

        internal void TryWarningMissingJsonIgnoreAttribute(SymbolAnalysisContext symbolAnalysisContext)
        {
            IPropertySymbol property = (IPropertySymbol)symbolAnalysisContext.Symbol;
            if (!IsSameType(property.Type, OptionalType) // Make sure the property is Optional<T>
                || property.IsStatic // Static properties don't get serialized by STJ
                || (property.DeclaredAccessibility != Accessibility.Internal && property.DeclaredAccessibility != Accessibility.Public)) // Only public/internal properties get serialized by STJ
            {
                return;
            }

            if (property.GetAttributes().Any(x => IsSameType(x.AttributeClass, JsonIgnoreAttributeType) && x.NamedArguments.Any(y => IsSameType(y.Value.Type, JsonIgnoreConditionType) && y.Value.Value.Equals((int)JsonIgnoreCondition.WhenWritingDefault))))
            {
                return; // It's valid!
            }

            // Yell at the user for missing a [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] on their public/internal Optional<T> property.
            // The JsonIgnore is required so that STJ skips the property when no value is set, just as the Discord API requires.
            symbolAnalysisContext.ReportDiagnostic(Diagnostic.Create(SupportedDiagnostics[0], property.Locations[0], property.Name, property.Type));
        }

        private static bool IsSameType(ITypeSymbol type1, Type type2) => GetNamespace(type1) == type2.FullName;
        private static string GetNamespace(INamespaceOrTypeSymbol? symbol, string? existingNamespace = null) => symbol == null ? string.Join('.', existingNamespace!.Split('.').Reverse().Where(x => !string.IsNullOrWhiteSpace(x))) : GetNamespace(symbol.ContainingNamespace, existingNamespace + '.' + symbol.MetadataName);
    }
}
