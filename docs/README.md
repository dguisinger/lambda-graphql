# Lambda.GraphQL Documentation

Welcome to the Lambda.GraphQL documentation! This library enables you to generate GraphQL schemas from C# Lambda functions for AWS AppSync with compile-time type safety and automatic resolver configuration.

## 📚 Documentation Sections

### Getting Started
- **[Getting Started Guide](getting-started.md)** - Installation, setup, and your first GraphQL schema
- **[Examples](examples.md)** - Working code examples from basic to advanced usage

### API Reference
- **[API Reference](api-reference.md)** - Complete API documentation for all classes and methods
- **[Attributes Reference](attributes.md)** - Detailed guide to all GraphQL attributes

### Advanced Topics
- **[Advanced Features](advanced-features.md)** - Union types, directives, subscriptions, and AWS scalars
- **[AWS Integration](aws-integration.md)** - AppSync integration and CDK deployment guides
- **[Architecture](architecture.md)** - Source generator design and system architecture

### Development
- **[Performance](performance.md)** - Performance considerations and optimization tips
- **[Troubleshooting](troubleshooting.md)** - Common issues and solutions
- **[Contributing](contributing.md)** - Development setup and contribution guidelines
- **[Migration Guide](migration.md)** - Version migration and breaking changes

## 🚀 Quick Start

```csharp
// 1. Define your GraphQL types
[GraphQLType("Product")]
public class Product
{
    [GraphQLField(Description = "Product identifier")]
    public Guid Id { get; set; }
    
    [GraphQLField(Description = "Product name")]
    public string Name { get; set; }
    
    [GraphQLField(Description = "Product price")]
    public decimal Price { get; set; }
}

// 2. Create Lambda functions with GraphQL operations
public class ProductFunctions
{
    [LambdaFunction]
    [GraphQLQuery("getProduct")]
    [GraphQLResolver(DataSource = "ProductsLambda")]
    public async Task<Product> GetProduct(
        [GraphQLArgument] Guid id)
    {
        // Your implementation here
        return new Product { Id = id, Name = "Sample", Price = 9.99m };
    }
}
```

This automatically generates:
- **GraphQL Schema** (`schema.graphql`) with proper SDL syntax
- **Resolver Manifest** (`resolvers.json`) for CDK deployment
- **Type Safety** at compile time with full IntelliSense support

## 🎯 Key Features

- **🔧 Compile-Time Generation** - GraphQL schemas generated during build
- **🛡️ Type Safety** - Full C# type safety with GraphQL schema validation
- **☁️ AWS Integration** - Native AppSync and CDK support
- **🚀 Advanced Features** - Union types, directives, subscriptions, custom scalars
- **📦 Zero Runtime Dependencies** - Pure compile-time source generation
- **🔄 AOT Compatible** - Works with Native AOT compilation

## 📖 Learn More

- Start with the **[Getting Started Guide](getting-started.md)** for installation and setup
- Explore **[Examples](examples.md)** to see the library in action
- Check **[Advanced Features](advanced-features.md)** for union types, directives, and more
- Review **[AWS Integration](aws-integration.md)** for deployment with CDK

## 🤝 Community

- **Issues**: Report bugs and request features on [GitHub Issues](https://github.com/your-org/lambda-graphql/issues)
- **Discussions**: Join the conversation on [GitHub Discussions](https://github.com/your-org/lambda-graphql/discussions)
- **Contributing**: See our [Contributing Guide](contributing.md) to get involved

---

*Lambda.GraphQL is designed to make GraphQL development with AWS AppSync as seamless and type-safe as possible for .NET developers.*
