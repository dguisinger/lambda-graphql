using FluentAssertions;
using Lambda.GraphQL.SourceGenerator;
using Lambda.GraphQL.SourceGenerator.Models;
using System.Collections.Generic;

namespace Lambda.GraphQL.Tests;

public class SdlGeneratorTests
{
    [Fact]
    public void GenerateSchema_ShouldCreateValidSDL()
    {
        // Arrange
        var types = new List<Lambda.GraphQL.SourceGenerator.Models.TypeInfo>
        {
            new Lambda.GraphQL.SourceGenerator.Models.TypeInfo
            {
                Name = "Product",
                Description = "A product",
                Kind = Lambda.GraphQL.SourceGenerator.Models.TypeKind.Object,
                Fields = new List<FieldInfo>
                {
                    new FieldInfo { Name = "id", Type = "ID", IsNullable = false },
                    new FieldInfo { Name = "name", Type = "String", IsNullable = false }
                }
            }
        };

        var operations = new List<ResolverInfo>
        {
            new ResolverInfo
            {
                TypeName = "Query",
                FieldName = "getProduct",
                DataSource = "ProductsLambda",
                ReturnType = "Product"
            }
        };

        // Act
        var sdl = SdlGenerator.GenerateSchema(types, operations, "TestAPI", "Test API");

        // Assert
        sdl.Should().Contain("schema {");
        sdl.Should().Contain("query: Query");
        sdl.Should().Contain("type Product {");
        sdl.Should().Contain("id: ID!");
        sdl.Should().Contain("name: String!");
        sdl.Should().Contain("type Query {");
        sdl.Should().Contain("getProduct: Product"); // Should use actual return type
    }

    [Fact]
    public void GenerateSchema_ShouldHandleEnumTypes()
    {
        // Arrange
        var types = new List<Lambda.GraphQL.SourceGenerator.Models.TypeInfo>
        {
            new Lambda.GraphQL.SourceGenerator.Models.TypeInfo
            {
                Name = "Status",
                Kind = Lambda.GraphQL.SourceGenerator.Models.TypeKind.Enum,
                IsEnum = true,
                EnumValues = new List<EnumValueInfo>
                {
                    new EnumValueInfo { Name = "ACTIVE", Description = "Active status" },
                    new EnumValueInfo { Name = "INACTIVE", IsDeprecated = true, DeprecationReason = "Use DISABLED" }
                }
            }
        };

        // Act
        var sdl = SdlGenerator.GenerateSchema(types, new List<ResolverInfo>());

        // Assert
        sdl.Should().Contain("enum Status {");
        sdl.Should().Contain("ACTIVE");
        sdl.Should().Contain("INACTIVE @deprecated(reason: \"Use DISABLED\")");
    }
}
