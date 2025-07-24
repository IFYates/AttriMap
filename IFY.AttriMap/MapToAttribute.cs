#pragma warning disable CS9113 // Ignore unused parameters (No logic here)

namespace IFY.AttriMap;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
public sealed class MapToAttribute<TTarget>(string targetProperty, string? transformerMethod = null) : Attribute
    where TTarget : new()
{
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
public sealed class MapToAttribute(Type targetType, string targetProperty, string? transformerMethod = null) : Attribute
{
}