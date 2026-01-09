using System.Text;
using System.Linq;
using System.Collections.Generic;
using Lambda.GraphQL.SourceGenerator.Models;

namespace Lambda.GraphQL.SourceGenerator;

/// <summary>
/// Generates GraphQL Schema Definition Language (SDL) from type and operation information.
/// </summary>
public static class SdlGenerator
{
    /// <summary>
    /// Generates a complete GraphQL SDL schema from types and operations.
    /// </summary>
    public static string GenerateSchema(IEnumerable<Models.TypeInfo> types, IEnumerable<ResolverInfo> operations, string? schemaName = null, string? description = null)
    {
        var sb = new StringBuilder();

        // Add schema description if provided
        if (!string.IsNullOrEmpty(description))
        {
            sb.AppendLine($"\"\"\"\n{description}\n\"\"\"");
        }

        // Generate schema definition
        var hasQuery = operations.Any(op => op.TypeName == "Query");
        var hasMutation = operations.Any(op => op.TypeName == "Mutation");
        var hasSubscription = operations.Any(op => op.TypeName == "Subscription");

        if (hasQuery || hasMutation || hasSubscription)
        {
            sb.AppendLine("schema {");
            if (hasQuery) sb.AppendLine("  query: Query");
            if (hasMutation) sb.AppendLine("  mutation: Mutation");
            if (hasSubscription) sb.AppendLine("  subscription: Subscription");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        // Generate custom types
        foreach (var type in types.OrderBy(t => t.Name))
        {
            GenerateType(sb, type);
            sb.AppendLine();
        }

        // Generate root operation types
        if (hasQuery)
        {
            GenerateRootType(sb, "Query", operations.Where(op => op.TypeName == "Query"));
            sb.AppendLine();
        }

        if (hasMutation)
        {
            GenerateRootType(sb, "Mutation", operations.Where(op => op.TypeName == "Mutation"));
            sb.AppendLine();
        }

        if (hasSubscription)
        {
            GenerateRootType(sb, "Subscription", operations.Where(op => op.TypeName == "Subscription"));
            sb.AppendLine();
        }

        return sb.ToString().Trim();
    }

    private static void GenerateType(StringBuilder sb, Models.TypeInfo type)
    {
        // Add description if present
        if (!string.IsNullOrEmpty(type.Description))
        {
            sb.AppendLine($"\"\"\"\n{type.Description}\n\"\"\"");
        }

        switch (type.Kind)
        {
            case Models.TypeKind.Object:
                GenerateObjectType(sb, type);
                break;
            case Models.TypeKind.Input:
                GenerateInputType(sb, type);
                break;
            case Models.TypeKind.Interface:
                GenerateInterfaceType(sb, type);
                break;
            case Models.TypeKind.Enum:
                GenerateEnumType(sb, type);
                break;
        }
    }

    private static void GenerateObjectType(StringBuilder sb, Models.TypeInfo type)
    {
        sb.AppendLine($"type {type.Name} {{");
        foreach (var field in type.Fields.OrderBy(f => f.Name))
        {
            GenerateField(sb, field, "  ");
        }
        sb.AppendLine("}");
    }

    private static void GenerateInputType(StringBuilder sb, Models.TypeInfo type)
    {
        sb.AppendLine($"input {type.Name} {{");
        foreach (var field in type.Fields.OrderBy(f => f.Name))
        {
            GenerateInputField(sb, field, "  ");
        }
        sb.AppendLine("}");
    }

    private static void GenerateInterfaceType(StringBuilder sb, Models.TypeInfo type)
    {
        sb.AppendLine($"interface {type.Name} {{");
        foreach (var field in type.Fields.OrderBy(f => f.Name))
        {
            GenerateField(sb, field, "  ");
        }
        sb.AppendLine("}");
    }

    private static void GenerateEnumType(StringBuilder sb, Models.TypeInfo type)
    {
        sb.AppendLine($"enum {type.Name} {{");
        foreach (var enumValue in type.EnumValues.OrderBy(e => e.Name))
        {
            if (!string.IsNullOrEmpty(enumValue.Description))
            {
                sb.AppendLine($"  \"\"\"\n  {enumValue.Description}\n  \"\"\"");
            }

            var line = $"  {enumValue.Name}";
            if (enumValue.IsDeprecated)
            {
                var reason = !string.IsNullOrEmpty(enumValue.DeprecationReason) 
                    ? $"reason: \"{enumValue.DeprecationReason}\"" 
                    : "";
                line += $" @deprecated({reason})";
            }
            sb.AppendLine(line);
        }
        sb.AppendLine("}");
    }

    private static void GenerateField(StringBuilder sb, FieldInfo field, string indent)
    {
        if (!string.IsNullOrEmpty(field.Description))
        {
            sb.AppendLine($"{indent}\"\"\"\n{indent}{field.Description}\n{indent}\"\"\"");
        }

        var fieldType = field.IsNullable ? field.Type : $"{field.Type}!";
        var line = $"{indent}{field.Name}: {fieldType}";

        if (field.IsDeprecated)
        {
            var reason = !string.IsNullOrEmpty(field.DeprecationReason) 
                ? $"reason: \"{field.DeprecationReason}\"" 
                : "";
            line += $" @deprecated({reason})";
        }

        sb.AppendLine(line);
    }

    private static void GenerateInputField(StringBuilder sb, FieldInfo field, string indent)
    {
        if (!string.IsNullOrEmpty(field.Description))
        {
            sb.AppendLine($"{indent}\"\"\"\n{indent}{field.Description}\n{indent}\"\"\"");
        }

        var fieldType = field.IsNullable ? field.Type : $"{field.Type}!";
        sb.AppendLine($"{indent}{field.Name}: {fieldType}");
    }

    private static void GenerateRootType(StringBuilder sb, string typeName, IEnumerable<ResolverInfo> operations)
    {
        sb.AppendLine($"type {typeName} {{");
        foreach (var operation in operations.OrderBy(op => op.FieldName))
        {
            sb.AppendLine($"  {operation.FieldName}: {operation.ReturnType}");
        }
        sb.AppendLine("}");
    }
}
