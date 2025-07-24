using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace IFY.AttriMap;

[Generator]
internal class SourceGenerator : IIncrementalGenerator
{
    private static readonly string MapToAttributeFullName = typeof(MapToAttribute).FullName;
    private static readonly string MapToAttributeGenericFullName = typeof(MapToAttribute).FullName + "<TTarget>";
    private static readonly string MapFromAttributeFullName = typeof(MapFromAttribute).FullName;
    private static readonly string MapFromAttributeGenericFullName = typeof(MapFromAttribute).FullName + "<TSource>";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register a syntax provider to find property declarations with attributes
        var propertyDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is PropertyDeclarationSyntax p && p.AttributeLists.Count > 0,
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null)!;
        var compilationAndProperties = context.CompilationProvider.Combine(propertyDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndProperties, (spc, source) =>
        {
            var (compilation, properties) = source;

            // Find all usages of the MapTo attributes on properties
            var usages = new List<AttributeUsage>();
            foreach (var prop in properties)
            {
                var model = compilation.GetSemanticModel(prop!.SyntaxTree);
                var symbol = model.GetDeclaredSymbol(prop);
                if (symbol is IPropertySymbol propertySymbol)
                {
                    foreach (var attr in propertySymbol.GetAttributes())
                    {
                        if (attr.AttributeClass?.IsGenericType == true
                            && attr.AttributeClass.ConstructedFrom?.ToDisplayString() == MapToAttributeGenericFullName)
                        {
                            var targetTypeArg = (INamedTypeSymbol)attr.AttributeClass.TypeArguments[0];
                            var targetPropArg = attr.ConstructorArguments.ElementAtOrDefault(0).Value?.ToString()
                                ?? throw new ArgumentException("Argument 2 must be a string", "targetProperty"); // TODO: build error
                            var transformerMethodArg = attr.ConstructorArguments.ElementAtOrDefault(1).Value?.ToString();

                            var use = new AttributeUsage(propertySymbol.ContainingType,
                                propertySymbol.Name,
                                targetTypeArg,
                                targetPropArg,
                                false, transformerMethodArg);
                            usages.Add(use); // TODO: static
                        }
                        else if (attr.AttributeClass?.ToDisplayString() == MapToAttributeFullName)
                        {
                            usages.Add(AttributeUsage.To(propertySymbol, attr));
                        }
                        else if (attr.AttributeClass?.IsGenericType == true
                            && attr.AttributeClass.ConstructedFrom?.ToDisplayString() == MapFromAttributeGenericFullName)
                        {
                            var sourceTypeArg = (INamedTypeSymbol)attr.AttributeClass.TypeArguments[0];
                            var sourcePropArg = attr.ConstructorArguments.ElementAtOrDefault(0).Value?.ToString()
                                ?? throw new ArgumentException("Argument 2 must be a string", "targetProperty"); // TODO: build error
                            var transformerMethodArg = attr.ConstructorArguments.ElementAtOrDefault(1).Value?.ToString();

                            var use = new AttributeUsage(sourceTypeArg,
                                sourcePropArg,
                                propertySymbol.ContainingType,
                                propertySymbol.Name,
                                true, transformerMethodArg);
                            usages.Add(use); // TODO: static
                        }
                        else if (attr.AttributeClass?.ToDisplayString() == MapFromAttributeFullName)
                        {
                            usages.Add(AttributeUsage.From(propertySymbol, attr));
                        }
                    }
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine("// Auto-generated AttriMap");
            sb.AppendLine("// " + DateTime.UtcNow.ToString("O"));

            var groupedMaps = usages.GroupBy(u => u.MapperHash).ToArray();
            sb.AppendLine("// Found " + groupedMaps.Length + " uses of MapTo/MapFrom");

            foreach (var map in groupedMaps)
            {
                var def = map.First();

                sb.AppendLine($"namespace {def.SourceTypeNamespace}");
                sb.AppendLine("{");
                sb.AppendLine($"    // {def.SourceTypeFullName} -> {def.TargetTypeFullName}");
                sb.AppendLine($"    internal static class AttriMap__{def.MapperHash}");
                sb.AppendLine("    {");
                sb.AppendLine($"        public static {def.TargetTypeFullName} To{def.TargetTypeName}(this {def.SourceTypeFullName} source)");
                sb.AppendLine("            => new()");
                sb.AppendLine("            {");

                foreach (var prop in map)
                {
                    sb.Append("                ")
                        .Append($"{prop.TargetPropertyName} = ");
                    if (prop.TransformerMethodFullName is not null)
                    {
                        sb.AppendLine($"{prop.TransformerMethodFullName}(source.{prop.SourcePropertyName}),");
                    }
                    else
                    {
                        sb.AppendLine($"source.{prop.SourcePropertyName},");
                    }
                }

                sb.AppendLine("            };");
                sb.AppendLine("    }");
                sb.AppendLine("}");
            }

            spc.AddSource("AttriMap.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        });

    }

    private static PropertyDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        => (PropertyDeclarationSyntax)context.Node;
}