# Feature: Complete Core Attributes and Implement Source Generator

The following plan should be complete, but its important that you validate documentation and codebase patterns and task sanity before you start implementing.

Pay special attention to naming of existing utils types and models. Import from the right files etc.

## Feature Description

Complete the implementation of core GraphQL attributes according to the comprehensive specification in graphql-hackathon.md, validate existing attributes are not placeholders, and implement a fully functional Roslyn source generator that analyzes C# code with GraphQL attributes to generate GraphQL SDL schemas and resolver manifests.

## User Story

As a .NET developer using AWS AppSync and Lambda
I want to annotate my C# classes and Lambda functions with GraphQL attributes
So that GraphQL schemas and resolver configurations are automatically generated at compile time

## Problem Statement

The current Lambda.GraphQL project has basic attribute scaffolding and a placeholder source generator, but lacks the core functionality needed to actually generate GraphQL schemas from C# code. The attributes may be incomplete compared to the comprehensive specification, and the source generator contains only TODO placeholders.

## Solution Statement

Validate and complete all core GraphQL attributes per the specification, then implement a full Roslyn incremental source generator that analyzes syntax trees, extracts GraphQL metadata, and generates both SDL schemas and resolver manifests as embedded resources and output files.

## Feature Metadata

**Feature Type**: New Capability
**Estimated Complexity**: High
**Primary Systems Affected**: Lambda.GraphQL (attributes), Lambda.GraphQL.SourceGenerator (core logic)
**Dependencies**: Microsoft.CodeAnalysis.CSharp, existing attribute system

---

## CONTEXT REFERENCES

### Relevant Codebase Files IMPORTANT: YOU MUST READ THESE FILES BEFORE IMPLEMENTING!

- `graphql-hackathon.md` (lines 150-350) - Why: Complete attribute specification and examples
- `Lambda.GraphQL/Attributes/GraphQLTypeAttribute.cs` - Why: Current type attribute implementation to validate
- `Lambda.GraphQL/Attributes/GraphQLFieldAttribute.cs` - Why: Current field attribute implementation to validate  
- `Lambda.GraphQL/Attributes/GraphQLOperationAttributes.cs` - Why: Current operation attributes to validate
- `Lambda.GraphQL/Attributes/GraphQLResolverAttribute.cs` - Why: Current resolver attribute to validate
- `Lambda.GraphQL.SourceGenerator/GraphQLSchemaGenerator.cs` - Why: Current placeholder generator to implement
- `Lambda.GraphQL.SourceGenerator/Models/TypeInfo.cs` - Why: Type info model structure
- `Lambda.GraphQL.SourceGenerator/Models/ResolverInfo.cs` - Why: Resolver info model structure
- `Lambda.GraphQL.Examples/Product.cs` - Why: Example usage pattern to support
- `Lambda.GraphQL.Examples/ProductFunctions.cs` - Why: Example Lambda function pattern to support

### New Files to Create

- `Lambda.GraphQL/Attributes/GraphQLSchemaAttribute.cs` - Assembly-level schema configuration
- `Lambda.GraphQL/Attributes/GraphQLArgumentAttribute.cs` - Method parameter GraphQL arguments
- `Lambda.GraphQL/Attributes/GraphQLIgnoreAttribute.cs` - Exclude properties from schema
- `Lambda.GraphQL/Attributes/GraphQLNonNullAttribute.cs` - Override nullability
- `Lambda.GraphQL/Attributes/GraphQLEnumValueAttribute.cs` - Enum value configuration
- `Lambda.GraphQL.SourceGenerator/TypeMapper.cs` - C# to GraphQL type mapping logic
- `Lambda.GraphQL.SourceGenerator/SdlGenerator.cs` - GraphQL SDL generation logic
- `Lambda.GraphQL.SourceGenerator/ResolverManifestGenerator.cs` - Resolver JSON generation

### Relevant Documentation YOU SHOULD READ THESE BEFORE IMPLEMENTING!

- [Microsoft.CodeAnalysis Documentation](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis)
  - Specific section: IIncrementalGenerator and syntax analysis
  - Why: Required for implementing Roslyn source generators
- [GraphQL SDL Specification](https://spec.graphql.org/October2021/#sec-Type-System)
  - Specific section: Type system and schema definition language
  - Why: Must generate valid GraphQL SDL syntax
- [AWS AppSync Resolver Reference](https://docs.aws.amazon.com/appsync/latest/devguide/resolver-context-reference.html)
  - Specific section: Resolver configuration format
  - Why: Must generate valid resolver manifest JSON

### Patterns to Follow

**Naming Conventions:**
- Attributes: `GraphQL{Purpose}Attribute` (e.g., GraphQLTypeAttribute)
- Enums: `{Purpose}Kind` (e.g., GraphQLTypeKind, ResolverKind)
- Models: `{Purpose}Info` (e.g., TypeInfo, FieldInfo)

**Attribute Pattern:**
```csharp
[AttributeUsage(AttributeTargets.Class)]
public sealed class GraphQLTypeAttribute : Attribute
{
    public GraphQLTypeAttribute(string? name = null) { Name = name; }
    public string? Name { get; }
    public string? Description { get; set; }
}
```

**Source Generator Pattern:**
```csharp
[Generator(LanguageNames.CSharp)]
public partial class GraphQLSchemaGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: (node, _) => IsRelevantNode(node),
            transform: (ctx, _) => ExtractInfo(ctx));
        context.RegisterSourceOutput(provider.Collect(), GenerateOutput);
    }
}
```

---

## IMPLEMENTATION PLAN

### Phase 1: Attribute Validation and Completion

Validate existing attributes against specification and implement missing core attributes.

**Tasks:**
- Audit existing attributes for completeness vs specification
- Implement missing core attributes (Schema, Argument, Ignore, NonNull, EnumValue)
- Add proper XML documentation to all attributes
- Validate attribute usage patterns match specification

### Phase 2: Source Generator Core Implementation

Implement the main source generator logic for syntax analysis and metadata extraction.

**Tasks:**
- Implement syntax node detection (IsGraphQLType, IsGraphQLOperation)
- Create metadata extraction logic (ExtractTypeInfo, ExtractOperationInfo)
- Build type mapping system (C# types to GraphQL types)
- Implement compilation analysis and validation

### Phase 3: SDL Generation Engine

Create the GraphQL Schema Definition Language generation logic.

**Tasks:**
- Implement SDL generation for Object, Input, and Enum types
- Generate Query and Mutation root types from Lambda functions
- Handle field definitions, arguments, and descriptions
- Support deprecation and custom naming

### Phase 4: Resolver Manifest Generation

Generate the resolver configuration JSON for AppSync CDK integration.

**Tasks:**
- Extract resolver configurations from attributes
- Generate resolver manifest JSON structure
- Map Lambda functions to resolver configurations
- Support both unit and pipeline resolvers

---

## STEP-BY-STEP TASKS

IMPORTANT: Execute every task in order, top to bottom. Each task is atomic and independently testable.

### CREATE Lambda.GraphQL/Attributes/GraphQLSchemaAttribute.cs

- **IMPLEMENT**: Assembly-level schema configuration attribute
- **PATTERN**: Follow existing attribute pattern from GraphQLTypeAttribute.cs
- **IMPORTS**: System namespace only
- **GOTCHA**: Must use AttributeTargets.Assembly for assembly-level attributes
- **VALIDATE**: `dotnet build --verbosity minimal`

### CREATE Lambda.GraphQL/Attributes/GraphQLArgumentAttribute.cs

- **IMPLEMENT**: Method parameter GraphQL argument configuration
- **PATTERN**: Mirror GraphQLFieldAttribute.cs structure
- **IMPORTS**: System namespace only
- **GOTCHA**: Use AttributeTargets.Parameter for method parameters
- **VALIDATE**: `dotnet build --verbosity minimal`

### CREATE Lambda.GraphQL/Attributes/GraphQLIgnoreAttribute.cs

- **IMPLEMENT**: Simple marker attribute to exclude properties
- **PATTERN**: Minimal attribute with no properties like a marker
- **IMPORTS**: System namespace only
- **GOTCHA**: Use AttributeTargets.Property | AttributeTargets.Method
- **VALIDATE**: `dotnet build --verbosity minimal`

### CREATE Lambda.GraphQL/Attributes/GraphQLNonNullAttribute.cs

- **IMPLEMENT**: Override nullability for properties
- **PATTERN**: Simple marker attribute
- **IMPORTS**: System namespace only
- **GOTCHA**: Use AttributeTargets.Property for nullability override
- **VALIDATE**: `dotnet build --verbosity minimal`

### CREATE Lambda.GraphQL/Attributes/GraphQLEnumValueAttribute.cs

- **IMPLEMENT**: Enum value configuration with description and deprecation
- **PATTERN**: Similar to GraphQLFieldAttribute.cs with Description and Deprecated properties
- **IMPORTS**: System namespace only
- **GOTCHA**: Use AttributeTargets.Field for enum values
- **VALIDATE**: `dotnet build --verbosity minimal`

### UPDATE Lambda.GraphQL.SourceGenerator/Models/TypeInfo.cs

- **IMPLEMENT**: Add missing properties per specification (IsInterface, IsEnum, EnumValues)
- **PATTERN**: Extend existing TypeInfo class structure
- **IMPORTS**: Add System.Collections.Generic if not present
- **GOTCHA**: Maintain existing property names and types
- **VALIDATE**: `dotnet build --verbosity minimal`

### UPDATE Lambda.GraphQL.SourceGenerator/Models/ResolverInfo.cs

- **IMPLEMENT**: Add missing properties (LambdaFunctionLogicalId, Runtime, RequestMapping, ResponseMapping)
- **PATTERN**: Extend existing ResolverInfo class structure
- **IMPORTS**: System.Collections.Generic already present
- **GOTCHA**: Match property names from graphql-hackathon.md specification
- **VALIDATE**: `dotnet build --verbosity minimal`

### CREATE Lambda.GraphQL.SourceGenerator/TypeMapper.cs

- **IMPLEMENT**: C# to GraphQL type mapping logic per specification table
- **PATTERN**: Static class with mapping methods
- **IMPORTS**: Microsoft.CodeAnalysis, System.Collections.Generic
- **GOTCHA**: Handle nullable reference types vs value types correctly
- **VALIDATE**: `dotnet build --verbosity minimal`

### UPDATE Lambda.GraphQL.SourceGenerator/GraphQLSchemaGenerator.cs - IsGraphQLType method

- **IMPLEMENT**: Detect class declarations with GraphQLType attributes
- **PATTERN**: Check SyntaxNode for ClassDeclarationSyntax with attributes
- **IMPORTS**: Microsoft.CodeAnalysis.CSharp.Syntax
- **GOTCHA**: Must handle both classes and enums, check attribute names
- **VALIDATE**: `dotnet build --verbosity minimal`

### UPDATE Lambda.GraphQL.SourceGenerator/GraphQLSchemaGenerator.cs - IsGraphQLOperation method

- **IMPLEMENT**: Detect method declarations with GraphQLQuery/Mutation attributes
- **PATTERN**: Check SyntaxNode for MethodDeclarationSyntax with Lambda and GraphQL attributes
- **IMPORTS**: Microsoft.CodeAnalysis.CSharp.Syntax
- **GOTCHA**: Must find methods with both LambdaFunction and GraphQL operation attributes
- **VALIDATE**: `dotnet build --verbosity minimal`

### UPDATE Lambda.GraphQL.SourceGenerator/GraphQLSchemaGenerator.cs - ExtractTypeInfo method

- **IMPLEMENT**: Extract TypeInfo from GeneratorSyntaxContext for classes with GraphQLType
- **PATTERN**: Use semantic model to get symbol information and attribute data
- **IMPORTS**: Microsoft.CodeAnalysis, Lambda.GraphQL.SourceGenerator.Models
- **GOTCHA**: Handle compilation errors gracefully, return null for invalid types
- **VALIDATE**: `dotnet build --verbosity minimal`

### UPDATE Lambda.GraphQL.SourceGenerator/GraphQLSchemaGenerator.cs - ExtractOperationInfo method

- **IMPLEMENT**: Extract operation info from Lambda functions with GraphQL attributes
- **PATTERN**: Use semantic model to analyze method symbols and attributes
- **IMPORTS**: Microsoft.CodeAnalysis, Lambda.GraphQL.SourceGenerator.Models
- **GOTCHA**: Must extract both Lambda function name and GraphQL operation details
- **VALIDATE**: `dotnet build --verbosity minimal`

### CREATE Lambda.GraphQL.SourceGenerator/SdlGenerator.cs

- **IMPLEMENT**: Generate GraphQL SDL from TypeInfo and operation collections
- **PATTERN**: Static class with generation methods returning string
- **IMPORTS**: System.Text, System.Linq, Models namespace
- **GOTCHA**: Must generate valid GraphQL SDL syntax, handle descriptions and deprecation
- **VALIDATE**: `dotnet build --verbosity minimal`

### CREATE Lambda.GraphQL.SourceGenerator/ResolverManifestGenerator.cs

- **IMPLEMENT**: Generate resolver manifest JSON from ResolverInfo collection
- **PATTERN**: Static class with JSON generation methods
- **IMPORTS**: System.Text.Json, Models namespace
- **GOTCHA**: Must match exact JSON schema from graphql-hackathon.md specification
- **VALIDATE**: `dotnet build --verbosity minimal`

### UPDATE Lambda.GraphQL.SourceGenerator/GraphQLSchemaGenerator.cs - GenerateSchema method

- **IMPLEMENT**: Orchestrate SDL and resolver manifest generation, emit as source files
- **PATTERN**: Use SourceProductionContext.AddSource to emit generated files
- **IMPORTS**: System.Text, SdlGenerator, ResolverManifestGenerator
- **GOTCHA**: Must emit both C# source with embedded resources and hint names for files
- **VALIDATE**: `dotnet build --verbosity minimal`

### UPDATE Lambda.GraphQL.Examples project to test generation

- **IMPLEMENT**: Add assembly-level GraphQLSchema attribute to test schema generation
- **PATTERN**: Add [assembly: GraphQLSchema("ExampleAPI")] to AssemblyInfo.cs
- **IMPORTS**: Lambda.GraphQL.Attributes
- **GOTCHA**: Must be in AssemblyInfo.cs or global using file
- **VALIDATE**: `dotnet build Lambda.GraphQL.Examples --verbosity minimal`

### ADD comprehensive tests for new functionality

- **IMPLEMENT**: Tests for all new attributes and source generator functionality
- **PATTERN**: Follow existing test pattern in AttributeTests.cs
- **IMPORTS**: FluentAssertions, Lambda.GraphQL.Attributes
- **GOTCHA**: Test both positive and negative cases, attribute validation
- **VALIDATE**: `dotnet test --verbosity minimal`

---

## TESTING STRATEGY

### Unit Tests

Design unit tests with fixtures and assertions following existing xUnit + FluentAssertions pattern:

- Attribute construction and property validation
- Type mapping logic correctness
- SDL generation syntax validation
- Resolver manifest JSON schema compliance
- Source generator syntax node detection

### Integration Tests

- End-to-end source generation from example project
- Generated SDL parsing validation
- Resolver manifest deserialization
- Multiple type and operation scenarios

### Edge Cases

- Nullable reference types vs value types
- Generic types and collections
- Inheritance and interface implementations
- Missing or invalid attribute configurations
- Compilation errors and malformed syntax

---

## VALIDATION COMMANDS

Execute every command to ensure zero regressions and 100% feature correctness.

### Level 1: Syntax & Style

```bash
dotnet build --verbosity minimal
```

### Level 2: Unit Tests

```bash
dotnet test --verbosity minimal
```

### Level 3: Integration Tests

```bash
dotnet build Lambda.GraphQL.Examples --verbosity minimal
```

### Level 4: Manual Validation

```bash
# Verify source generator produces output
dotnet build Lambda.GraphQL.Examples -v detailed | grep "GraphQL"

# Check for generated files in obj directory
find Lambda.GraphQL.Examples/obj -name "*.g.cs" -o -name "*.graphql" -o -name "*.json"

# Validate NuGet package creation
dotnet pack Lambda.GraphQL/Lambda.GraphQL.csproj -c Release --verbosity minimal
```

### Level 5: Additional Validation

```bash
# Verify no compilation warnings
dotnet build --verbosity normal --warnaserror

# Check generated SDL syntax (if GraphQL CLI available)
# graphql-schema-linter generated-schema.graphql
```

---

## ACCEPTANCE CRITERIA

- [ ] All core attributes implemented per graphql-hackathon.md specification
- [ ] Source generator detects GraphQL-attributed classes and methods
- [ ] Valid GraphQL SDL generated from C# types and Lambda functions
- [ ] Resolver manifest JSON generated with correct schema
- [ ] Type mapping handles all specified C# to GraphQL type conversions
- [ ] Nullability handling follows specification (C# nullable → GraphQL nullable)
- [ ] Generated code compiles without errors or warnings
- [ ] All validation commands pass with zero errors
- [ ] Unit test coverage for all new functionality (80%+)
- [ ] Integration tests verify end-to-end schema generation
- [ ] Example project demonstrates working functionality
- [ ] No regressions in existing functionality

---

## COMPLETION CHECKLIST

- [ ] All tasks completed in order
- [ ] Each task validation passed immediately
- [ ] All validation commands executed successfully
- [ ] Full test suite passes (unit + integration)
- [ ] No linting or type checking errors
- [ ] Manual testing confirms schema generation works
- [ ] Acceptance criteria all met
- [ ] Code reviewed for quality and maintainability

---

## NOTES

**Type Mapping Priority**: Focus on basic types first (string, int, bool, decimal) before complex types (generics, collections).

**SDL Generation**: Generate minimal valid SDL initially, then add descriptions, deprecation, and advanced features.

**Error Handling**: Source generator must handle compilation errors gracefully and not crash the build process.

**Performance**: Use incremental generation patterns to avoid regenerating unchanged schemas.

**Debugging**: Add diagnostic output to help troubleshoot source generator issues during development.
