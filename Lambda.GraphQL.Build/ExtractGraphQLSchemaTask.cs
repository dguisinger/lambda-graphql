using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Lambda.GraphQL.Build;

/// <summary>
/// MSBuild task that extracts GraphQL schema from compiled assemblies.
/// </summary>
public class ExtractGraphQLSchemaTask : Task
{
    /// <summary>
    /// Path to the assembly to extract schema from.
    /// </summary>
    [Required]
    public string? AssemblyPath { get; set; }

    /// <summary>
    /// Output directory for generated schema files.
    /// </summary>
    [Required]
    public string? OutputDirectory { get; set; }

    /// <summary>
    /// Name of the generated schema file.
    /// </summary>
    public string SchemaFileName { get; set; } = "schema.graphql";

    /// <summary>
    /// Name of the generated resolver manifest file.
    /// </summary>
    public string ResolverFileName { get; set; } = "resolvers.json";

    public override bool Execute()
    {
        try
        {
            if (string.IsNullOrEmpty(AssemblyPath) || string.IsNullOrEmpty(OutputDirectory))
            {
                Log.LogError("AssemblyPath and OutputDirectory are required.");
                return false;
            }

            if (!File.Exists(AssemblyPath))
            {
                Log.LogWarning($"Assembly not found: {AssemblyPath}. Skipping schema extraction.");
                return true; // Not an error - assembly might not have GraphQL content
            }

            Log.LogMessage(MessageImportance.High, $"Extracting GraphQL schema from {AssemblyPath}");

            // Ensure output directory exists
            Directory.CreateDirectory(OutputDirectory);

            // Load assembly and extract schema metadata
            var (sdl, resolverManifest) = ExtractSchemaFromAssembly(AssemblyPath);

            if (!string.IsNullOrEmpty(sdl))
            {
                var schemaPath = Path.Combine(OutputDirectory, SchemaFileName);
                File.WriteAllText(schemaPath, sdl);
                Log.LogMessage(MessageImportance.High, $"Generated GraphQL schema: {schemaPath}");
            }

            if (!string.IsNullOrEmpty(resolverManifest))
            {
                var resolverPath = Path.Combine(OutputDirectory, ResolverFileName);
                File.WriteAllText(resolverPath, resolverManifest);
                Log.LogMessage(MessageImportance.High, $"Generated resolver manifest: {resolverPath}");
            }

            if (string.IsNullOrEmpty(sdl) && string.IsNullOrEmpty(resolverManifest))
            {
                Log.LogMessage(MessageImportance.Low, "No GraphQL schema found in assembly.");
            }

            return true;
        }
        catch (Exception ex)
        {
            Log.LogError($"Error extracting GraphQL schema: {ex.Message}");
            return false;
        }
    }

    private (string? sdl, string? resolverManifest) ExtractSchemaFromAssembly(string assemblyPath)
    {
        try
        {
            // Load assembly using MetadataLoadContext to avoid loading dependencies
            using var metadataLoadContext = new System.Reflection.MetadataLoadContext(
                new System.Reflection.PathAssemblyResolver(new[] { assemblyPath }));
            
            var assembly = metadataLoadContext.LoadFromAssemblyPath(assemblyPath);

            // Look for generated schema attributes
            var sdl = ExtractMetadataValue(assembly, "GraphQL.Schema");
            var resolverManifest = ExtractMetadataValue(assembly, "GraphQL.ResolverManifest");

            return (sdl, resolverManifest);
        }
        catch (Exception ex)
        {
            Log.LogWarning($"Could not extract schema using MetadataLoadContext: {ex.Message}");
            
            // Fallback: try to load assembly directly (may fail with dependencies)
            try
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                var sdl = ExtractMetadataValue(assembly, "GraphQL.Schema");
                var resolverManifest = ExtractMetadataValue(assembly, "GraphQL.ResolverManifest");
                return (sdl, resolverManifest);
            }
            catch (Exception fallbackEx)
            {
                Log.LogWarning($"Fallback assembly loading also failed: {fallbackEx.Message}");
                return (null, null);
            }
        }
    }

    private string? ExtractMetadataValue(Assembly assembly, string key)
    {
        try
        {
            var attributes = assembly.GetCustomAttributes<AssemblyMetadataAttribute>();
            var metadataAttr = attributes.FirstOrDefault(attr => attr.Key == key);
            return metadataAttr?.Value;
        }
        catch (Exception ex)
        {
            Log.LogWarning($"Could not extract metadata '{key}': {ex.Message}");
            return null;
        }
    }
}
