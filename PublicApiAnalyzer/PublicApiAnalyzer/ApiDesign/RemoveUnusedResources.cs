// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace PublicApiAnalyzer.ApiDesign
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    internal sealed partial class RemoveUnusedResources : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor RemoveUnusedResourcesDiagnostic = new DiagnosticDescriptor(
            id: RoslynDiagnosticIds.RemoveUnusedResources,
            title: "Remove unused resources",
            messageFormat: "Unused resource",
            category: AnalyzerCategory.ApiDesign,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: null,
            helpLinkUri: $"https://github.com/DotNetAnalyzers/PublicApiAnalyzer/blob/master/documentation/{RoslynDiagnosticIds.RemoveUnusedResources}.md",
            customTags: new string[0]);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(RemoveUnusedResourcesDiagnostic);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(HandleNamedType, SymbolKind.NamedType);
        }

        private static void HandleNamedType(SymbolAnalysisContext context)
        {
            INamedTypeSymbol namedType = (INamedTypeSymbol)context.Symbol;
            if (namedType.TypeKind != TypeKind.Class)
            {
                return;
            }

            if (!namedType.Locations[0].IsInSource)
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(RemoveUnusedResourcesDiagnostic, namedType.Locations[0]));
        }
    }
}
