#pragma warning disable CS9113 // Ignore unused parameters (No logic here)

using Microsoft.CodeAnalysis;

namespace IFY.AttriMap;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
public sealed class MapFromAttribute<TSource>(string? sourceProperty = null, string? transformerMethod = null) : Attribute
{
    public static readonly string FullTypeName = $"{MapFromAttribute.FullTypeName}<{nameof(TSource)}>";
    private static readonly string AttributeGenericPrefix = typeof(MapFromAttribute).FullName;

    public static bool IsMatch(AttributeData attr)
        => attr.AttributeClass?.IsGenericType == true
        && attr.AttributeClass.ConstructedFrom?.ToDisplayString() == FullTypeName;
    public static bool IsMatch(string? name)
        => name == FullTypeName
        || name?.StartsWith(AttributeGenericPrefix) == true;
    public static bool IsPossibleMatch(string name)
        => name.StartsWith("MapFrom<") || name.StartsWith("MapFromAttribute<")
        || IsMatch(name);

    // TODO: Transformer property instead of constructor argument
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
public sealed class MapFromAttribute(Type sourceType, string? sourceProperty = null, string? transformerMethod = null) : Attribute
{
    public static readonly string FullTypeName = typeof(MapFromAttribute).FullName;

    public static bool IsMatch(AttributeData attr)
        => attr.AttributeClass?.IsGenericType == false
        && attr.AttributeClass.ConstructedFrom?.ToDisplayString() == FullTypeName;
    public static bool IsMatch(string? name)
        => name == FullTypeName
        || MapFromAttribute<object>.IsMatch(name);
    public static bool IsPossibleMatch(string name)
        => name is "MapFrom" or "MapFromAttribute"
        || MapFromAttribute<object>.IsPossibleMatch(name);

    // TODO: Transformer property instead of constructor argument
}