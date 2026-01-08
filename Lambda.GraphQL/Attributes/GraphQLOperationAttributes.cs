using System;

namespace Lambda.GraphQL.Attributes;

/// <summary>
/// Marks a Lambda function as a GraphQL query operation.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class GraphQLQueryAttribute : Attribute
{
    public GraphQLQueryAttribute(string? name = null)
    {
        Name = name;
    }

    public string? Name { get; }
    public string? Description { get; set; }
}

/// <summary>
/// Marks a Lambda function as a GraphQL mutation operation.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class GraphQLMutationAttribute : Attribute
{
    public GraphQLMutationAttribute(string? name = null)
    {
        Name = name;
    }

    public string? Name { get; }
    public string? Description { get; set; }
}
