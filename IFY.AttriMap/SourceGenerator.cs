using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace IFY.AttriMap;

[Generator]
internal class SourceGenerator : IIncrementalGenerator
{
    private static readonly string MapToAttributeFullName = typeof(MapToAttribute).FullName;
    private static readonly string MapToAttributeGenericFullName = $"{typeof(MapToAttribute).FullName}<TTarget>";
    private static readonly string MapFromAttributeFullName = typeof(MapFromAttribute).FullName;
    private static readonly string MapFromAttributeGenericFullName = $"{typeof(MapFromAttribute).FullName}<TSource>";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register a syntax provider to find property declarations with attributes
        var propertyDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is PropertyDeclarationSyntax p && p.AttributeLists.Count > 0,
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null)!;
        var compilationAndProperties = context.CompilationProvider.Combine(propertyDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndProperties, (context, source) =>
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
                        AttributeUsage? newUsage = null;
                        if ((attr.AttributeClass?.IsGenericType == true
                            && attr.AttributeClass.ConstructedFrom?.ToDisplayString() == MapToAttributeGenericFullName)
                            || attr.AttributeClass?.ToDisplayString() == MapToAttributeFullName)
                        {
                            newUsage = AttributeUsage.To(propertySymbol, attr);
                        }
                        else if ((attr.AttributeClass?.IsGenericType == true
                            && attr.AttributeClass.ConstructedFrom?.ToDisplayString() == MapFromAttributeGenericFullName)
                            || attr.AttributeClass?.ToDisplayString() == MapFromAttributeFullName)
                        {
                            newUsage = AttributeUsage.From(propertySymbol, attr);
                        }

                        if (newUsage is not null)
                        {
                            usages.Add(newUsage.Value);

                            // TODO: Warn on duplicate mapping
                            //context.ReportDuplicatePropertyMapping(propertySymbol, newUsage.Value);
                        }
                    }
                }
            }

            // Generate the AttriMap extension methods
            var sb = new StringBuilder();
            sb.AppendLine("// Auto-generated AttriMap extension methods");
            sb.AppendLine($"// {DateTime.UtcNow:O}");

            var groupedMaps = usages.GroupBy(u => u.MapperHash).ToArray();
            sb.AppendLine($"// Found {groupedMaps.Length} uses of MapTo/MapFrom");

            foreach (var map in groupedMaps)
            {
                var def = map.First();

                sb.AppendLine($"namespace {def.SourceTypeNamespace}");
                sb.AppendLine("{");
                sb.AppendLine($"    // {def.SourceTypeFullName} -> {def.TargetTypeFullName}");
                sb.AppendLine($"    public static class AttriMap__{def.MapperHash}");
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

            context.AddSource("AttriMap.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        });

    }

    private static PropertyDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        => (PropertyDeclarationSyntax)context.Node;
}