# AttriMap
A simple attribute-based mapping library for C# that allows you to map properties between types.

## Features
- Attribute-based mapping; defined on the types themselves
- Source-generated extension methods for best performance
- Mapping from source to target and vice versa, or through interfaces
- Transforming values during mapping

## Full MapTo Example
```csharp
using IFY.AttriMap;

var source = new MySourceType
{
    Name = "Test",
    SourceValue = "Example",
    Date = "2025-07-24"
};
var target = source.ToMyTargetType(); // Generated extension method named after the target type

Console.WriteLine(target.Name); // Output: Test
Console.WriteLine(target.TargetValue); // Output: Example
Console.WriteLine(target.Date.ToString("yyyy-MM-dd")); // Output: 2025-07-24

class MySourceType
{
    [MapTo<MyTargetType>] // Direct mapping (C# 12+ syntax)
    public string Name { get; set; }

    [MapTo(typeof(MyTargetType), nameof(MyTargetType.TargetValue))] // Mapping to a different property name
    public string SourceValue { get; set; }

    [MapTo(typeof(MyTargetType), transformerMethod: nameof(ParseDateTime))] // Transforming value during mapping
    public string Date { get; set; }
    public static DateTime ParseDateTime(string value) => DateTime.Parse(value);
}

class MyTargetType
{
    public string Name { get; init; }
    public string TargetValue { get; init; }
    public DateTime Date { get; init; }
}
```

## Full MapFrom Example
```csharp
using IFY.AttriMap;

var source = new MySourceType
{
    Name = "Test",
    SourceValue = "Example",
    Date = "2025-07-24"
};
var target = source.ToMyTargetType(); // Generated extension method named after the target type

Console.WriteLine(target.Name); // Output: Test
Console.WriteLine(target.TargetValue); // Output: Example
Console.WriteLine(target.Date.ToString("yyyy-MM-dd")); // Output: 2025-07-24

class MySourceType
{
    public string Name { get; init; }
    public string SourceValue { get; init; }
    public string Date { get; init; }
}

class MyTargetType
{
    [MapFrom<MySourceType>] // Direct mapping (C# 12+ syntax)
    public string Name { get; set; }

    [MapFrom(typeof(MySourceType), nameof(MySourceType.SourceValue))] // Mapping to a different property name
    public string TargetValue { get; set; }

    [MapFrom(typeof(MySourceType), transformerMethod: nameof(ParseDateTime))] // Transforming value during mapping
    public DateTime Date { get; set; }
    public static DateTime ParseDateTime(string value) => DateTime.Parse(value);
}
```