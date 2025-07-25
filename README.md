# Source to target
For when the source type can reference the target type.
```csharp
using IFY.AttriMap;

var source = new Source { Name = "Test" };
var target = source.ToTarget();
Console.WriteLine(target.Name); // Output: Test

class Source
{
    [MapTo(typeof(Target), nameof(Target.Name))]
    // C# 12+: [MapTo<Target>(nameof(Target.Name))]
    // [MapTo(typeof(Target))] // Property name matches target property name
    public string Name { get; set; }
}

class Target
{
    public string Name { get; set; }
}
```

Through interface
```csharp
using IFY.AttriMap;

var source = new Source { Name = "Test" };
var target = source.ToTarget();
Console.WriteLine(target.Name); // Output: Test

interface ISource
{
    [MapTo(typeof(Target), nameof(Target.Name))]
    string Name { get; }
}
class Source : ISource
{
    public string Name { get; set; }
}

class Target
{
    public string Name { get; set; }
}
```

Transform value
```csharp
using IFY.AttriMap;

var source = new Source { Name = "Test" };
var target = source.ToTarget();
Console.WriteLine(target.Name); // Output: TEST

class Source
{
    [MapTo(typeof(Target), nameof(Target.Name), nameof(ToUpper))]
    // C# 12+: [MapTo<Target>(nameof(Target.Name), nameof(ToUpper))]
    public string Name { get; set; }
    public static string ToUpper(string value) => value.ToUpperInvariant();
}

class Target
{
    public string Name { get; set; }
}
```

# Target to source
For when the target type can reference the source type.
```csharp
using IFY.AttriMap;

var source = new Source { Name = "Test" };
var target = source.ToTarget();
Console.WriteLine(target.Name); // Output: Test

class Source
{
    public string Name { get; set; }
}

class Target
{
    [MapFrom(typeof(Source), nameof(Source.Name))]
    // C# 12+: [MapFrom<Source>(nameof(Source.Name))]
    public string Name { get; set; }
}
```

Through interface
```csharp
using IFY.AttriMap;

var source = new Source { Name = "Test" };
var target = source.ToTarget();
Console.WriteLine(target.Name); // Output: Test

interface ISource
{
    string Name { get; }
}
class Source : ISource
{
    public string Name { get; set; }
}

class Target
{
    [MapFrom(typeof(ISource), nameof(Source.Name))]
    public string Name { get; set; }
}
```

Transform value
```csharp
using IFY.AttriMap;

var source = new Source { Name = "Test" };
var target = source.ToTarget();
Console.WriteLine(target.Name); // Output: TEST

class Source
{
    public string Name { get; set; }
}

class Target
{
    [MapFrom(typeof(Source), nameof(Source.Name), nameof(ToUpper))]
    // C# 12+: [MapFrom<Source>(nameof(Source.Name), nameof(ToUpper))]
    public string Name { get; set; }
    public static string ToUpper(string value) => value.ToUpperInvariant();
}
```
