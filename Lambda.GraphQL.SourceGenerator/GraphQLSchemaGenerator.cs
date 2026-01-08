using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

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

    private static bool IsGraphQLType(SyntaxNode node) => false; // TODO: Implement
    private static object? ExtractTypeInfo(GeneratorSyntaxContext context) => null; // TODO: Implement
    private static bool IsGraphQLOperation(SyntaxNode node) => false; // TODO: Implement
    private static object? ExtractOperationInfo(GeneratorSyntaxContext context) => null; // TODO: Implement
    
    private static void GenerateSchema(SourceProductionContext context, 
        ((ImmutableArray<object?> Left, ImmutableArray<object?> Right) Left, Compilation Right) combined) 
    { 
        // TODO: Implement schema generation
    }
}
