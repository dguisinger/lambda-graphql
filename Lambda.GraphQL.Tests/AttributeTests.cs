using FluentAssertions;
using Lambda.GraphQL.Attributes;

namespace Lambda.GraphQL.Tests;

public class AttributeTests
{
    [Fact]
    public void GraphQLTypeAttribute_ShouldSetNameAndDescription()
    {
        // Arrange & Act
        var attribute = new GraphQLTypeAttribute("Product")
        {
            Description = "A product type",
            Kind = GraphQLTypeKind.Object
        };

        // Assert
        attribute.Name.Should().Be("Product");
        attribute.Description.Should().Be("A product type");
        attribute.Kind.Should().Be(GraphQLTypeKind.Object);
    }

    [Fact]
    public void GraphQLFieldAttribute_ShouldSetNameAndDescription()
    {
        // Arrange & Act
        var attribute = new GraphQLFieldAttribute("productName")
        {
            Description = "The name of the product"
        };

        // Assert
        attribute.Name.Should().Be("productName");
        attribute.Description.Should().Be("The name of the product");
    }

    [Fact]
    public void GraphQLQueryAttribute_ShouldSetNameAndDescription()
    {
        // Arrange & Act
        var attribute = new GraphQLQueryAttribute("getProduct")
        {
            Description = "Get a product by ID"
        };

        // Assert
        attribute.Name.Should().Be("getProduct");
        attribute.Description.Should().Be("Get a product by ID");
    }
}
