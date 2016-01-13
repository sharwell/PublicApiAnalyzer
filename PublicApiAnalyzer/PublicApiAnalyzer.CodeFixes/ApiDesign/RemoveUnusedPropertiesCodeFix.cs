// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace PublicApiAnalyzer.ApiDesign
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.FindSymbols;
    using Microsoft.CodeAnalysis.Text;

    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic, Name = "DeclarePublicAPIFix")]
    [Shared]
    internal class RemoveUnusedPropertiesCodeFix : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(RoslynDiagnosticIds.RemoveUnusedResources);

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Remove unused properties",
                        cancellationToken => this.GetFixAsync(context.Document, diagnostic, cancellationToken),
                        nameof(DeclarePublicAPIFix)),
                    diagnostic);
            }

            return SpecializedTasks.CompletedTask;
        }

        private async Task<Document> GetFixAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var symbol = (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(token.Parent, cancellationToken);

            List<TextChange> changes = new List<TextChange>();
            foreach (var member in symbol.GetMembers())
            {
                if (member.Kind != SymbolKind.Property)
                {
                    continue;
                }

                IEnumerable<ReferencedSymbol> references = await SymbolFinder.FindReferencesAsync(member, document.Project.Solution, cancellationToken).ConfigureAwait(false);
                if (references.Any(x => x.Locations.Any()))
                {
                    continue;
                }

                changes.Add(new TextChange(member.Locations[0].SourceSpan, "@" + member.Name));
            }

            SourceText text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            return document.WithText(text.WithChanges(changes));
        }
    }
}
