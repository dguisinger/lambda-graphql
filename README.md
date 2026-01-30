# Lambda.GraphQL

A .NET library that generates GraphQL schemas from C# Lambda functions for AWS AppSync. Provides compile-time schema generation through source generators and MSBuild tasks, enabling type-safe GraphQL API development with AWS Lambda Annotations.

## ✨ Features

- **🔧 Compile-Time Generation** - GraphQL schemas generated during build with zero runtime overhead
- **🛡️ Type Safety** - Full C# type safety with GraphQL schema validation and IntelliSense
- **☁️ AWS Native** - Built specifically for AWS AppSync with CDK integration
- **🚀 Advanced GraphQL** - Union types, interfaces, directives, subscriptions, and custom scalars
- **📦 Zero Dependencies** - Pure compile-time source generation with no runtime dependencies
- **🔄 AOT Compatible** - Works seamlessly with Native AOT compilation

## 🚀 Quick Start (Hackathon Evaluation)

### Prerequisites
- .NET 6.0+ SDK
- Git

### Setup for Evaluation
```bash
# Clone the repository
git clone https://github.com/dguisinger/lambda-graphql
cd lambda-graphql

# Build the solution
dotnet build

# Run tests to verify functionality
dotnet test

# See the generated schema and resolvers
cat Lambda.GraphQL.Examples/schema.graphql
cat Lambda.GraphQL.Examples/resolvers.json

# See the generated C# source files (optional)
find Lambda.GraphQL.Examples/obj -name "*.cs" -path "*GraphQLSchemaGenerator*"
```

### Try It Yourself
```bash
# Create a new test project
dotnet new console -n MyGraphQLTest
cd MyGraphQLTest

# Add project reference (instead of NuGet package)
dotnet add reference ../Lambda.GraphQL/Lambda.GraphQL.csproj
dotnet add package Amazon.Lambda.Annotations

# Copy example code from Lambda.GraphQL.Examples
# Build and see your schema generated!
dotnet build
```

> **Note**: This is a hackathon submission - package will be published to NuGet after evaluation.

### Basic Usage

```csharp
using Lambda.GraphQL.Attributes;
using Amazon.Lambda.Annotations;

// 1. Define your GraphQL types
[GraphQLType("Product", Description = "A product in the catalog")]
public class Product
{
    [GraphQLField(Description = "Unique product identifier")]
    public Guid Id { get; set; }
    
    [GraphQLField(Description = "Product name")]
    public string Name { get; set; }
    
    [GraphQLField(Description = "Product price in USD")]
    public decimal Price { get; set; }
}

// 2. Create input types
[GraphQLType("CreateProductInput", Kind = GraphQLTypeKind.Input)]
public class CreateProductInput
{
    [GraphQLField(Description = "Product name")]
    public string Name { get; set; }
    
    [GraphQLField(Description = "Product price")]
    public decimal Price { get; set; }
}

// 3. Implement Lambda functions with GraphQL operations
public class ProductFunctions
{
    [LambdaFunction(MemorySize = 1024, Timeout = 30)]
    [GraphQLQuery("getProduct", Description = "Get a product by ID")]
    [GraphQLResolver]  // DataSource auto-generated as "GetProductDataSource"
    public async Task<Product> GetProduct(
        [GraphQLArgument(Description = "Product ID")] Guid id)
    {
        // Your implementation here
        return new Product 
        { 
            Id = id, 
            Name = "Sample Product", 
            Price = 29.99m 
        };
    }

    [LambdaFunction(MemorySize = 512, Timeout = 15)]
    [GraphQLMutation("createProduct", Description = "Create a new product")]
    [GraphQLResolver]  // DataSource auto-generated as "CreateProductDataSource"
    public async Task<Product> CreateProduct(
        [GraphQLArgument] CreateProductInput input)
    {
        // Your implementation here
        return new Product 
        { 
            Id = Guid.NewGuid(), 
            Name = input.Name, 
            Price = input.Price 
        };
    }
}
```

### Generated Output

The above code automatically generates:

**schema.graphql**
```graphql
"""
Generated GraphQL schema from Lambda functions
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
  Product name
  """
  name: String!
  """
  Product price in USD
  """
  price: Float!
}

input CreateProductInput {
  """
  Product name
  """
  name: String!
  """
  Product price
  """
  price: Float!
}

type Query {
  """
  Get a product by ID
  """
  getProduct(id: ID!): Product!
}

type Mutation {
  """
  Create a new product
  """
  createProduct(input: CreateProductInput!): Product!
}
```

**resolvers.json**
```json
{
  "resolvers": [
    {
      "typeName": "Query",
      "fieldName": "getProduct",
      "kind": "UNIT",
      "dataSource": "GetProductDataSource",
      "lambdaFunctionName": "GetProduct",
      "lambdaFunctionLogicalId": "GetProductFunction",
      "memorySize": 1024,
      "timeout": 30
    },
    {
      "typeName": "Mutation", 
      "fieldName": "createProduct",
      "kind": "UNIT",
      "dataSource": "CreateProductDataSource",
      "lambdaFunctionName": "CreateProduct",
      "lambdaFunctionLogicalId": "CreateProductFunction",
      "memorySize": 512,
      "timeout": 15
    }
  ],
  "dataSources": [
    {
      "name": "GetProductDataSource",
      "type": "AWS_LAMBDA",
      "lambdaConfig": {
        "functionArn": "${GetProductFunction.Arn}"
      }
    },
    {
      "name": "CreateProductDataSource",
      "type": "AWS_LAMBDA",
      "lambdaConfig": {
        "functionArn": "${CreateProductFunction.Arn}"
      }
    }
  ]
}
```

## 🎯 Advanced Features

### Lambda Annotations Configuration
```csharp
[LambdaFunction(
    MemorySize = 2048,
    Timeout = 60,
    Policies = new[] { "AmazonDynamoDBFullAccess" }
)]
[GraphQLQuery("complexQuery")]
public async Task<Result> ComplexQuery(string param) { }
```
Configuration automatically flows to deployed Lambda functions via CDK.

### Union Types
```csharp
[GraphQLUnion("SearchResult", "Product", "User", "Order")]
public class SearchResult { }

[LambdaFunction]
[GraphQLQuery("search")]
public async Task<List<object>> Search(string term)
{
    // Return mixed types - AppSync handles union resolution
    return results;
}
```

### AWS Scalar Types
```csharp
[GraphQLType("User")]
public class User
{
    [GraphQLField] public Guid Id { get; set; }           // → ID!
    [GraphQLField] public DateTime CreatedAt { get; set; } // → AWSDateTime!
    [GraphQLField] public DateOnly BirthDate { get; set; } // → AWSDate
    [GraphQLField] public System.Net.Mail.MailAddress Email { get; set; } // → AWSEmail!
    
    [GraphQLField]
    [GraphQLTimestamp]
    public long LastLoginTimestamp { get; set; }           // → AWSTimestamp!
}
```

### Subscriptions
```csharp
[LambdaFunction]
[GraphQLSubscription("orderStatusChanged")]
[GraphQLResolver(DataSource = "OrderSubscriptionLambda")]
public async Task<Order> OrderStatusChanged(Guid orderId)
{
    // Subscription implementation
}
```

### Auth Directives
```csharp
[GraphQLType("AdminData")]
[GraphQLAuthDirective(AuthMode.UserPools, CognitoGroups = "admin")]
public class AdminData
{
    [GraphQLField] public string SensitiveInfo { get; set; }
}
```

## 📖 Documentation

- **[📚 Full Documentation](docs/README.md)** - Complete documentation hub
- **[🚀 Getting Started](docs/getting-started.md)** - Detailed setup and installation guide
- **[📋 API Reference](docs/api-reference.md)** - Complete API documentation
- **[🔧 Advanced Features](docs/advanced-features.md)** - Union types, directives, subscriptions
- **[☁️ AWS Integration](docs/aws-integration.md)** - AppSync and CDK deployment guides
- **[🏗️ Architecture](docs/architecture.md)** - Source generator design and internals

## 🛠️ Requirements

- **.NET 6.0+** - For source generator support
- **C# 10+** - For nullable reference types and modern language features
- **AWS Lambda Annotations** - For Lambda function definitions

## 📦 Packages

- **Lambda.GraphQL** - Main package with attributes and source generator
- **Lambda.GraphQL.Build** - MSBuild tasks for schema extraction
- **Lambda.GraphQL.SourceGenerator** - Roslyn source generator (included automatically)

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](docs/contributing.md) for details on:

- Setting up the development environment
- Running tests and validation
- Submitting pull requests
- Code review process

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built for the AWS AppSync and Lambda ecosystem
- Inspired by GraphQL best practices and .NET source generator patterns
- Developed as part of the Dynamous Kiro Hackathon

---

**Ready to get started?** Check out our [Getting Started Guide](docs/getting-started.md) or explore the [Examples](docs/examples.md)!
