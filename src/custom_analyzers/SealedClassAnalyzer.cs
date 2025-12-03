using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CustomAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SealedClassAnalyzer : DiagnosticAnalyzer
{
    public const string DIAGNOSTIC_ID = "SCA001";
    private const string CATEGORY = "Design";

    private static readonly LocalizableString Title = "Class should be sealed";
    private static readonly LocalizableString MessageFormat = "Class '{0}' should be sealed unless it's designed for inheritance";
    private static readonly LocalizableString Description = "Classes should be sealed by default to prevent unintended inheritance. Mark the class as sealed, abstract, or add virtual/protected members if inheritance is intended.";

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

        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        // Only analyze classes
        if (namedTypeSymbol.TypeKind != TypeKind.Class)
        {
            return;
        }

        // Skip if already sealed
        if (namedTypeSymbol.IsSealed)
        {
            return;
        }

        // Skip if abstract (designed for inheritance)
        if (namedTypeSymbol.IsAbstract)
        {
            return;
        }

        // Skip if static
        if (namedTypeSymbol.IsStatic)
        {
            return;
        }

        // Skip if it's a record (records have different semantics)
        if (namedTypeSymbol.IsRecord)
        {
            return;
        }

        // Skip compiler-generated Program class from top-level statements
        if (namedTypeSymbol.Name == "Program" &&
            namedTypeSymbol.GetMembers().Any(m => m.Name == "<Main>$"))
        {
            return;
        }

        // Skip if class has protected or virtual members (designed for inheritance)
        if (HasProtectedOrVirtualMembers(namedTypeSymbol))
        {
            return;
        }

        // Skip if class has a non-private constructor with 'protected' accessibility
        // (indicates it's designed to be inherited)
        if (HasProtectedConstructors(namedTypeSymbol))
        {
            return;
        }

        // Skip if the class derives from anything other than System.Object
        // (it might be part of an inheritance hierarchy)
        if (namedTypeSymbol.BaseType != null && namedTypeSymbol.BaseType.SpecialType != SpecialType.System_Object)
        {
            return;
        }

        // Report diagnostic
        var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool HasProtectedOrVirtualMembers(INamedTypeSymbol classSymbol)
    {
        var members = classSymbol.GetMembers();

        foreach (var member in members)
        {
            // Check for virtual, abstract, or override members
            if (member.IsVirtual || member.IsAbstract || member.IsOverride)
            {
                return true;
            }

            // Check for protected or protected internal members (exclude compiler-generated)
            if ((member.DeclaredAccessibility == Accessibility.Protected ||
                 member.DeclaredAccessibility == Accessibility.ProtectedOrInternal) &&
                !member.IsImplicitlyDeclared)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasProtectedConstructors(INamedTypeSymbol classSymbol)
    {
        return classSymbol.Constructors.Any(constructor =>
            constructor.DeclaredAccessibility == Accessibility.Protected ||
            constructor.DeclaredAccessibility == Accessibility.ProtectedOrInternal);
    }
}
