# Feature: Implement GraphQL Hackathon Phase 2 - Resolver Tracking

The following plan implements Phase 2 of the GraphQL hackathon project, focusing on resolver tracking and manifest generation while fixing the schema generation pipeline from Phase 1.

## Feature Description

Implement comprehensive resolver tracking for AppSync GraphQL APIs, generating both GraphQL SDL schemas and resolver configuration manifests. This includes fixing the existing MSBuild task integration, enhancing resolver attribute support, and creating a complete CDK-compatible resolver manifest system.

## User Story

As a developer building GraphQL APIs with AWS Lambda and AppSync
I want the library to automatically track resolver configurations and generate deployment manifests
So that I can seamlessly deploy my GraphQL API with proper resolver mappings using CDK

## Problem Statement

Phase 1 implemented basic schema generation but lacks:
1. **Missing Schema Output**: The MSBuild task isn't properly extracting and writing schema files
2. **Incomplete Resolver Tracking**: GraphQLResolverAttribute exists but isn't fully processed
3. **No Resolver Manifest**: Missing resolvers.json generation for CDK deployment
4. **Limited Resolver Support**: No pipeline resolver or data source tracking
5. **Integration Issues**: MSBuild task path resolution problems

## Solution Statement

Complete the resolver tracking system by implementing proper GraphQLResolverAttribute processing, generating comprehensive resolver manifests, fixing MSBuild task integration, and creating CDK-compatible output files that enable seamless AppSync API deployment.

## Feature Metadata

**Feature Type**: Enhancement/Bug Fix
**Estimated Complexity**: Medium
**Primary Systems Affected**: Source Generator, MSBuild Task, Resolver Manifest Generation
**Dependencies**: Microsoft.CodeAnalysis, Microsoft.Build.Framework, System.Text.Json

---

## CONTEXT REFERENCES

### Relevant Codebase Files IMPORTANT: YOU MUST READ THESE FILES BEFORE IMPLEMENTING!

- `graphql-hackathon.md` (lines 200-350) - Why: Contains Phase 2 requirements and resolver manifest specification
- `Lambda.GraphQL.SourceGenerator/GraphQLSchemaGenerator.cs` (lines 250-350) - Why: Contains existing resolver extraction logic that needs enhancement
- `Lambda.GraphQL.SourceGenerator/Models/ResolverInfo.cs` - Why: Current resolver model that needs expansion
- `Lambda.GraphQL.SourceGenerator/ResolverManifestGenerator.cs` - Why: Existing manifest generator that needs completion
- `Lambda.GraphQL.Build/ExtractGraphQLSchemaTask.cs` - Why: MSBuild task that needs fixing for proper file output
- `Lambda.GraphQL/build/Lambda.GraphQL.targets` - Why: MSBuild integration that needs path resolution fixes
- `Lambda.GraphQL/Attributes/GraphQLResolverAttribute.cs` - Why: Resolver attribute definition and properties
- `Lambda.GraphQL.Examples/ProductFunctions.cs` - Why: Example usage patterns to validate against

### New Files to Create

- `Lambda.GraphQL.SourceGenerator/Models/DataSourceInfo.cs` - Data source configuration model
- `Lambda.GraphQL.SourceGenerator/Models/ResolverManifest.cs` - Complete manifest model
- `Lambda.GraphQL.Tests/ResolverTrackingTests.cs` - Tests for resolver tracking functionality
- `Lambda.GraphQL.Tests/ManifestGenerationTests.cs` - Tests for manifest generation

### Relevant Documentation YOU SHOULD READ THESE BEFORE IMPLEMENTING!

- [AWS AppSync Resolver Reference](https://docs.aws.amazon.com/appsync/latest/devguide/resolver-context-reference.html)
  - Specific section: Unit and Pipeline resolver configuration
  - Why: Required for proper resolver manifest structure
- [MSBuild Task Documentation](https://docs.microsoft.com/en-us/visualstudio/msbuild/task-writing)
  - Specific section: Custom task implementation and file output
  - Why: Needed for fixing MSBuild task integration
- [Roslyn Source Generator Cookbook](https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md)
  - Specific section: Generating additional files
  - Why: Required for proper file generation patterns

### Patterns to Follow

**Resolver Attribute Processing Pattern:**
```csharp
// Extract resolver configuration from attributes
var resolverAttr = methodSymbol.GetAttributes()
    .FirstOrDefault(attr => attr.AttributeClass?.Name == "GraphQLResolverAttribute");

var resolverInfo = new ResolverInfo
{
    Kind = GetResolverKind(resolverAttr),
    DataSource = GetAttributePropertyValue(resolverAttr, "DataSource"),
    Functions = GetPipelineFunctions(resolverAttr)
};
```

**MSBuild Task File Output Pattern:**
```csharp
// Write files to output directory
var schemaPath = Path.Combine(OutputDirectory, SchemaFileName);
File.WriteAllText(schemaPath, sdl);
Log.LogMessage(MessageImportance.High, $"Generated schema: {schemaPath}");
```

**Manifest Generation Pattern:**
```csharp
// Generate CDK-compatible resolver manifest
var manifest = new ResolverManifest
{
    Version = "1.0.0",
    GeneratedAt = DateTime.UtcNow,
    Resolvers = resolvers,
    DataSources = dataSources
};
```

---

## IMPLEMENTATION PLAN

### Phase 1: Foundation - Fix MSBuild Integration

**Tasks:**
- Fix MSBuild task path resolution in Lambda.GraphQL.targets
- Ensure proper assembly loading and schema extraction
- Validate file output to correct directories

### Phase 2: Core Implementation - Resolver Tracking

**Tasks:**
- Enhance GraphQLResolverAttribute processing in source generator
- Implement data source tracking and deduplication
- Create comprehensive resolver manifest generation
- Add pipeline resolver support

### Phase 3: Integration & Output

**Tasks:**
- Integrate resolver manifest generation with MSBuild task
- Ensure both schema.graphql and resolvers.json are generated
- Add proper error handling and diagnostics
- Validate CDK compatibility

### Phase 4: Testing & Validation

**Tasks:**
- Create comprehensive test suite for resolver tracking
- Add integration tests for manifest generation
- Validate against example project
- Ensure backward compatibility

---

## STEP-BY-STEP TASKS

IMPORTANT: Execute every task in order, top to bottom. Each task is atomic and independently testable.

### UPDATE Lambda.GraphQL/build/Lambda.GraphQL.targets

- **IMPLEMENT**: Fix MSBuild task assembly path resolution to work in development and packaged scenarios
- **PATTERN**: Use MSBuildThisFileDirectory with proper relative paths for both dev and NuGet scenarios
- **IMPORTS**: No new imports needed
- **GOTCHA**: Must work both in development (project references) and NuGet package scenarios
- **VALIDATE**: `dotnet build Lambda.GraphQL.Examples --verbosity normal`

### UPDATE Lambda.GraphQL.Build/ExtractGraphQLSchemaTask.cs

- **IMPLEMENT**: Ensure proper file output for both schema.graphql and resolvers.json
- **PATTERN**: Use File.WriteAllText with proper path construction and error handling
- **IMPORTS**: Add System.IO for file operations
- **GOTCHA**: Handle cases where output directory doesn't exist
- **VALIDATE**: `dotnet build Lambda.GraphQL.Examples && ls Lambda.GraphQL.Examples/bin/Debug/net6.0/*.graphql`

### CREATE Lambda.GraphQL.SourceGenerator/Models/DataSourceInfo.cs

- **IMPLEMENT**: Model for AppSync data source configuration
- **PATTERN**: Follow existing model patterns in ResolverInfo.cs with proper properties
- **IMPORTS**: No imports needed for simple model
- **GOTCHA**: Must support both Lambda and other data source types for future extensibility
- **VALIDATE**: `dotnet build --verbosity minimal`

### CREATE Lambda.GraphQL.SourceGenerator/Models/ResolverManifest.cs

- **IMPLEMENT**: Complete manifest model matching hackathon specification
- **PATTERN**: Include version, timestamp, resolvers array, and data sources array
- **IMPORTS**: System.Text.Json attributes for serialization
- **GOTCHA**: Must match exact JSON schema expected by CDK integration
- **VALIDATE**: `dotnet build --verbosity minimal`

### UPDATE Lambda.GraphQL.SourceGenerator/GraphQLSchemaGenerator.cs

- **IMPLEMENT**: Enhanced resolver attribute processing with data source extraction
- **PATTERN**: Extract DataSource, Kind, Functions properties from GraphQLResolverAttribute
- **IMPORTS**: No new imports needed
- **GOTCHA**: Handle both unit and pipeline resolvers, default to unit if not specified
- **VALIDATE**: `dotnet build --verbosity minimal && dotnet test`

### UPDATE Lambda.GraphQL.SourceGenerator/ResolverManifestGenerator.cs

- **IMPLEMENT**: Complete manifest generation with data source deduplication
- **PATTERN**: Group resolvers by data source, generate unique data source entries
- **IMPORTS**: System.Text.Json for serialization
- **GOTCHA**: Ensure data source names are unique and properly referenced
- **VALIDATE**: `dotnet build --verbosity minimal`

### UPDATE Lambda.GraphQL.SourceGenerator/GraphQLSchemaGenerator.cs

- **IMPLEMENT**: Integrate resolver manifest generation into main schema generation pipeline
- **PATTERN**: Generate both SDL and resolver manifest in GenerateSchema method
- **IMPORTS**: No new imports needed
- **GOTCHA**: Both outputs must be available to MSBuild task via assembly metadata
- **VALIDATE**: `dotnet build Lambda.GraphQL.Examples --verbosity normal`

### CREATE Lambda.GraphQL.Tests/ResolverTrackingTests.cs

- **IMPLEMENT**: Comprehensive tests for resolver attribute processing
- **PATTERN**: Mirror existing test patterns from ReturnTypeExtractionTests.cs
- **IMPORTS**: xUnit, FluentAssertions, Microsoft.CodeAnalysis
- **GOTCHA**: Test both unit and pipeline resolver scenarios
- **VALIDATE**: `dotnet test --verbosity minimal`

### CREATE Lambda.GraphQL.Tests/ManifestGenerationTests.cs

- **IMPLEMENT**: Tests for resolver manifest generation and JSON serialization
- **PATTERN**: Create test resolvers and validate JSON output structure
- **IMPORTS**: xUnit, FluentAssertions, System.Text.Json
- **GOTCHA**: Validate against exact JSON schema expected by CDK
- **VALIDATE**: `dotnet test --verbosity minimal`

### UPDATE Lambda.GraphQL.Examples/ProductFunctions.cs

- **IMPLEMENT**: Add pipeline resolver example to demonstrate Phase 2 functionality
- **PATTERN**: Add method with GraphQLResolver(Kind = ResolverKind.Pipeline, Functions = [...])
- **IMPORTS**: No new imports needed
- **GOTCHA**: Ensure example demonstrates both unit and pipeline patterns
- **VALIDATE**: `dotnet build Lambda.GraphQL.Examples && cat Lambda.GraphQL.Examples/bin/Debug/net6.0/resolvers.json`

---

## TESTING STRATEGY

### Unit Tests

Design unit tests with fixtures and assertions following existing xUnit + FluentAssertions pattern:
- **Resolver Attribute Processing**: Verify correct extraction of DataSource, Kind, Functions
- **Data Source Deduplication**: Test that duplicate data sources are properly merged
- **Manifest Generation**: Validate JSON structure matches CDK expectations
- **Pipeline Resolver Support**: Test pipeline resolver configuration parsing

### Integration Tests

- **End-to-End Schema Generation**: Build example project and verify both files are generated
- **MSBuild Task Integration**: Validate task runs and produces correct output files
- **CDK Compatibility**: Ensure generated manifest can be consumed by CDK constructs

### Edge Cases

- Missing resolver attributes (should default to unit resolver)
- Invalid pipeline function configurations
- Duplicate data source names with different configurations
- Large numbers of resolvers and data sources

---

## VALIDATION COMMANDS

Execute every command to ensure zero regressions and 100% feature correctness.

### Level 1: Syntax & Style

```bash
dotnet build --verbosity minimal --warnaserror
```

### Level 2: Unit Tests

```bash
dotnet test --verbosity minimal
```

### Level 3: Integration Tests

```bash
dotnet build Lambda.GraphQL.Examples --verbosity normal
```

### Level 4: Manual Validation

```bash
# Verify schema files are generated
ls -la Lambda.GraphQL.Examples/bin/Debug/net6.0/schema.graphql
ls -la Lambda.GraphQL.Examples/bin/Debug/net6.0/resolvers.json

# Validate JSON structure
cat Lambda.GraphQL.Examples/bin/Debug/net6.0/resolvers.json | jq '.'

# Check schema content
cat Lambda.GraphQL.Examples/bin/Debug/net6.0/schema.graphql
```

### Level 5: Additional Validation

```bash
# Verify NuGet packaging still works
dotnet pack Lambda.GraphQL/Lambda.GraphQL.csproj -c Release --verbosity minimal
```

---

## ACCEPTANCE CRITERIA

- [ ] MSBuild task properly extracts and writes both schema.graphql and resolvers.json files
- [ ] GraphQLResolverAttribute is fully processed with DataSource, Kind, and Functions support
- [ ] Resolver manifest generation creates CDK-compatible JSON structure
- [ ] Pipeline resolver configuration is properly supported and tested
- [ ] Data source deduplication works correctly for multiple resolvers
- [ ] All existing tests continue to pass (29+ tests)
- [ ] New tests validate resolver tracking and manifest generation
- [ ] Build pipeline produces clean output with no warnings or errors
- [ ] Example project demonstrates both unit and pipeline resolver patterns
- [ ] Generated files are properly placed in output directory

---

## COMPLETION CHECKLIST

- [ ] All tasks completed in dependency order
- [ ] Each task validation passed immediately after implementation
- [ ] All validation commands executed successfully
- [ ] Full test suite passes (unit + integration)
- [ ] No linting or type checking errors
- [ ] Manual testing confirms both schema.graphql and resolvers.json are generated
- [ ] JSON manifest structure validated against CDK requirements
- [ ] All acceptance criteria met
- [ ] Code reviewed for quality and maintainability

---

## NOTES

**Design Decisions:**
- Resolver manifest follows AWS AppSync CDK patterns for seamless integration
- Data source deduplication prevents duplicate entries in manifest
- Pipeline resolver support enables advanced AppSync patterns
- MSBuild task path resolution supports both development and NuGet scenarios

**Performance Considerations:**
- Resolver tracking adds minimal overhead to existing source generation
- Data source deduplication uses efficient grouping operations
- JSON serialization uses System.Text.Json for optimal performance

**Backward Compatibility:**
- All existing Phase 1 functionality is preserved
- New resolver attributes are optional and default to sensible values
- MSBuild task maintains existing property names and behavior

**CDK Integration:**
- Generated resolver manifest matches expected CDK construct input format
- Data source configuration supports Lambda function ARN templating
- Resolver configuration includes all required AppSync properties
