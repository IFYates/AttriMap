using Microsoft.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace IFY.AttriMap;

/// <summary>
/// Represents a usage of the MapTo attribute, containing information about the source and target types and properties,
/// </summary>
readonly struct AttributeUsage
{
    public string MapperHash { get; }

    public string SourceTypeNamespace { get; }
    public string SourceTypeFullName { get; }
    public string SourcePropertyName { get; }

    public string TargetTypeFullName { get; }
    public string TargetTypeName { get; }
    public string TargetPropertyName { get; }

    public string? TransformerMethodFullName { get; }

    public AttributeUsage(INamedTypeSymbol sourceTypeSymbol, string sourcePropertyName, INamedTypeSymbol targetTypeSymbol, string? targetPropertyName, bool tranformerOnTarget, string? transformerMethodName)
    {
        SourceTypeNamespace = sourceTypeSymbol.ContainingNamespace.ToDisplayString();
        SourceTypeFullName = sourceTypeSymbol.ToDisplayString();
        SourcePropertyName = sourcePropertyName;

        TargetTypeFullName = targetTypeSymbol.ToDisplayString();
        TargetTypeName = targetTypeSymbol.Name;
        TargetPropertyName = targetPropertyName ?? sourcePropertyName;

        // TODO: Check target property exists

        // Hash the type names to create a unique identifier for the mapper
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes($"{SourceTypeFullName}__{TargetTypeFullName}"));
        MapperHash = new string([.. hash.Select(b => (char)('A' + (b % 26)))]);

        if (transformerMethodName is not null)
        {
            var transformerParent = tranformerOnTarget ? targetTypeSymbol : sourceTypeSymbol;
            var transformerMethod = transformerParent.GetMembers(transformerMethodName)
                .OfType<IMethodSymbol>()
                .Where(m => m.Parameters.Length == 1) // TODO: and arg type
                .SingleOrDefault();
            // TODO: Check result type against target property
            if (transformerMethod is not null)
            {
                TransformerMethodFullName = tranformerOnTarget || transformerMethod.IsStatic
                    ? $"{transformerParent.ToDisplayString()}.{transformerMethodName}"
                    : $"source.{transformerMethodName}";
            }
            else
            {
                // TODO: Missing transformer method
            }
        }
        else
        {
            // TODO: Check target property type
        }
    }

    public static AttributeUsage To(IPropertySymbol propertySymbol, AttributeData attr)
    {
        var targetTypeArg = attr.ConstructorArguments.ElementAtOrDefault(0).Value as INamedTypeSymbol
            ?? throw new ArgumentException("Argument 1 must be a type", "targetType"); // TODO: build error
        var targetPropArg = attr.ConstructorArguments.ElementAtOrDefault(1).Value?.ToString();
        var transformerMethodArg = attr.ConstructorArguments.ElementAtOrDefault(2).Value?.ToString();

        return new AttributeUsage(propertySymbol.ContainingType,
            propertySymbol.Name,
            targetTypeArg,
            targetPropArg,
            false, transformerMethodArg);
    }

    public static AttributeUsage From(IPropertySymbol propertySymbol, AttributeData attr)
    {
        var sourceTypeArg = attr.ConstructorArguments.ElementAtOrDefault(0).Value as INamedTypeSymbol
            ?? throw new ArgumentException("Argument 1 must be a type", "targetType"); // TODO: build error
        var sourcePropArg = attr.ConstructorArguments.ElementAtOrDefault(1).Value?.ToString();
        var transformerMethodArg = attr.ConstructorArguments.ElementAtOrDefault(2).Value?.ToString();

        return new AttributeUsage(sourceTypeArg,
            sourcePropArg ?? propertySymbol.Name,
            propertySymbol.ContainingType,
            propertySymbol.Name,
            true, transformerMethodArg);
    }
}
