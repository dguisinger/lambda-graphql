using System;

namespace Lambda.GraphQL.Attributes;

/// <summary>
/// Marks a Lambda function as a GraphQL subscription operation.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class GraphQLSubscriptionAttribute : Attribute
{
    public GraphQLSubscriptionAttribute(string? name = null)
    {
        Name = name;
    }

    public string? Name { get; }
    public string? Description { get; set; }
}
