# Resolver Manifest (resolvers.json)

The `resolvers.json` file is generated during build and provides metadata for CDK deployment of your GraphQL API.

## Purpose

This manifest bridges the gap between your C# Lambda functions and AWS AppSync infrastructure, enabling automated CDK deployment.

## Key Fields Explained

### Resolver Configuration

```json
{
  "typeName": "Query",
  "fieldName": "getProduct",
  "kind": "UNIT",
  "dataSource": "ProductsLambda",
  "lambdaFunctionName": "GetProduct",
  "lambdaFunctionLogicalId": "GetProductFunction",
  "runtime": "APPSYNC_JS"
}
```

| Field | Description |
|-------|-------------|
| `typeName` | GraphQL root type (`Query`, `Mutation`, `Subscription`) |
| `fieldName` | GraphQL field name |
| `kind` | Resolver type: `UNIT` (direct) or `PIPELINE` (multi-step) |
| `dataSource` | AppSync data source name to invoke |
| `lambdaFunctionName` | C# method name (used for `ANNOTATIONS_HANDLER` env var) |
| `lambdaFunctionLogicalId` | CDK logical ID for the Lambda function resource |
| `runtime` | **AppSync resolver runtime** (JavaScript), NOT Lambda runtime |

### Important: Runtime Clarification

- **`runtime: "APPSYNC_JS"`** refers to the **AppSync resolver runtime** (the JavaScript code that runs in AppSync before/after calling your Lambda)
- Your Lambda functions still run on **.NET 6+** runtime
- AppSync uses JavaScript resolvers to transform requests/responses between GraphQL and Lambda

## Lambda Annotations Architecture

With Lambda Annotations, **each `[LambdaFunction]` method becomes a separate Lambda function**:

```csharp
[LambdaFunction]
[GraphQLQuery("getProduct")]
[GraphQLResolver(DataSource = "ProductsLambda")]
public Task<Product> GetProduct(string id) { }

[LambdaFunction]
[GraphQLQuery("getUser")]
[GraphQLResolver(DataSource = "UserLambda")]
public Task<User> GetUser(Guid id) { }
```

This generates:
- **Two separate Lambda functions** (`GetProductFunction`, `GetUserFunction`)
- **Two separate data sources** (`ProductsLambda`, `UserLambda`)
- **Two resolvers** mapping GraphQL fields to their respective data sources

## Data Source Naming

### Option 1: Explicit (Recommended for Grouping)
```csharp
[GraphQLResolver(DataSource = "ProductsLambda")]
```
Use when you want a logical grouping name (even though each Lambda is separate).

### Option 2: Auto-Generated
```csharp
[GraphQLResolver] // Auto-generates: "GetProductDataSource"
```
If omitted, data source name defaults to `{MethodName}DataSource`.

**Note:** Even with the same `DataSource` name, the CDK creates separate data sources per Lambda function. The name is primarily for organizational clarity.

## CDK Integration

The CDK stack uses this manifest to:

1. **Create Lambda functions** - One per resolver using `lambdaFunctionLogicalId`
2. **Create data sources** - One per Lambda function
3. **Create resolvers** - Link GraphQL fields to data sources
4. **Set environment variables** - `ANNOTATIONS_HANDLER={lambdaFunctionName}` for routing

### Lambda Annotations Routing

Each Lambda function is deployed with:
```typescript
handler: 'Lambda.GraphQL.Examples', // Assembly name
environment: {
  ANNOTATIONS_HANDLER: 'GetProduct' // Routes to specific method
}
```

Lambda Annotations uses this to route requests to the correct C# method within the assembly.

## Pipeline Resolvers

For advanced scenarios, you can create pipeline resolvers:

```csharp
[GraphQLResolver(Kind = ResolverKind.Pipeline, Functions = new[] { "AuthFunction", "DataFunction" })]
```

This creates a multi-step resolver that executes functions in sequence.

## Schema Validation

The manifest includes:
- `$schema` - JSON schema URL for validation
- `version` - Manifest format version
- `generatedAt` - Build timestamp

## Example Output

```json
{
  "$schema": "https://lambda-graphql.dev/schemas/resolvers.json",
  "version": "1.0.0",
  "generatedAt": "2026-01-30T15:33:55Z",
  "resolvers": [
    {
      "typeName": "Query",
      "fieldName": "getProduct",
      "kind": "UNIT",
      "dataSource": "ProductsLambda",
      "lambdaFunctionName": "GetProduct",
      "lambdaFunctionLogicalId": "GetProductFunction",
      "runtime": "APPSYNC_JS"
    }
  ],
  "dataSources": [
    {
      "name": "ProductsLambda",
      "type": "AWS_LAMBDA",
      "serviceRoleArn": "${LambdaDataSourceRole.Arn}",
      "lambdaConfig": {
        "functionArn": "${GetProductFunction.Arn}"
      }
    }
  ],
  "functions": []
}
```

## Best Practices

1. **Use explicit data source names** for clarity in CloudWatch logs and metrics
2. **Keep resolver logic simple** - complex logic belongs in your C# Lambda functions
3. **One Lambda per GraphQL field** - aligns with Lambda Annotations architecture
4. **Use pipeline resolvers** only when you need multi-step processing (auth + data, etc.)
