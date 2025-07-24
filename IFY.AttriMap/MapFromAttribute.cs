#pragma warning disable CS9113 // Ignore unused parameters (No logic here)

namespace IFY.AttriMap;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
public sealed class MapFromAttribute<TSource>(string sourceProperty, string? transformerMethod = null) : Attribute
{
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
public sealed class MapFromAttribute(Type sourceType, string sourceProperty, string? transformerMethod = null) : Attribute
{
}