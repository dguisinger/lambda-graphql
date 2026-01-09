using System.Collections.Generic;

namespace Lambda.GraphQL.SourceGenerator.Models;

/// <summary>
/// Represents a GraphQL resolver configuration.
/// </summary>
public sealed class ResolverInfo
{
    public string TypeName { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public ResolverKind Kind { get; set; }
    public string? DataSource { get; set; }
    public string? LambdaFunctionName { get; set; }
    public string? LambdaFunctionLogicalId { get; set; }
    public string Runtime { get; set; } = "APPSYNC_JS";
    public string? RequestMapping { get; set; }
    public string? ResponseMapping { get; set; }
    public List<string> Functions { get; set; } = new();
    public string ReturnType { get; set; } = "String";
}

/// <summary>
/// Represents the kind of AppSync resolver.
/// </summary>
public enum ResolverKind
{
    Unit,
    Pipeline
}
