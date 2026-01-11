# Feature: Phase 3 Advanced GraphQL Features

The following plan should be complete, but its important that you validate documentation and codebase patterns and task sanity before you start implementing.

Pay special attention to naming of existing utils types and models. Import from the right files etc.

## Feature Description

Implement Phase 3 advanced GraphQL features to provide full GraphQL specification support for the Lambda.GraphQL library. This includes union types, enhanced interface support, AWS custom scalars, custom directives, subscriptions, field-level auth directives, and Relay connection pattern helpers.

## User Story

As a .NET developer using AWS AppSync
I want to use advanced GraphQL features like unions, custom scalars, and directives
So that I can build sophisticated GraphQL APIs with full type safety and AWS integration

## Problem Statement

The current Lambda.GraphQL implementation only supports basic GraphQL features (objects, inputs, enums, basic interfaces). To be production-ready and competitive, it needs to support advanced GraphQL features that are commonly used in real-world applications, particularly AWS-specific features for AppSync integration.

## Solution Statement

Extend the existing source generator and type system to support:
1. Union types for polymorphic return values
2. Enhanced interface support with implementation tracking
3. AWS custom scalar types (AWSDateTime, AWSDate, etc.)
4. Custom directive definitions and applications
5. Subscription operation support
6. Field-level authorization directives
7. Relay connection pattern helpers for pagination

## Feature Metadata

**Feature Type**: Enhancement
**Estimated Complexity**: High
**Primary Systems Affected**: Source Generator, Type System, SDL Generation, Attributes
**Dependencies**: Microsoft.CodeAnalysis.CSharp, existing Lambda.GraphQL packages

---

## CONTEXT REFERENCES

### Relevant Codebase Files IMPORTANT: YOU MUST READ THESE FILES BEFORE IMPLEMENTING!

- `Lambda.GraphQL/Attributes/GraphQLTypeAttribute.cs` - Why: Contains existing type kind enum, need to extend for Union
- `Lambda.GraphQL.SourceGenerator/Models/TypeInfo.cs` - Why: Core type model that needs union support and interface implementations
- `Lambda.GraphQL.SourceGenerator/GraphQLSchemaGenerator.cs` (lines 15-45) - Why: Main generator logic for type extraction
- `Lambda.GraphQL.SourceGenerator/SdlGenerator.cs` (lines 86-120) - Why: Contains interface generation pattern to mirror for unions
- `Lambda.GraphQL.Tests/SdlGeneratorTests.cs` (lines 187-220) - Why: Interface test pattern to follow for new features
- `Lambda.GraphQL/Attributes/GraphQLOperationAttributes.cs` - Why: Operation attributes pattern for subscription support
- `Lambda.GraphQL/Attributes/GraphQLResolverAttribute.cs` - Why: Resolver configuration pattern for auth directives

### New Files to Create

- `Lambda.GraphQL/Attributes/GraphQLUnionAttribute.cs` - Union type definition attribute
- `Lambda.GraphQL/Attributes/GraphQLScalarAttribute.cs` - Custom scalar type attribute
- `Lambda.GraphQL/Attributes/GraphQLDirectiveAttribute.cs` - Custom directive definition
- `Lambda.GraphQL/Attributes/GraphQLApplyDirectiveAttribute.cs` - Apply directive to fields/types
- `Lambda.GraphQL/Attributes/GraphQLSubscriptionAttribute.cs` - Subscription operation marker
- `Lambda.GraphQL/Attributes/GraphQLAuthDirectiveAttribute.cs` - AWS auth directive
- `Lambda.GraphQL/Attributes/GraphQLConnectionAttribute.cs` - Relay connection helper
- `Lambda.GraphQL.SourceGenerator/Models/UnionInfo.cs` - Union type model
- `Lambda.GraphQL.SourceGenerator/Models/DirectiveInfo.cs` - Directive model
- `Lambda.GraphQL.SourceGenerator/Models/ScalarInfo.cs` - Custom scalar model
- `Lambda.GraphQL.SourceGenerator/AwsScalarMapper.cs` - AWS scalar type mapping
- `Lambda.GraphQL.Tests/UnionTypeTests.cs` - Union type generation tests
- `Lambda.GraphQL.Tests/DirectiveTests.cs` - Directive generation tests
- `Lambda.GraphQL.Tests/AwsScalarTests.cs` - AWS scalar mapping tests

### Relevant Documentation YOU SHOULD READ THESE BEFORE IMPLEMENTING!

- [GraphQL Union Types Specification](https://spec.graphql.org/October2021/#sec-Unions)
  - Specific section: Union type definition and resolution
  - Why: Required for implementing union type syntax and semantics
- [AWS AppSync Scalar Types](https://docs.aws.amazon.com/appsync/latest/devguide/scalars.html)
  - Specific section: AWS scalar definitions (AWSDateTime, AWSDate, etc.)
  - Why: Required for mapping C# types to AWS scalars
- [GraphQL Directive Specification](https://spec.graphql.org/October2021/#sec-Type-System.Directives)
  - Specific section: Directive definitions and applications
  - Why: Required for custom directive implementation

### Patterns to Follow

**Attribute Pattern:**
```csharp
[AttributeUsage(AttributeTargets.Class)]
public sealed class GraphQLTypeAttribute : Attribute
{
    public GraphQLTypeAttribute(string? name = null) { Name = name; }
    public string? Name { get; }
    public GraphQLTypeKind Kind { get; set; } = GraphQLTypeKind.Object;
}
```

**Type Info Model Pattern:**
```csharp
public sealed class TypeInfo
{
    public string Name { get; set; } = string.Empty;
    public TypeKind Kind { get; set; }
    public List<FieldInfo> Fields { get; set; } = new();
}
```

**SDL Generation Pattern:**
```csharp
case Models.TypeKind.Interface:
    GenerateInterfaceType(sb, type);
    break;

private static void GenerateInterfaceType(StringBuilder sb, Models.TypeInfo type)
{
    sb.AppendLine($"interface {type.Name} {{");
    // Generate fields
    sb.AppendLine("}");
}
```

**Test Pattern:**
```csharp
[Fact]
public void GenerateSchema_ShouldRenderInterfaceTypes()
{
    var types = new List<TypeInfo> { /* test data */ };
    var sdl = SdlGenerator.GenerateSchema(types, new List<ResolverInfo>());
    sdl.Should().Contain("interface Node {");
}
```

---

## IMPLEMENTATION PLAN

### Phase 1: Foundation - Union Types and Enhanced Interfaces

**Tasks:**
- Extend GraphQLTypeKind enum to include Union
- Create GraphQLUnionAttribute for union type definitions
- Add UnionInfo model for union type metadata
- Extend TypeInfo to support union member types and interface implementations
- Update source generator to detect and extract union types

### Phase 2: AWS Custom Scalars

**Tasks:**
- Create AwsScalarMapper for C# to AWS scalar type mapping
- Add GraphQLScalarAttribute for custom scalar definitions
- Implement automatic mapping for DateTime → AWSDateTime, Guid → ID, etc.
- Update SDL generation to include AWS scalar types
- Add validation for AWS scalar constraints

### Phase 3: Directive System

**Tasks:**
- Create GraphQLDirectiveAttribute for directive definitions
- Add GraphQLApplyDirectiveAttribute for applying directives
- Implement DirectiveInfo model for directive metadata
- Add directive generation to SDL output
- Create AWS auth directive attributes (@aws_auth, @aws_cognito_user_pools)

### Phase 4: Subscriptions and Advanced Features

**Tasks:**
- Add GraphQLSubscriptionAttribute for subscription operations
- Extend resolver tracking for subscription resolvers
- Implement Relay connection pattern helpers
- Add field-level authorization directive support
- Update resolver manifest generation for new features

---

## STEP-BY-STEP TASKS

IMPORTANT: Execute every task in order, top to bottom. Each task is atomic and independently testable.

### CREATE Lambda.GraphQL/Attributes/GraphQLUnionAttribute.cs

- **IMPLEMENT**: Union type attribute with member type specification
- **PATTERN**: Mirror GraphQLTypeAttribute structure from GraphQLTypeAttribute.cs:8-20
- **IMPORTS**: System, AttributeUsage
- **GOTCHA**: Union members must be object types, not scalars or interfaces
- **VALIDATE**: `dotnet build Lambda.GraphQL --verbosity minimal`

### UPDATE Lambda.GraphQL/Attributes/GraphQLTypeAttribute.cs

- **IMPLEMENT**: Add Union to GraphQLTypeKind enum
- **PATTERN**: Follow existing enum pattern at line 28
- **IMPORTS**: None needed
- **GOTCHA**: Maintain enum order for backward compatibility
- **VALIDATE**: `dotnet build Lambda.GraphQL --verbosity minimal`

### CREATE Lambda.GraphQL.SourceGenerator/Models/UnionInfo.cs

- **IMPLEMENT**: Union type model with member types list
- **PATTERN**: Mirror TypeInfo structure from TypeInfo.cs:8-20
- **IMPORTS**: System.Collections.Generic
- **GOTCHA**: Union members are type names, not TypeInfo references
- **VALIDATE**: `dotnet build Lambda.GraphQL.SourceGenerator --verbosity minimal`

### UPDATE Lambda.GraphQL.SourceGenerator/Models/TypeInfo.cs

- **IMPLEMENT**: Add UnionMembers property and InterfaceImplementations list
- **PATTERN**: Follow existing property pattern at lines 10-15
- **IMPORTS**: None needed
- **GOTCHA**: Initialize collections to avoid null reference exceptions
- **VALIDATE**: `dotnet build Lambda.GraphQL.SourceGenerator --verbosity minimal`

### UPDATE Lambda.GraphQL.SourceGenerator/GraphQLSchemaGenerator.cs

- **IMPLEMENT**: Union type detection in IsGraphQLType method
- **PATTERN**: Mirror interface detection logic at line 84
- **IMPORTS**: None needed
- **GOTCHA**: Check for GraphQLUnionAttribute specifically
- **VALIDATE**: `dotnet build Lambda.GraphQL.SourceGenerator --verbosity minimal`

### UPDATE Lambda.GraphQL.SourceGenerator/GraphQLSchemaGenerator.cs

- **IMPLEMENT**: Union type extraction in ExtractTypeInfoWithDiagnostics
- **PATTERN**: Follow interface extraction pattern at lines 94-96
- **IMPORTS**: None needed
- **GOTCHA**: Extract union member types from attribute parameters
- **VALIDATE**: `dotnet build Lambda.GraphQL.SourceGenerator --verbosity minimal`

### UPDATE Lambda.GraphQL.SourceGenerator/SdlGenerator.cs

- **IMPLEMENT**: Union type SDL generation
- **PATTERN**: Mirror GenerateInterfaceType method at lines 115-120
- **IMPORTS**: None needed
- **GOTCHA**: Union syntax is "union Name = Type1 | Type2 | Type3"
- **VALIDATE**: `dotnet build Lambda.GraphQL.SourceGenerator --verbosity minimal`

### CREATE Lambda.GraphQL/Attributes/GraphQLScalarAttribute.cs

- **IMPLEMENT**: Custom scalar type attribute
- **PATTERN**: Mirror GraphQLTypeAttribute structure
- **IMPORTS**: System, AttributeUsage
- **GOTCHA**: Scalars are leaf types, no fields allowed
- **VALIDATE**: `dotnet build Lambda.GraphQL --verbosity minimal`

### CREATE Lambda.GraphQL.SourceGenerator/AwsScalarMapper.cs

- **IMPLEMENT**: Static mapping methods for C# types to AWS scalars
- **PATTERN**: Create static class with mapping methods
- **IMPORTS**: System
- **GOTCHA**: DateTime → AWSDateTime, DateOnly → AWSDate, TimeOnly → AWSTime, Guid → ID
- **VALIDATE**: `dotnet build Lambda.GraphQL.SourceGenerator --verbosity minimal`

### UPDATE Lambda.GraphQL.SourceGenerator/GraphQLSchemaGenerator.cs

- **IMPLEMENT**: AWS scalar type mapping in type resolution
- **PATTERN**: Add scalar mapping before existing type resolution
- **IMPORTS**: None needed
- **GOTCHA**: Check AWS scalar mapping before default GraphQL types
- **VALIDATE**: `dotnet build Lambda.GraphQL.SourceGenerator --verbosity minimal`

### CREATE Lambda.GraphQL/Attributes/GraphQLDirectiveAttribute.cs

- **IMPLEMENT**: Directive definition attribute
- **PATTERN**: Mirror GraphQLTypeAttribute with locations and arguments
- **IMPORTS**: System, AttributeUsage
- **GOTCHA**: Directives have locations (FIELD_DEFINITION, OBJECT, etc.)
- **VALIDATE**: `dotnet build Lambda.GraphQL --verbosity minimal`

### CREATE Lambda.GraphQL/Attributes/GraphQLApplyDirectiveAttribute.cs

- **IMPLEMENT**: Apply directive to types/fields attribute
- **PATTERN**: Simple attribute with directive name and arguments
- **IMPORTS**: System, AttributeUsage
- **GOTCHA**: Can be applied multiple times to same target
- **VALIDATE**: `dotnet build Lambda.GraphQL --verbosity minimal`

### CREATE Lambda.GraphQL.SourceGenerator/Models/DirectiveInfo.cs

- **IMPLEMENT**: Directive metadata model
- **PATTERN**: Mirror TypeInfo structure with locations and arguments
- **IMPORTS**: System.Collections.Generic
- **GOTCHA**: Arguments are name-type pairs, locations are enum flags
- **VALIDATE**: `dotnet build Lambda.GraphQL.SourceGenerator --verbosity minimal`

### CREATE Lambda.GraphQL/Attributes/GraphQLSubscriptionAttribute.cs

- **IMPLEMENT**: Subscription operation attribute
- **PATTERN**: Mirror GraphQLQueryAttribute from GraphQLOperationAttributes.cs
- **IMPORTS**: System, AttributeUsage
- **GOTCHA**: Subscriptions are long-running operations
- **VALIDATE**: `dotnet build Lambda.GraphQL --verbosity minimal`

### CREATE Lambda.GraphQL/Attributes/GraphQLAuthDirectiveAttribute.cs

- **IMPLEMENT**: AWS auth directive attribute (@aws_auth, @aws_cognito_user_pools)
- **PATTERN**: Simple attribute with auth mode enum
- **IMPORTS**: System, AttributeUsage
- **GOTCHA**: Multiple auth modes can be applied to same field
- **VALIDATE**: `dotnet build Lambda.GraphQL --verbosity minimal`

### CREATE Lambda.GraphQL.Tests/UnionTypeTests.cs

- **IMPLEMENT**: Unit tests for union type generation
- **PATTERN**: Mirror SdlGeneratorTests.cs interface test pattern at lines 187-220
- **IMPORTS**: Xunit, FluentAssertions, test models
- **GOTCHA**: Test both union definition and member type resolution
- **VALIDATE**: `dotnet test --filter "UnionTypeTests" --verbosity minimal`

### CREATE Lambda.GraphQL.Tests/AwsScalarTests.cs

- **IMPLEMENT**: Unit tests for AWS scalar type mapping
- **PATTERN**: Follow existing test patterns in SdlGeneratorTests.cs
- **IMPORTS**: Xunit, FluentAssertions, System
- **GOTCHA**: Test DateTime → AWSDateTime, Guid → ID mappings
- **VALIDATE**: `dotnet test --filter "AwsScalarTests" --verbosity minimal`

### CREATE Lambda.GraphQL.Tests/DirectiveTests.cs

- **IMPLEMENT**: Unit tests for directive generation
- **PATTERN**: Follow existing test patterns
- **IMPORTS**: Xunit, FluentAssertions
- **GOTCHA**: Test both directive definitions and applications
- **VALIDATE**: `dotnet test --filter "DirectiveTests" --verbosity minimal`

### UPDATE Lambda.GraphQL.SourceGenerator/ResolverManifestGenerator.cs

- **IMPLEMENT**: Subscription resolver support in manifest generation
- **PATTERN**: Mirror existing resolver generation logic
- **IMPORTS**: None needed
- **GOTCHA**: Subscriptions have different resolver configuration than queries/mutations
- **VALIDATE**: `dotnet build Lambda.GraphQL.SourceGenerator --verbosity minimal`

### CREATE Lambda.GraphQL.Examples/AdvancedTypes.cs

- **IMPLEMENT**: Example classes demonstrating union types, AWS scalars, directives
- **PATTERN**: Mirror existing Product.cs structure
- **IMPORTS**: Lambda.GraphQL.Attributes, System
- **GOTCHA**: Provide realistic examples for documentation
- **VALIDATE**: `dotnet build Lambda.GraphQL.Examples --verbosity minimal`

---

## TESTING STRATEGY

### Unit Tests

Design comprehensive unit tests covering:
- Union type SDL generation with multiple member types
- AWS scalar type mapping for all supported C# types
- Directive definition and application in SDL output
- Subscription operation detection and resolver generation
- Interface implementation tracking and inheritance

### Integration Tests

Test end-to-end schema generation with:
- Complex schemas using unions, interfaces, and custom scalars
- AWS auth directive application on fields and types
- Subscription resolver manifest generation
- Relay connection pattern helpers

### Edge Cases

- Union types with no members (should generate error)
- Circular union references (should be detected and handled)
- Invalid AWS scalar mappings (unsupported C# types)
- Directive applications on invalid targets
- Subscription resolvers without proper configuration

---

## VALIDATION COMMANDS

Execute every command to ensure zero regressions and 100% feature correctness.

### Level 1: Syntax & Style

```bash
dotnet build --verbosity minimal
dotnet format --verify-no-changes
```

### Level 2: Unit Tests

```bash
dotnet test --verbosity minimal
dotnet test --filter "UnionTypeTests" --verbosity minimal
dotnet test --filter "AwsScalarTests" --verbosity minimal
dotnet test --filter "DirectiveTests" --verbosity minimal
```

### Level 3: Integration Tests

```bash
dotnet build Lambda.GraphQL.Examples --verbosity minimal
dotnet test Lambda.GraphQL.Tests --verbosity minimal
```

### Level 4: Manual Validation

```bash
# Check generated schema includes union types
cat Lambda.GraphQL.Examples/schema.graphql | grep -E "union|interface"

# Verify AWS scalar types in schema
cat Lambda.GraphQL.Examples/schema.graphql | grep -E "AWSDateTime|AWSDate|ID"

# Check resolver manifest includes subscriptions
cat Lambda.GraphQL.Examples/resolvers.json | grep -E "subscription|SUBSCRIPTION"
```

### Level 5: Additional Validation

```bash
# Validate GraphQL schema syntax
npx graphql-schema-linter Lambda.GraphQL.Examples/schema.graphql || echo "GraphQL linter not available"
```

---

## ACCEPTANCE CRITERIA

- [ ] Union types can be defined with GraphQLUnionAttribute and generate correct SDL
- [ ] AWS scalar types (AWSDateTime, AWSDate, AWSTime, ID, AWSJSON) are automatically mapped from C# types
- [ ] Custom directives can be defined and applied to types and fields
- [ ] AWS auth directives (@aws_auth, @aws_cognito_user_pools) are supported
- [ ] Subscription operations are detected and included in resolver manifest
- [ ] Interface types track their implementations for proper SDL generation
- [ ] All existing functionality remains unchanged (no regressions)
- [ ] Generated SDL is valid GraphQL syntax
- [ ] Resolver manifest includes all new resolver types
- [ ] Unit test coverage is maintained above 80%
- [ ] Integration tests verify end-to-end functionality
- [ ] Examples demonstrate all new features

---

## COMPLETION CHECKLIST

- [ ] All tasks completed in order
- [ ] Each task validation passed immediately
- [ ] All validation commands executed successfully
- [ ] Full test suite passes (unit + integration)
- [ ] No linting or type checking errors
- [ ] Manual testing confirms features work
- [ ] Acceptance criteria all met
- [ ] Code reviewed for quality and maintainability
- [ ] Examples updated to demonstrate new features
- [ ] Documentation reflects new capabilities

---

## NOTES

**Design Decisions:**
- Union types use attribute-based member specification rather than inheritance to maintain C# type safety
- AWS scalar mapping is automatic based on C# type analysis, with override capability via attributes
- Directive system supports both definition and application to maintain GraphQL specification compliance
- Subscription support focuses on resolver configuration rather than runtime subscription handling

**Trade-offs:**
- Union member types must be specified as strings in attributes (compile-time safe but not refactor-safe)
- AWS scalar mapping is opinionated but can be overridden with explicit scalar attributes
- Directive arguments are string-based for simplicity but lose type safety

**Performance Considerations:**
- Union type resolution adds minimal overhead to source generation
- AWS scalar mapping uses static lookups for performance
- Directive processing is deferred to SDL generation phase to avoid compilation impact

**Security Considerations:**
- AWS auth directives are generated but not enforced at compile time
- Directive arguments should be validated for injection attacks in production use
- Subscription resolvers require proper AWS IAM configuration for security
