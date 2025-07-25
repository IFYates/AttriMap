using Microsoft.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace IFY.AttriMap;

/// <summary>
/// Represents a usage of the MapTo attribute, containing information about the source and target types and properties,
/// </summary>
readonly struct AttributeUsage(INamedTypeSymbol sourceTypeSymbol, string sourcePropertyName, INamedTypeSymbol targetTypeSymbol, string targetPropertyName, string? transformerMethodName)
{
    /// <summary>
    /// Hash of the type names to create a unique identifier for the mapper.
    /// </summary>
    public string MapperHash { get; } = generateMapperHash(sourceTypeSymbol, targetTypeSymbol);
    private static string generateMapperHash(INamedTypeSymbol sourceType, INamedTypeSymbol targetType)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes($"{sourceType.ToDisplayString()}__{targetType.ToDisplayString()}"));
        return new string([.. hash.Select(b => (char)('A' + (b % 26)))]);
    }

    public string SourceTypeNamespace { get; } = sourceTypeSymbol.ContainingNamespace.ToDisplayString();
    public string SourceTypeFullName { get; } = sourceTypeSymbol.ToDisplayString();
    public string SourcePropertyName { get; } = sourcePropertyName;

    public string TargetTypeFullName { get; } = targetTypeSymbol.ToDisplayString();
    public string TargetTypeName { get; } = targetTypeSymbol.Name;
    public string TargetPropertyName { get; } = targetPropertyName;

    public string? TransformerMethodFullName { get; } = transformerMethodName;

    private static IMethodSymbol? getMethod(INamedTypeSymbol typeSymbol, string methodName)
    {
        return typeSymbol.GetMembers(methodName)
            .OfType<IMethodSymbol>()
            .Where(m => m.Parameters.Length == 1) // TODO: and source prop type? Or leave to compiler?
            .SingleOrDefault();
    }

    public static AttributeUsage To(IPropertySymbol propertySymbol, AttributeData attr)
    {
        // TODO: Check source property has 'get' accessor

        var isGeneric = attr.AttributeClass!.IsGenericType;
        var targetTypeArg = (isGeneric
            ? (INamedTypeSymbol)attr.AttributeClass!.TypeArguments[0]
            : attr.ConstructorArguments.ElementAtOrDefault(0).Value as INamedTypeSymbol)
            ?? throw new ArgumentException("Argument 1 must be a type", "targetType"); // TODO: build error
        var targetPropArg = attr.ConstructorArguments.ElementAtOrDefault(isGeneric ? 0 : 1).Value?.ToString()
            ?? propertySymbol.Name;

        // TODO: Check target property exists and has 'init'/'set' accessor

        // Resolve source transformer method
        string? transformerMethodName = null;
        var transformerMethodArg = attr.ConstructorArguments.ElementAtOrDefault(isGeneric ? 1 : 2).Value?.ToString();
        if (transformerMethodArg is not null)
        {
            var transformerMethod = getMethod(propertySymbol.ContainingType, transformerMethodArg);
            if (transformerMethod is not null)
            {
                // TODO: Check result type against target property? Or leave to compiler?
                transformerMethodName = transformerMethod.IsStatic
                    ? $"{propertySymbol.ContainingType.ToDisplayString()}.{transformerMethodArg}"
                    : $"source.{transformerMethodArg}";
            }
            else
            {
                // TODO: Missing transformer method
                throw new MissingMethodException();
            }
        }

        return new AttributeUsage(propertySymbol.ContainingType,
            propertySymbol.Name,
            targetTypeArg,
            targetPropArg,
            transformerMethodName);
    }

    public static AttributeUsage From(IPropertySymbol propertySymbol, AttributeData attr)
    {
        // TODO: Check target property has 'init'/'set' accessor

        var isGeneric = attr.AttributeClass!.IsGenericType;
        var sourceTypeArg = (isGeneric
            ? (INamedTypeSymbol)attr.AttributeClass!.TypeArguments[0]
            : attr.ConstructorArguments.ElementAtOrDefault(0).Value as INamedTypeSymbol)
            ?? throw new ArgumentException("Argument 1 must be a type", "sourceType"); // TODO: build error
        var sourcePropArg = attr.ConstructorArguments.ElementAtOrDefault(isGeneric ? 0 : 1).Value?.ToString()
            ?? propertySymbol.Name;

        // TODO: Check source property exists and has 'get' accessor

        // Resolve target transformer method
        string? transformerMethodName = null;
        var transformerMethodArg = attr.ConstructorArguments.ElementAtOrDefault(isGeneric ? 1 : 2).Value?.ToString();
        if (transformerMethodArg is not null)
        {
            var transformerMethod = getMethod(propertySymbol.ContainingType, transformerMethodArg);
            if (transformerMethod is not null)
            {
                // TODO: Check result type against target property? Or leave to compiler?
                transformerMethodName = $"{propertySymbol.ContainingType.ToDisplayString()}.{transformerMethodArg}";
            }
            else
            {
                // TODO: Missing transformer method
                throw new MissingMethodException();
            }
        }

        return new AttributeUsage(sourceTypeArg,
            sourcePropArg,
            propertySymbol.ContainingType,
            propertySymbol.Name,
            transformerMethodName);
    }
}
