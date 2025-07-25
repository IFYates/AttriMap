using Microsoft.CodeAnalysis;

namespace IFY.AttriMap;

/// <summary>
/// Handles raising build errors.
/// </summary>
internal static class CodeErrorReporter
{
    public static void ReportDuplicatePropertyMapping(this SourceProductionContext context, IPropertySymbol sourcePropertySymbol, AttributeUsage mapping)
    {
        context.ReportDiagnostic(Diagnostic.Create(DuplicatePropertyMapping, sourcePropertySymbol.Locations.First(), mapping.SourceTypeFullName, mapping.SourcePropertyName, mapping.TargetTypeFullName));
    }
    public static readonly DiagnosticDescriptor DuplicatePropertyMapping = new(
        id: "ATMA100",
        title: "Found duplicate property mapping",
        messageFormat: "Found multiple mapping attributes for '{0}.{1}' to target '{2}'",
        category: "Correctness",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );
}