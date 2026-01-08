using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using Lambda.GraphQL.SourceGenerator.Models;

namespace Lambda.GraphQL.SourceGenerator;

/// <summary>
/// Generates resolver manifest JSON for AppSync CDK integration.
/// </summary>
public static class ResolverManifestGenerator
{
    /// <summary>
    /// Generates a resolver manifest JSON from resolver information.
    /// </summary>
    public static string GenerateManifest(IEnumerable<ResolverInfo> resolvers)
    {
        var resolverList = resolvers.ToList();
        var dataSources = ExtractDataSources(resolverList);

        var manifest = new
        {
            schema = "https://lambda-graphql.dev/schemas/resolvers.json",
            version = "1.0.0",
            generatedAt = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            resolvers = resolverList.Select(r => new
            {
                typeName = r.TypeName,
                fieldName = r.FieldName,
                kind = r.Kind.ToString().ToUpperInvariant(),
                dataSource = r.DataSource,
                lambdaFunctionName = r.LambdaFunctionName,
                lambdaFunctionLogicalId = r.LambdaFunctionLogicalId,
                runtime = r.Runtime,
                requestMapping = r.RequestMapping,
                responseMapping = r.ResponseMapping,
                functions = r.Functions.Count > 0 ? r.Functions : null
            }).Where(r => r.dataSource != null),
            dataSources = dataSources.Select(ds => new
            {
                name = ds.Name,
                type = "AWS_LAMBDA",
                serviceRoleArn = "${LambdaDataSourceRole.Arn}",
                lambdaConfig = new
                {
                    functionArn = $"${{{ds.LogicalId}.Arn}}"
                }
            }),
            functions = new object[0] // Empty for now, would be populated for pipeline resolvers
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(manifest, options);
    }

    private static List<DataSourceInfo> ExtractDataSources(List<ResolverInfo> resolvers)
    {
        var dataSources = new List<DataSourceInfo>();
        var seenDataSources = new HashSet<string>();

        foreach (var resolver in resolvers)
        {
            if (!string.IsNullOrEmpty(resolver.DataSource) && 
                !seenDataSources.Contains(resolver.DataSource))
            {
                dataSources.Add(new DataSourceInfo
                {
                    Name = resolver.DataSource,
                    LogicalId = resolver.LambdaFunctionLogicalId ?? $"{resolver.LambdaFunctionName}Function"
                });
                seenDataSources.Add(resolver.DataSource);
            }
        }

        return dataSources;
    }

    private class DataSourceInfo
    {
        public string Name { get; set; } = string.Empty;
        public string LogicalId { get; set; } = string.Empty;
    }
}
