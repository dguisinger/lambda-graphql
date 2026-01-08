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

    [Fact]
    public void GraphQLSchemaAttribute_ShouldSetNameAndDescription()
    {
        // Arrange & Act
        var attribute = new GraphQLSchemaAttribute("TestAPI")
        {
            Description = "Test GraphQL API",
            Version = "1.0.0"
        };

        // Assert
        attribute.Name.Should().Be("TestAPI");
        attribute.Description.Should().Be("Test GraphQL API");
        attribute.Version.Should().Be("1.0.0");
    }

    [Fact]
    public void GraphQLArgumentAttribute_ShouldSetNameAndDescription()
    {
        // Arrange & Act
        var attribute = new GraphQLArgumentAttribute("id")
        {
            Description = "Product identifier"
        };

        // Assert
        attribute.Name.Should().Be("id");
        attribute.Description.Should().Be("Product identifier");
    }

    [Fact]
    public void GraphQLIgnoreAttribute_ShouldBeMarkerAttribute()
    {
        // Arrange & Act
        var attribute = new GraphQLIgnoreAttribute();

        // Assert
        attribute.Should().NotBeNull();
    }

    [Fact]
    public void GraphQLNonNullAttribute_ShouldBeMarkerAttribute()
    {
        // Arrange & Act
        var attribute = new GraphQLNonNullAttribute();

        // Assert
        attribute.Should().NotBeNull();
    }

    [Fact]
    public void GraphQLEnumValueAttribute_ShouldSetNameAndDescription()
    {
        // Arrange & Act
        var attribute = new GraphQLEnumValueAttribute("ACTIVE")
        {
            Description = "Active status",
            Deprecated = true,
            DeprecationReason = "Use ENABLED instead"
        };

        // Assert
        attribute.Name.Should().Be("ACTIVE");
        attribute.Description.Should().Be("Active status");
        attribute.Deprecated.Should().BeTrue();
        attribute.DeprecationReason.Should().Be("Use ENABLED instead");
    }
}
