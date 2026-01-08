using System.Collections.Generic;

namespace Lambda.GraphQL.SourceGenerator.Models;

/// <summary>
/// Represents a GraphQL type extracted from C# code.
/// </summary>
public sealed class TypeInfo
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TypeKind Kind { get; set; }
    public List<FieldInfo> Fields { get; set; } = new();
    public bool IsInterface { get; set; }
    public bool IsEnum { get; set; }
    public List<EnumValueInfo> EnumValues { get; set; } = new();
}

/// <summary>
/// Represents a GraphQL enum value.
/// </summary>
public sealed class EnumValueInfo
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDeprecated { get; set; }
    public string? DeprecationReason { get; set; }
}

/// <summary>
/// Represents a GraphQL field.
/// </summary>
public sealed class FieldInfo
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public bool IsDeprecated { get; set; }
    public string? DeprecationReason { get; set; }
}

/// <summary>
/// Represents the kind of GraphQL type.
/// </summary>
public enum TypeKind
{
    Object,
    Input,
    Interface,
    Enum
}
