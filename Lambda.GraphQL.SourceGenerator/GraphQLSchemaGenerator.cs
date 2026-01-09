using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Lambda.GraphQL.SourceGenerator.Models;

namespace Lambda.GraphQL.SourceGenerator;

/// <summary>
/// Roslyn source generator that creates GraphQL schemas from C# types and Lambda functions.
/// </summary>
[Generator(LanguageNames.CSharp)]
public partial class GraphQLSchemaGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find classes with GraphQL type attributes
        var typeDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (s, _) => IsGraphQLType(s),
                transform: (ctx, _) => ExtractTypeInfo(ctx))
            .Where(t => t != null);

        // Find Lambda functions with GraphQL operation attributes
        var operationDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (s, _) => IsGraphQLOperation(s),
                transform: (ctx, _) => ExtractOperationInfo(ctx))
            .Where(o => o != null);

        // Combine and generate schema
        var combined = typeDeclarations.Collect()
            .Combine(operationDeclarations.Collect())
            .Combine(context.CompilationProvider);

        context.RegisterSourceOutput(combined, GenerateSchema);
    }

    private static bool IsGraphQLType(SyntaxNode node)
    {
        if (node is not ClassDeclarationSyntax and not EnumDeclarationSyntax)
            return false;

        var hasGraphQLTypeAttribute = node.DescendantNodes()
            .OfType<AttributeSyntax>()
            .Any(attr => attr.Name.ToString().Contains("GraphQLType"));

        return hasGraphQLTypeAttribute;
    }
    private static object? ExtractTypeInfo(GeneratorSyntaxContext context)
    {
        try
        {
            var semanticModel = context.SemanticModel;
            INamedTypeSymbol? typeSymbol = null;

            if (context.Node is ClassDeclarationSyntax classDecl)
            {
                typeSymbol = semanticModel.GetDeclaredSymbol(classDecl);
            }
            else if (context.Node is EnumDeclarationSyntax enumDecl)
            {
                typeSymbol = semanticModel.GetDeclaredSymbol(enumDecl);
            }

            if (typeSymbol == null)
                return null;

            var graphqlTypeAttr = typeSymbol.GetAttributes()
                .FirstOrDefault(attr => attr.AttributeClass?.Name == "GraphQLTypeAttribute");

            if (graphqlTypeAttr == null)
                return null;

            var typeInfo = new Models.TypeInfo
            {
                Name = GetAttributeStringValue(graphqlTypeAttr, 0) ?? typeSymbol.Name,
                Description = GetAttributePropertyValue(graphqlTypeAttr, "Description"),
                IsInterface = typeSymbol.TypeKind == Microsoft.CodeAnalysis.TypeKind.Interface,
                IsEnum = typeSymbol.TypeKind == Microsoft.CodeAnalysis.TypeKind.Enum
            };

            // Set Kind based on type
            if (typeInfo.IsEnum)
            {
                typeInfo.Kind = Models.TypeKind.Enum;
                ExtractEnumValues(typeSymbol, typeInfo);
            }
            else if (typeInfo.IsInterface)
            {
                typeInfo.Kind = Models.TypeKind.Interface;
            }
            else
            {
                // Check Kind property from attribute
                var kindValue = GetAttributePropertyValue(graphqlTypeAttr, "Kind");
                if (kindValue == "Input")
                    typeInfo.Kind = Models.TypeKind.Input;
                else
                    typeInfo.Kind = Models.TypeKind.Object;
            }

            // Extract fields for non-enum types
            if (!typeInfo.IsEnum)
            {
                ExtractFields(typeSymbol, typeInfo);
            }

            return typeInfo;
        }
        catch (ArgumentException ex)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.TypeExtractionError,
                context.Node.GetLocation(),
                context.Node.ToString(),
                ex.Message);
            // Note: We can't report diagnostics here as we don't have access to the context
            // This will be handled in the main generation method
            return null;
        }
        catch (InvalidOperationException ex)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.TypeExtractionError,
                context.Node.GetLocation(),
                context.Node.ToString(),
                ex.Message);
            return null;
        }
        catch (System.Exception ex)
        {
            // For unexpected exceptions, still return null but log the error type
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.TypeExtractionError,
                context.Node.GetLocation(),
                context.Node.ToString(),
                $"Unexpected error: {ex.GetType().Name} - {ex.Message}");
            return null;
        }
    }

    private static string? GetAttributeStringValue(AttributeData? attribute, int index)
    {
        if (attribute?.ConstructorArguments.Length > index)
        {
            var arg = attribute.ConstructorArguments[index];
            return arg.Value?.ToString();
        }
        return null;
    }

    private static string? GetAttributePropertyValue(AttributeData? attribute, string propertyName)
    {
        if (attribute == null) return null;
        var namedArg = attribute.NamedArguments.FirstOrDefault(arg => arg.Key == propertyName);
        return namedArg.Value.Value?.ToString();
    }

    private static void ExtractEnumValues(INamedTypeSymbol enumSymbol, Models.TypeInfo typeInfo)
    {
        foreach (var member in enumSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            if (member.IsStatic && member.HasConstantValue)
            {
                var enumValueAttr = member.GetAttributes()
                    .FirstOrDefault(attr => attr.AttributeClass?.Name == "GraphQLEnumValueAttribute");

                var enumValue = new EnumValueInfo
                {
                    Name = GetAttributeStringValue(enumValueAttr, 0) ?? member.Name,
                    Description = GetAttributePropertyValue(enumValueAttr, "Description"),
                    IsDeprecated = GetAttributePropertyValue(enumValueAttr, "Deprecated") == "True",
                    DeprecationReason = GetAttributePropertyValue(enumValueAttr, "DeprecationReason")
                };

                typeInfo.EnumValues.Add(enumValue);
            }
        }
    }

    private static void ExtractFields(INamedTypeSymbol typeSymbol, Models.TypeInfo typeInfo)
    {
        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is IPropertySymbol property)
            {
                // Skip if marked with GraphQLIgnore
                var hasIgnore = property.GetAttributes()
                    .Any(attr => attr.AttributeClass?.Name == "GraphQLIgnoreAttribute");
                if (hasIgnore) continue;

                var fieldAttr = property.GetAttributes()
                    .FirstOrDefault(attr => attr.AttributeClass?.Name == "GraphQLFieldAttribute");

                var fieldInfo = new FieldInfo
                {
                    Name = GetAttributeStringValue(fieldAttr, 0) ?? property.Name,
                    Description = GetAttributePropertyValue(fieldAttr, "Description"),
                    Type = TypeMapper.MapType(property.Type),
                    IsNullable = !TypeMapper.IsNonNull(property.Type),
                    IsDeprecated = GetAttributePropertyValue(fieldAttr, "Deprecated") == "True",
                    DeprecationReason = GetAttributePropertyValue(fieldAttr, "DeprecationReason")
                };

                // Check for GraphQLNonNull override
                var hasNonNull = property.GetAttributes()
                    .Any(attr => attr.AttributeClass?.Name == "GraphQLNonNullAttribute");
                if (hasNonNull)
                {
                    fieldInfo.IsNullable = false;
                }

                typeInfo.Fields.Add(fieldInfo);
            }
        }
    }
    private static bool IsGraphQLOperation(SyntaxNode node)
    {
        if (node is not MethodDeclarationSyntax method)
            return false;

        var attributes = method.AttributeLists
            .SelectMany(list => list.Attributes)
            .Select(attr => attr.Name.ToString())
            .ToList();

        var hasLambdaFunction = attributes.Any(name => name.Contains("LambdaFunction"));
        var hasGraphQLOperation = attributes.Any(name => 
            name.Contains("GraphQLQuery") || 
            name.Contains("GraphQLMutation") || 
            name.Contains("GraphQLSubscription"));

        return hasLambdaFunction && hasGraphQLOperation;
    }
    private static object? ExtractOperationInfo(GeneratorSyntaxContext context)
    {
        try
        {
            if (context.Node is not MethodDeclarationSyntax method)
                return null;

            var semanticModel = context.SemanticModel;
            var methodSymbol = semanticModel.GetDeclaredSymbol(method);
            if (methodSymbol == null)
                return null;

            // Find GraphQL operation attribute
            var graphqlOpAttr = methodSymbol.GetAttributes()
                .FirstOrDefault(attr => 
                    attr.AttributeClass?.Name == "GraphQLQueryAttribute" ||
                    attr.AttributeClass?.Name == "GraphQLMutationAttribute" ||
                    attr.AttributeClass?.Name == "GraphQLSubscriptionAttribute");

            if (graphqlOpAttr == null)
                return null;

            // Find resolver attribute
            var resolverAttr = methodSymbol.GetAttributes()
                .FirstOrDefault(attr => attr.AttributeClass?.Name == "GraphQLResolverAttribute");

            // Find Lambda function attribute
            var lambdaAttr = methodSymbol.GetAttributes()
                .FirstOrDefault(attr => attr.AttributeClass?.Name == "LambdaFunctionAttribute");

            var operationName = GetAttributeStringValue(graphqlOpAttr, 0) ?? methodSymbol.Name;
            var typeName = graphqlOpAttr.AttributeClass?.Name switch
            {
                "GraphQLQueryAttribute" => "Query",
                "GraphQLMutationAttribute" => "Mutation",
                "GraphQLSubscriptionAttribute" => "Subscription",
                _ => "Query"
            };

            var resolverInfo = new ResolverInfo
            {
                TypeName = typeName,
                FieldName = operationName,
                Kind = ResolverKind.Unit,
                DataSource = GetAttributePropertyValue(resolverAttr, "DataSource"),
                LambdaFunctionName = methodSymbol.Name,
                LambdaFunctionLogicalId = $"{methodSymbol.Name}Function",
                Runtime = "APPSYNC_JS",
                RequestMapping = GetAttributePropertyValue(resolverAttr, "RequestMapping"),
                ResponseMapping = GetAttributePropertyValue(resolverAttr, "ResponseMapping"),
                ReturnType = ReturnTypeExtractor.GetFormattedReturnType(methodSymbol)
            };

            // Check if it's a pipeline resolver
            var functionsValue = GetAttributePropertyValue(resolverAttr, "Functions");
            if (!string.IsNullOrEmpty(functionsValue))
            {
                resolverInfo.Kind = ResolverKind.Pipeline;
                // Parse functions array - simplified for now
                resolverInfo.Functions.Add(functionsValue);
            }

            return resolverInfo;
        }
        catch (ArgumentException ex)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.OperationExtractionError,
                context.Node.GetLocation(),
                context.Node.ToString(),
                ex.Message);
            return null;
        }
        catch (InvalidOperationException ex)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.OperationExtractionError,
                context.Node.GetLocation(),
                context.Node.ToString(),
                ex.Message);
            return null;
        }
        catch (System.Exception ex)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.OperationExtractionError,
                context.Node.GetLocation(),
                context.Node.ToString(),
                $"Unexpected error: {ex.GetType().Name} - {ex.Message}");
            return null;
        }
    }
    
    private static void GenerateSchema(SourceProductionContext context, 
        ((ImmutableArray<object?> Left, ImmutableArray<object?> Right) Left, Compilation Right) combined) 
    { 
        try
        {
            var (typeAndOperationData, compilation) = combined;
            var (typeData, operationData) = typeAndOperationData;

            // Extract types and operations from the collected data
            var types = typeData.OfType<Models.TypeInfo>().ToList();
            var operations = operationData.OfType<ResolverInfo>().ToList();

            if (!types.Any() && !operations.Any())
                return;

            // Find schema attribute for metadata
            string? schemaName = null;
            string? schemaDescription = null;

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var root = syntaxTree.GetRoot();
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                
                // Look for assembly attributes
                var assemblyAttrs = root.DescendantNodes()
                    .OfType<AttributeSyntax>()
                    .Where(attr => attr.Name.ToString().Contains("GraphQLSchema"));

                foreach (var attr in assemblyAttrs)
                {
                    // Extract schema name and description - simplified
                    schemaName = "GeneratedSchema";
                    schemaDescription = "Generated GraphQL schema from Lambda functions";
                    break;
                }
            }

            // Generate GraphQL SDL
            var sdl = SdlGenerator.GenerateSchema(types, operations, schemaName, schemaDescription);
            
            // Generate resolver manifest
            var resolverManifest = ResolverManifestGenerator.GenerateManifest(operations);

            // Emit SDL as embedded resource
            var sdlSource = $@"
using System.Reflection;

[assembly: System.Reflection.AssemblyMetadata(""GraphQL.Schema"", @""{EscapeString(sdl)}"")]
[assembly: System.Reflection.AssemblyMetadata(""GraphQL.ResolverManifest"", @""{EscapeString(resolverManifest)}"")]

namespace Lambda.GraphQL.Generated
{{
    internal static class GeneratedSchema
    {{
        public const string SDL = @""{EscapeString(sdl)}"";
        public const string ResolverManifest = @""{EscapeString(resolverManifest)}"";
    }}
}}
";

            context.AddSource("GraphQLSchema.g.cs", sdlSource);
        }
        catch (System.Exception ex)
        {
            // Add diagnostic for debugging
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.SchemaGenerationError,
                Location.None,
                ex.Message);
            
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static string EscapeString(string input)
    {
        return input.Replace("\"", "\"\"").Replace("\r\n", "\\r\\n").Replace("\n", "\\n");
    }
}
