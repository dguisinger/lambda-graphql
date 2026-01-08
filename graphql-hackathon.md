# Lambda.GraphQL - Schema Generator for AWS AppSync

## Overview

A .NET library that generates GraphQL schemas from AWS Lambda functions using AppSync. This library provides compile-time schema generation through source generators and MSBuild tasks, enabling type-safe GraphQL API development with AWS Lambda Annotations.

## Goals

1. **Schema Generation** - Generate GraphQL SDL schemas from C# types and Lambda function attributes
2. **Resolver Tracking** - Track AppSync resolver configurations (unit resolvers, pipeline resolvers)
3. **Type Safety** - Compile-time validation of GraphQL types and resolver mappings
4. **CDK Integration** - Generate CDK-compatible resolver configurations
5. **AOT Compatibility** - Full support for Native AOT compilation

## Architecture

### Package Structure

```
Lambda.GraphQL/
├── Lambda.GraphQL.sln
├── Lambda.GraphQL/                          # Main package (attributes + bundled components)
│   ├── Attributes/
│   │   ├── GraphQLSchemaAttribute.cs        # Assembly-level schema info
│   │   ├── GraphQLTypeAttribute.cs          # Type definitions (Object, Input, Interface, Enum)
│   │   ├── GraphQLFieldAttribute.cs         # Field metadata
│   │   ├── GraphQLArgumentAttribute.cs      # Argument definitions
│   │   ├── GraphQLResolverAttribute.cs      # Resolver configuration
│   │   ├── GraphQLQueryAttribute.cs         # Query operation marker
│   │   ├── GraphQLMutationAttribute.cs      # Mutation operation marker
│   │   ├── GraphQLSubscriptionAttribute.cs  # Subscription operation marker
│   │   ├── GraphQLDirectiveAttribute.cs     # Custom directive definitions
│   │   ├── GraphQLDeprecatedAttribute.cs    # Deprecation marker
│   │   ├── GraphQLIgnoreAttribute.cs        # Exclude from schema
│   │   ├── GraphQLNonNullAttribute.cs       # Non-null override
│   │   ├── GraphQLOutputAttribute.cs        # Generated output marker (internal)
│   │   └── GraphQLAuthDirectiveAttribute.cs # AppSync auth directive
│   └── build/
│       ├── Lambda.GraphQL.props
│       └── Lambda.GraphQL.targets
├── Lambda.GraphQL.Build/                    # MSBuild task for schema extraction
│   └── ExtractGraphQLSchemaTask.cs
├── Lambda.GraphQL.SourceGenerator/          # Roslyn source generator
│   ├── GraphQLSchemaGenerator.cs
│   ├── GraphQLSchemaGenerator_Types.cs
│   ├── GraphQLSchemaGenerator_Resolvers.cs
│   ├── GraphQLSchemaGenerator_Directives.cs
│   └── Models/
│       ├── TypeInfo.cs
│       ├── FieldInfo.cs
│       ├── ResolverInfo.cs
│       └── DirectiveInfo.cs
├── Lambda.GraphQL.Tests/                    # Unit and property-based tests
├── Lambda.GraphQL.Examples/                 # Example Lambda project
└── docs/
    ├── getting-started.md
    ├── attributes.md
    ├── resolvers.md
    └── cdk-integration.md
```

## Core Concepts

### 1. Schema Generation Pipeline

```
┌─────────────────────────────────────────────────────────────────┐
│  Source Code                                                     │
│  • C# classes with [GraphQLType] attributes                     │
│  • Lambda functions with [GraphQLQuery/Mutation] attributes     │
│  • Assembly-level [GraphQLSchema] configuration                 │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  Source Generator (compile-time)                                │
│  • Analyzes syntax trees and semantic models                    │
│  • Extracts type definitions and resolver mappings              │
│  • Generates GraphQL SDL as assembly attribute                  │
│  • Generates resolver manifest JSON                             │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  MSBuild Task (post-build)                                      │
│  • Extracts SDL from generated source or assembly               │
│  • Writes schema.graphql to output directory                    │
│  • Writes resolvers.json manifest                               │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  Output Files                                                    │
│  • schema.graphql - GraphQL SDL schema                          │
│  • resolvers.json - Resolver configuration manifest             │
└─────────────────────────────────────────────────────────────────┘
```

### 2. Resolver Tracking

AppSync resolvers need to be tracked for CDK deployment. The generator produces a `resolvers.json` manifest:

```json
{
  "resolvers": [
    {
      "typeName": "Query",
      "fieldName": "getProduct",
      "kind": "UNIT",
      "dataSource": "ProductsLambda",
      "lambdaFunction": "GetProduct",
      "runtime": {
        "name": "APPSYNC_JS",
        "runtimeVersion": "1.0.0"
      }
    },
    {
      "typeName": "Mutation",
      "fieldName": "createOrder",
      "kind": "PIPELINE",
      "functions": ["ValidateOrder", "CreateOrder", "NotifyCustomer"]
    }
  ],
  "dataSources": [
    {
      "name": "ProductsLambda",
      "type": "AWS_LAMBDA",
      "lambdaConfig": {
        "functionArn": "${ProductsFunction.Arn}"
      }
    }
  ]
}
```

### 3. Type Mapping

| C# Type | GraphQL Type |
|---------|--------------|
| `string` | `String` |
| `int`, `long` | `Int` |
| `float`, `double`, `decimal` | `Float` |
| `bool` | `Boolean` |
| `Guid`, `Ulid` | `ID` |
| `DateTime`, `DateTimeOffset` | `AWSDateTime` |
| `DateOnly` | `AWSDate` |
| `TimeOnly` | `AWSTime` |
| `T?` (nullable) | `T` (nullable in GraphQL) |
| `T` (non-nullable value type) | `T!` |
| `List<T>`, `T[]` | `[T]` |
| `Dictionary<string, T>` | `AWSJSON` |
| Custom class with `[GraphQLType]` | Custom Object/Input type |
| Enum | GraphQL Enum |

## Attribute Reference

### Assembly-Level Attributes

#### GraphQLSchemaAttribute

```csharp
[assembly: GraphQLSchema("ProductsAPI",
    Description = "Product catalog GraphQL API",
    Version = "1.0.0")]
```

### Type Attributes

#### GraphQLTypeAttribute

```csharp
// Object type (default)
[GraphQLType("Product", Description = "A product in the catalog")]
public class Product
{
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// Input type
[GraphQLType("CreateProductInput", Kind = GraphQLTypeKind.Input)]
public class CreateProductInput
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// Interface
[GraphQLType("Node", Kind = GraphQLTypeKind.Interface)]
public interface INode
{
    string Id { get; }
}

// Enum (auto-detected from C# enum)
[GraphQLType("OrderStatus")]
public enum OrderStatus
{
    [GraphQLEnumValue(Description = "Order is pending")]
    Pending,
    [GraphQLEnumValue(Deprecated = "Use Shipped instead")]
    Processing,
    Shipped,
    Delivered
}
```

#### GraphQLFieldAttribute

```csharp
[GraphQLType("Product")]
public class Product
{
    [GraphQLField(Description = "Unique product identifier")]
    public string Id { get; set; }

    [GraphQLField("displayName", Description = "Product display name")]
    public string Name { get; set; }

    [GraphQLField(Deprecated = "Use displayPrice instead")]
    public decimal Price { get; set; }

    [GraphQLIgnore]
    public DateTime InternalTimestamp { get; set; }

    [GraphQLNonNull]
    public string? RequiredField { get; set; }  // Override nullability
}
```

### Operation Attributes

#### GraphQLQueryAttribute / GraphQLMutationAttribute

```csharp
public class ProductFunctions
{
    [LambdaFunction]
    [GraphQLQuery("getProduct", Description = "Get a product by ID")]
    [GraphQLResolver(DataSource = "ProductsLambda")]
    public Task<Product> GetProduct(
        [GraphQLArgument(Description = "Product ID")] string id)
    {
        // Implementation
    }

    [LambdaFunction]
    [GraphQLMutation("createProduct")]
    [GraphQLResolver(DataSource = "ProductsLambda")]
    public Task<Product> CreateProduct(
        [GraphQLArgument] CreateProductInput input)
    {
        // Implementation
    }

    [LambdaFunction]
    [GraphQLQuery("listProducts")]
    [GraphQLResolver(DataSource = "ProductsLambda")]
    public Task<ProductConnection> ListProducts(
        [GraphQLArgument] int? first,
        [GraphQLArgument] string? after)
    {
        // Implementation
    }
}
```

### Resolver Attributes

#### GraphQLResolverAttribute

```csharp
// Unit resolver (default)
[GraphQLResolver(DataSource = "ProductsLambda")]

// Unit resolver with custom code
[GraphQLResolver(
    DataSource = "ProductsLambda",
    RequestMapping = "custom-request.js",
    ResponseMapping = "custom-response.js")]

// Pipeline resolver
[GraphQLResolver(
    Kind = ResolverKind.Pipeline,
    Functions = new[] { "ValidateInput", "ProcessOrder", "SendNotification" })]
```

#### GraphQLAuthDirectiveAttribute

```csharp
// Cognito user pools auth
[GraphQLAuthDirective(AuthMode = AuthMode.UserPools)]

// IAM auth
[GraphQLAuthDirective(AuthMode = AuthMode.IAM)]

// API key auth
[GraphQLAuthDirective(AuthMode = AuthMode.ApiKey)]

// Multiple auth modes
[GraphQLAuthDirective(AuthMode = AuthMode.UserPools)]
[GraphQLAuthDirective(AuthMode = AuthMode.IAM)]
```

### Directive Attributes

#### GraphQLDirectiveAttribute

```csharp
// Custom directive definition
[assembly: GraphQLDirective("auth",
    Locations = DirectiveLocation.FieldDefinition,
    Arguments = "requires: String!")]

// Usage on field
[GraphQLField]
[GraphQLApplyDirective("auth", Arguments = "requires: \"ADMIN\"")]
public string AdminOnlyField { get; set; }
```

## Generated Output

### schema.graphql

```graphql
"""
Product catalog GraphQL API
"""
schema {
  query: Query
  mutation: Mutation
}

"""
A product in the catalog
"""
type Product {
  """
  Unique product identifier
  """
  id: ID!
  
  """
  Product display name
  """
  displayName: String!
  
  """
  @deprecated Use displayPrice instead
  """
  price: Float! @deprecated(reason: "Use displayPrice instead")
  
  displayPrice: String!
}

input CreateProductInput {
  name: String!
  price: Float!
}

type Query {
  """
  Get a product by ID
  """
  getProduct(
    """
    Product ID
    """
    id: ID!
  ): Product
  
  listProducts(first: Int, after: String): ProductConnection!
}

type Mutation {
  createProduct(input: CreateProductInput!): Product!
}

type ProductConnection {
  edges: [ProductEdge!]!
  pageInfo: PageInfo!
}

type ProductEdge {
  node: Product!
  cursor: String!
}

type PageInfo {
  hasNextPage: Boolean!
  endCursor: String
}

enum OrderStatus {
  """
  Order is pending
  """
  PENDING
  
  """
  @deprecated Use SHIPPED instead
  """
  PROCESSING @deprecated(reason: "Use SHIPPED instead")
  
  SHIPPED
  DELIVERED
}
```

### resolvers.json

```json
{
  "$schema": "https://lambda-graphql.dev/schemas/resolvers.json",
  "version": "1.0.0",
  "generatedAt": "2026-01-08T12:00:00Z",
  "resolvers": [
    {
      "typeName": "Query",
      "fieldName": "getProduct",
      "kind": "UNIT",
      "dataSource": "ProductsLambda",
      "lambdaFunctionName": "GetProduct",
      "lambdaFunctionLogicalId": "GetProductFunction",
      "runtime": "APPSYNC_JS"
    },
    {
      "typeName": "Query",
      "fieldName": "listProducts",
      "kind": "UNIT",
      "dataSource": "ProductsLambda",
      "lambdaFunctionName": "ListProducts",
      "lambdaFunctionLogicalId": "ListProductsFunction",
      "runtime": "APPSYNC_JS"
    },
    {
      "typeName": "Mutation",
      "fieldName": "createProduct",
      "kind": "UNIT",
      "dataSource": "ProductsLambda",
      "lambdaFunctionName": "CreateProduct",
      "lambdaFunctionLogicalId": "CreateProductFunction",
      "runtime": "APPSYNC_JS"
    }
  ],
  "dataSources": [
    {
      "name": "ProductsLambda",
      "type": "AWS_LAMBDA",
      "serviceRoleArn": "${LambdaDataSourceRole.Arn}",
      "lambdaConfig": {
        "functionArn": "${ProductsFunction.Arn}"
      }
    }
  ],
  "functions": []
}
```

## CDK Integration

The generated `resolvers.json` can be consumed by CDK to create AppSync resources:

```csharp
// In CDK stack
var schema = SchemaFile.FromAsset("path/to/schema.graphql");
var resolverManifest = ResolverManifest.FromFile("path/to/resolvers.json");

var api = new GraphqlApi(this, "ProductsApi", new GraphqlApiProps
{
    Name = "products-api",
    Schema = schema,
    AuthorizationConfig = new AuthorizationConfig
    {
        DefaultAuthorization = new AuthorizationMode
        {
            AuthorizationType = AuthorizationType.USER_POOL,
            UserPoolConfig = new UserPoolConfig { UserPool = userPool }
        }
    }
});

// Auto-create resolvers from manifest
foreach (var resolver in resolverManifest.Resolvers)
{
    var dataSource = api.AddLambdaDataSource(
        resolver.DataSource,
        lambdaFunctions[resolver.LambdaFunctionLogicalId]);
    
    dataSource.CreateResolver(resolver.TypeName, resolver.FieldName);
}
```

## Implementation Phases

### Phase 1: Core Schema Generation (MVP)

**Goal:** Generate valid GraphQL SDL from C# types and Lambda functions

- [ ] Project structure setup
- [ ] Core attributes (GraphQLType, GraphQLField, GraphQLQuery, GraphQLMutation)
- [ ] Source generator for type extraction
- [ ] SDL generation for basic types (Object, Input, Enum)
- [ ] MSBuild task for schema extraction
- [ ] Basic tests

**Deliverables:**
- `Lambda.GraphQL` package
- `Lambda.GraphQL.Build` package
- `Lambda.GraphQL.SourceGenerator` package
- `schema.graphql` output

### Phase 2: Resolver Tracking

**Goal:** Track resolver configurations for CDK deployment

- [ ] GraphQLResolverAttribute implementation
- [ ] Resolver manifest generation (resolvers.json)
- [ ] Data source tracking
- [ ] Lambda function logical ID mapping
- [ ] Pipeline resolver support

**Deliverables:**
- `resolvers.json` output
- CDK integration documentation

### Phase 3: Advanced Features

**Goal:** Full GraphQL feature support

- [ ] Interface types
- [ ] Union types
- [ ] Custom scalars (AWS types)
- [ ] Directives (built-in and custom)
- [ ] Subscriptions
- [ ] Field-level auth directives
- [ ] Relay connection pattern helpers

## Technical Considerations

### Source Generator Design

The source generator uses Roslyn's incremental generator pattern:

```csharp
[Generator(LanguageNames.CSharp)]
public partial class GraphQLSchemaGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. Find classes with GraphQL attributes
        var typeDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (s, _) => IsGraphQLType(s),
                transform: (ctx, _) => ExtractTypeInfo(ctx))
            .Where(t => t != null);

        // 2. Find Lambda functions with GraphQL operation attributes
        var operationDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (s, _) => IsGraphQLOperation(s),
                transform: (ctx, _) => ExtractOperationInfo(ctx))
            .Where(o => o != null);

        // 3. Combine and generate schema
        var combined = typeDeclarations.Collect()
            .Combine(operationDeclarations.Collect())
            .Combine(context.CompilationProvider);

        context.RegisterSourceOutput(combined, GenerateSchema);
    }
}
```

### AOT Compatibility

- All attributes use simple types (strings, enums, primitives)
- No reflection at runtime
- Schema extraction via source file parsing (primary) or MetadataLoadContext (fallback)
- Generated code is AOT-safe

### Nullability Handling

GraphQL nullability is the inverse of C#:
- C# `string` (nullable reference type) → GraphQL `String` (nullable)
- C# `string?` (explicit nullable) → GraphQL `String` (nullable)
- C# `int` (non-nullable value type) → GraphQL `Int!` (non-null)
- C# `int?` (nullable value type) → GraphQL `Int` (nullable)

The generator respects C# nullable annotations and provides `[GraphQLNonNull]` for overrides.

### AppSync-Specific Features

- AWS scalar types (AWSDateTime, AWSDate, AWSTime, AWSJSON, AWSEmail, etc.)
- Auth directives (@aws_auth, @aws_cognito_user_pools, @aws_iam, @aws_api_key)
- Subscription support with @aws_subscribe directive
- Pipeline resolver configuration

## Testing Strategy

### Unit Tests
- Attribute parsing
- Type mapping
- SDL generation
- Resolver manifest generation

### Property-Based Tests (FsCheck)
- Deterministic output for same input
- Valid SDL syntax for all generated schemas
- Resolver manifest schema compliance

### Integration Tests
- End-to-end schema generation
- CDK deployment validation
- AppSync API creation

## Dependencies

### Runtime Dependencies
- None (attributes only)

### Build Dependencies
- Microsoft.CodeAnalysis.CSharp (source generator)
- Microsoft.Build.Framework (MSBuild task)
- System.Reflection.MetadataLoadContext (fallback extraction)

### Test Dependencies
- xUnit
- FsCheck.Xunit
- FluentAssertions

## Open Questions

1. **Schema-first vs Code-first:** Should we support importing existing `.graphql` files and generating C# stubs?

2. **Federation Support:** Should we support Apollo Federation directives (@key, @external, @requires)?

3. **Subscription Resolver Pattern:** How should we handle subscription resolvers differently from query/mutation?

4. **Custom Scalar Registration:** How should users register custom scalar types and their serialization?

5. **Validation:** Should we validate the generated schema against AppSync limitations at compile time?
