#pragma warning disable CS9113 // Ignore unused parameters (No logic here)

using Microsoft.CodeAnalysis;

namespace IFY.AttriMap;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
public sealed class MapToAttribute<TTarget>(string? targetProperty = null, string? transformerMethod = null) : Attribute
    where TTarget : new()
{
    public static readonly string FullTypeName = $"{MapToAttribute.FullTypeName}<{nameof(TTarget)}>";
    private static readonly string AttributeGenericPrefix = typeof(MapToAttribute).FullName;

    public static bool IsMatch(AttributeData attr)
        => attr.AttributeClass?.IsGenericType == true
        && attr.AttributeClass.ConstructedFrom?.ToDisplayString() == FullTypeName;
    public static bool IsMatch(string? name)
        => name == FullTypeName
        || name?.StartsWith(AttributeGenericPrefix) == true;
    public static bool IsPossibleMatch(string name)
        => name.StartsWith("MapTo<") || name.StartsWith("MapToAttribute<")
        || IsMatch(name);

    // TODO: Transformer property instead of constructor argument
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
public sealed class MapToAttribute(Type targetType, string? targetProperty, string? transformerMethod = null) : Attribute
{
    public static readonly string FullTypeName = typeof(MapToAttribute).FullName;

    public static bool IsMatch(AttributeData attr)
        => attr.AttributeClass?.IsGenericType == false
        && attr.AttributeClass.ConstructedFrom?.ToDisplayString() == FullTypeName;
    public static bool IsMatch(string? name)
        => name == FullTypeName
        || MapToAttribute<object>.IsMatch(name);
    public static bool IsPossibleMatch(string name)
        => name is "MapTo" or "MapToAttribute"
        || MapToAttribute<object>.IsPossibleMatch(name);

    // TODO: Transformer property instead of constructor argument
}