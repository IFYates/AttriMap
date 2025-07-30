using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace IFY.AttriMap;

[Generator]
internal class SourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register a syntax provider to find property declarations with MapTo/MapFrom attributes
        var propertyDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(selectProperties, resolvePropertyMaps);
        var compilationAndProperties = context.CompilationProvider.Combine(propertyDeclarations.Collect());

        // Find all properties with probable MapTo/MapFrom attributes
        static bool selectProperties(SyntaxNode node, CancellationToken token)
        {
            return node is PropertyDeclarationSyntax propertyNode
                && propertyNode.AttributeLists.Any(l => l.Attributes.Any(a => isProbablyAttribute(a.Name.ToString())));
            static bool isProbablyAttribute(string name)
                => MapFromAttribute.IsPossibleMatch(name) || MapToAttribute.IsPossibleMatch(name);
        }

        // Find all usages of the MapTo attributes on filtered properties
        static AttributeUsage[] resolvePropertyMaps(GeneratorSyntaxContext context, CancellationToken token)
        {
            var property = (PropertyDeclarationSyntax)context.Node;
            var propertySymbol = (IPropertySymbol)context.SemanticModel.GetDeclaredSymbol(property, token)!;

            var usages = new List<AttributeUsage>();
            foreach (var attr in propertySymbol.GetAttributes())
            {
                AttributeUsage? newUsage = null;
                if (MapToAttribute.IsMatch(attr)
                    || MapToAttribute<object>.IsMatch(attr))
                {
                    newUsage = AttributeUsage.To(propertySymbol, attr);
                }
                else if (MapFromAttribute.IsMatch(attr)
                    || MapFromAttribute<object>.IsMatch(attr))
                {
                    newUsage = AttributeUsage.From(propertySymbol, attr);
                }
                if (newUsage is not null)
                {
                    usages.Add(newUsage.Value);
                }
            }
            return [.. usages];
        }

        context.RegisterSourceOutput(compilationAndProperties, (context, source) =>
        {
            var (compilation, maps) = source;
            var usages = maps.SelectMany(m => m).ToArray();

            // TODO: All warnings and errors should be reported through the context
            // TODO: Warn on duplicate mapping
            //context.ReportDuplicatePropertyMapping(propertySymbol, newUsage.Value);

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
}