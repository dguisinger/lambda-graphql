using System;

namespace Lambda.GraphQL.Attributes;

/// <summary>
/// Marks a field as an AWS timestamp scalar (AWSTimestamp).
/// Use this for Unix timestamp values (seconds since epoch).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class GraphQLTimestampAttribute : Attribute
{
}
