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

    public string? TransformerMethodName { get; }
    public bool TransformerMethodExists { get; }
    public bool TransformerMethodIsStatic { get; }

    public AttributeUsage(INamedTypeSymbol sourceTypeSymbol, string sourcePropertyName, INamedTypeSymbol targetTypeSymbol, string targetPropertyName, string? transformerMethodName)
    {
        SourceTypeNamespace = sourceTypeSymbol.ContainingNamespace.ToDisplayString();
        SourceTypeFullName = sourceTypeSymbol.ToDisplayString();
        SourcePropertyName = sourcePropertyName;

        TargetTypeFullName = targetTypeSymbol.ToDisplayString();
        TargetTypeName = targetTypeSymbol.Name;
        TargetPropertyName = targetPropertyName;

        MapperHash = Hash(SourceTypeFullName + "__" + TargetTypeFullName);

        TransformerMethodName = transformerMethodName;
        if (transformerMethodName is not null)
        {
            var transformerMethod = sourceTypeSymbol
                .GetMembers(transformerMethodName)
                .OfType<IMethodSymbol>()
                .Where(m => m.Parameters.Length == 1) // TODO: and arg type
                .SingleOrDefault();
            // TODO: check result type?
            TransformerMethodExists = transformerMethod is not null;
            TransformerMethodIsStatic = transformerMethod?.IsStatic == true;
        }
    }
}
