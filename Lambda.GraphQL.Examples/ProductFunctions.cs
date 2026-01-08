using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Lambda.GraphQL.Attributes;
using System;
using System.Threading.Tasks;

namespace Lambda.GraphQL.Examples;

/// <summary>
/// Example Lambda functions with GraphQL operations.
/// </summary>
public class ProductFunctions
{
    [LambdaFunction]
    [GraphQLQuery("getProduct", Description = "Get a product by ID")]
    [GraphQLResolver(DataSource = "ProductsLambda")]
    public Task<Product> GetProduct(string id)
    {
        // TODO: Implement product retrieval
        return Task.FromResult(new Product { Id = id, Name = "Sample Product", Price = 99.99m });
    }

    [LambdaFunction]
    [GraphQLMutation("createProduct", Description = "Create a new product")]
    [GraphQLResolver(DataSource = "ProductsLambda")]
    public Task<Product> CreateProduct(CreateProductInput input)
    {
        // TODO: Implement product creation
        return Task.FromResult(new Product 
        { 
            Id = Guid.NewGuid().ToString(), 
            Name = input.Name, 
            Price = input.Price 
        });
    }
}
