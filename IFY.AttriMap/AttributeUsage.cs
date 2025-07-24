using Microsoft.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace IFY.AttriMap;

/// <summary>
/// Represents a usage of the MapTo attribute, containing information about the source and target types and properties,
/// </summary>
readonly struct AttributeUsage
{
    static string Hash(string input)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new string([.. hash.Select(b => (char)('A' + (b % 26)))]);
    }
    public string MapperHash { get; }

    public string SourceTypeNamespace { get; }
    public string SourceTypeFullName { get; }
    public string SourcePropertyName { get; }

    public string TargetTypeFullName { get; }
    public string TargetTypeName { get; }
    public string TargetPropertyName { get; }

    public string? TransformerMethodFullName { get; }

    public AttributeUsage(INamedTypeSymbol sourceTypeSymbol, string sourcePropertyName, INamedTypeSymbol targetTypeSymbol, string targetPropertyName, bool tranformerOnTarget, string? transformerMethodName)
    {
        SourceTypeNamespace = sourceTypeSymbol.ContainingNamespace.ToDisplayString();
        SourceTypeFullName = sourceTypeSymbol.ToDisplayString();
        SourcePropertyName = sourcePropertyName;

        TargetTypeFullName = targetTypeSymbol.ToDisplayString();
        TargetTypeName = targetTypeSymbol.Name;
        TargetPropertyName = targetPropertyName;

        MapperHash = Hash(SourceTypeFullName + "__" + TargetTypeFullName);

        if (transformerMethodName is not null)
        {
            var transformerMethod = (tranformerOnTarget ? targetTypeSymbol : sourceTypeSymbol)
                .GetMembers(transformerMethodName)
                .OfType<IMethodSymbol>()
                .Where(m => m.Parameters.Length == 1) // TODO: and arg type
                .SingleOrDefault();
            // TODO: check result type?
            if (transformerMethod is not null)
            {
                TransformerMethodFullName = transformerMethod.IsStatic
                    ? $"{transformerMethod.ContainingType.ToDisplayString()}.{transformerMethodName}"
                    : $"source.{transformerMethodName}";
            }
        }
    }

    public static AttributeUsage To(IPropertySymbol propertySymbol, AttributeData attr)
    {
        var targetTypeArg = attr.ConstructorArguments.ElementAtOrDefault(0).Value as INamedTypeSymbol
            ?? throw new ArgumentException("Argument 1 must be a type", "targetType"); // TODO: build error
        var targetPropArg = attr.ConstructorArguments.ElementAtOrDefault(1).Value?.ToString()
            ?? throw new ArgumentException("Argument 2 must be a string", "targetProperty"); // TODO: build error
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
        var sourcePropArg = attr.ConstructorArguments.ElementAtOrDefault(1).Value?.ToString()
            ?? throw new ArgumentException("Argument 2 must be a string", "targetProperty"); // TODO: build error
        var transformerMethodArg = attr.ConstructorArguments.ElementAtOrDefault(2).Value?.ToString();

        return new AttributeUsage(sourceTypeArg,
            sourcePropArg,
            propertySymbol.ContainingType,
            propertySymbol.Name,
            true, transformerMethodArg);
    }
}
