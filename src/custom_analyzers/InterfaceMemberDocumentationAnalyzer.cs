using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CustomAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class InterfaceMemberDocumentationAnalyzer : DiagnosticAnalyzer
{
    public const string DIAGNOSTIC_ID = "IDA001";
    private const string CATEGORY = "Documentation";

    private static readonly LocalizableString Title = "Interface member missing XML documentation";
    private static readonly LocalizableString MessageFormat = "Interface member '{0}' is missing XML documentation";
    private static readonly LocalizableString Description = "All interface members should have XML documentation comments.";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DIAGNOSTIC_ID,
        Title,
        MessageFormat,
        CATEGORY,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(
            AnalyzeSymbol,
            SymbolKind.Method,
            SymbolKind.Property,
            SymbolKind.Event);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var symbol = context.Symbol;

        // Check if the member is declared in an interface
        if (symbol.ContainingType?.TypeKind != TypeKind.Interface)
        {
            return;
        }

        // Check if the member is public (interfaces are public by default)
        if (symbol.DeclaredAccessibility != Accessibility.Public)
        {
            return;
        }

        // Check if XML documentation exists
        var xmlComment = symbol.GetDocumentationCommentXml();

        if (string.IsNullOrWhiteSpace(xmlComment))
        {
            var diagnostic = Diagnostic.Create(Rule, symbol.Locations[0], symbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}