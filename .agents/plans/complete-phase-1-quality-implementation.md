# Feature: Complete Phase 1 Quality Implementation - Production-Ready Core Schema Generation

The following plan addresses critical quality gaps in the current Phase 1 implementation to deliver production-ready GraphQL schema generation within the defined hackathon scope.

## Feature Description

Transform the current Phase 1 MVP into production-quality code by fixing hardcoded return types in SDL generation, implementing the missing MSBuild task for schema extraction, and replacing generic exception handling with proper diagnostic reporting. This ensures the implementation fully delivers on Phase 1 specifications with robust error handling and complete functionality.

## User Story

As a .NET developer using Lambda.GraphQL
I want accurate GraphQL schemas generated from my C# code with proper build integration and meaningful error messages
So that I can confidently use this tool in production environments and get actionable feedback when issues occur

## Problem Statement

The current Phase 1 implementation has three critical gaps that prevent production use:
1. **Hardcoded Return Types**: SDL generation hardcodes all operation return types as "String", producing incomplete/incorrect schemas
2. **Missing MSBuild Task**: Core Phase 1 deliverable not implemented, preventing automated schema extraction
3. **Generic Exception Handling**: Swallows all exceptions without diagnostic reporting, making debugging impossible

## Solution Statement

Implement proper return type extraction from method symbols, create a functional MSBuild task for schema extraction, and replace generic exception handling with specific diagnostic reporting. This maintains the existing architecture while completing the Phase 1 specification to production quality.

## Feature Metadata

**Feature Type**: Bug Fix/Enhancement
**Estimated Complexity**: Medium
**Primary Systems Affected**: Source Generator, MSBuild Task, SDL Generation, Error Handling
**Dependencies**: Microsoft.Build.Framework, Microsoft.CodeAnalysis, System.Reflection.MetadataLoadContext

---

## CONTEXT REFERENCES

### Relevant Codebase Files IMPORTANT: YOU MUST READ THESE FILES BEFORE IMPLEMENTING!

- `Lambda.GraphQL.SourceGenerator/GraphQLSchemaGenerator.cs` (lines 76, 200, 280-320) - Why: Contains generic exception handling that needs specific diagnostics
- `Lambda.GraphQL.SourceGenerator/SdlGenerator.cs` (lines 120-140) - Why: Contains hardcoded "String" return types that need proper extraction
- `Lambda.GraphQL.SourceGenerator/Models/ResolverInfo.cs` - Why: Model structure for resolver information including return types
- `Lambda.GraphQL.Build/` (entire directory) - Why: MSBuild task implementation location
- `Lambda.GraphQL.Tests/TypeMapperTests.cs` - Why: Testing pattern for Roslyn compilation and type analysis
- `Directory.Build.props` - Why: Centralized build configuration for MSBuild integration

### New Files to Create

- `Lambda.GraphQL.Build/ExtractGraphQLSchemaTask.cs` - MSBuild task implementation for schema extraction
- `Lambda.GraphQL.SourceGenerator/ReturnTypeExtractor.cs` - Utility for extracting method return types
- `Lambda.GraphQL.Tests/ExtractGraphQLSchemaTaskTests.cs` - Unit tests for MSBuild task
- `Lambda.GraphQL.Tests/ReturnTypeExtractionTests.cs` - Tests for return type extraction

### Relevant Documentation YOU SHOULD READ THESE BEFORE IMPLEMENTING!

- [Microsoft MSBuild Task Writing](https://learn.microsoft.com/en-us/visualstudio/msbuild/task-writing?view=vs-2022)
  - Specific section: ITask interface and Execute method implementation
  - Why: Required for implementing MSBuild task correctly
- [Roslyn Source Generator Diagnostics](https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md)
  - Specific section: Diagnostic reporting with context.ReportDiagnostic
  - Why: Shows proper error reporting patterns for source generators

### Patterns to Follow

**Naming Conventions:**
- PascalCase for classes and public members
- camelCase for private fields and local variables
- Suffix "Attribute" for attribute classes
- Suffix "Tests" for test classes

**Error Handling Pattern (from existing codebase):**
```csharp
// Current (bad):
catch { return null; }

// Target (good):
catch (SpecificException ex)
{
    context.ReportDiagnostic(Diagnostic.Create(descriptor, location, ex.Message));
    return null;
}
```

**Testing Pattern (from TypeMapperTests.cs):**
```csharp
private static Compilation CreateCompilation(string source)
{
    var syntaxTree = CSharpSyntaxTree.ParseText(source);
    var references = new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) };
    return CSharpCompilation.Create("TestAssembly", new[] { syntaxTree }, references, 
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
}
```

**MSBuild Task Pattern:**
```csharp
public class TaskName : Task
{
    [Required]
    public string RequiredProperty { get; set; }
    
    public override bool Execute()
    {
        try
        {
            // Implementation
            return true;
        }
        catch (Exception ex)
        {
            Log.LogError("Error: {0}", ex.Message);
            return false;
        }
    }
}
```

---

## IMPLEMENTATION PLAN

### Phase 1: Foundation - Diagnostic Infrastructure

**Tasks:**
- Create diagnostic descriptors for common source generator errors
- Implement helper methods for diagnostic reporting
- Update exception handling patterns to use diagnostics

### Phase 2: Core Implementation - Return Type Extraction

**Tasks:**
- Implement return type extraction from method symbols
- Update SDL generation to use actual return types
- Add comprehensive type mapping for complex return types

### Phase 3: MSBuild Integration - Schema Extraction Task

**Tasks:**
- Implement MSBuild task for schema extraction
- Add task registration and build targets
- Integrate with existing build pipeline

### Phase 4: Testing & Validation - Comprehensive Coverage

**Tasks:**
- Add unit tests for all new functionality
- Create integration tests for end-to-end workflows
- Validate against Phase 1 acceptance criteria

---

## STEP-BY-STEP TASKS

IMPORTANT: Execute every task in order, top to bottom. Each task is atomic and independently testable.

### CREATE Lambda.GraphQL.SourceGenerator/DiagnosticDescriptors.cs

- **IMPLEMENT**: Static diagnostic descriptor definitions for common source generator errors
- **PATTERN**: Follow existing diagnostic patterns from Roslyn documentation
- **IMPORTS**: Microsoft.CodeAnalysis, Microsoft.CodeAnalysis.CSharp
- **GOTCHA**: Diagnostic IDs must be unique and follow LGQL### format
- **VALIDATE**: `dotnet build --verbosity minimal`

### UPDATE Lambda.GraphQL.SourceGenerator/GraphQLSchemaGenerator.cs

- **IMPLEMENT**: Replace generic catch blocks with specific exception handling and diagnostic reporting
- **PATTERN**: Use context.ReportDiagnostic with specific descriptors (lines 76, 200)
- **IMPORTS**: Add using for DiagnosticDescriptors
- **GOTCHA**: Don't break existing functionality - maintain null returns for graceful degradation
- **VALIDATE**: `dotnet build --verbosity minimal && dotnet test`

### CREATE Lambda.GraphQL.SourceGenerator/ReturnTypeExtractor.cs

- **IMPLEMENT**: Static utility class for extracting return types from method symbols
- **PATTERN**: Mirror TypeMapper.cs structure and patterns
- **IMPORTS**: Microsoft.CodeAnalysis, System.Linq
- **GOTCHA**: Handle generic types, Task<T>, and nullable return types correctly
- **VALIDATE**: `dotnet build --verbosity minimal`

### UPDATE Lambda.GraphQL.SourceGenerator/SdlGenerator.cs

- **IMPLEMENT**: Replace hardcoded "String" return types with actual extracted types
- **PATTERN**: Use ReturnTypeExtractor in GenerateRootType method (line 130)
- **IMPORTS**: Add using for ReturnTypeExtractor
- **GOTCHA**: Maintain backward compatibility for operations without return type info
- **VALIDATE**: `dotnet build --verbosity minimal && dotnet test`

### CREATE Lambda.GraphQL.Build/ExtractGraphQLSchemaTask.cs

- **IMPLEMENT**: MSBuild task that extracts schema from generated assembly metadata
- **PATTERN**: Follow MSBuild Task base class pattern with Execute method
- **IMPORTS**: Microsoft.Build.Framework, Microsoft.Build.Utilities, System.Reflection
- **GOTCHA**: Handle cases where assembly doesn't contain generated schema metadata
- **VALIDATE**: `dotnet build Lambda.GraphQL.Build --verbosity minimal`

### UPDATE Lambda.GraphQL/build/Lambda.GraphQL.targets

- **IMPLEMENT**: MSBuild targets that execute schema extraction task after build
- **PATTERN**: Follow existing MSBuild target patterns in .NET ecosystem
- **IMPORTS**: Reference ExtractGraphQLSchemaTask assembly
- **GOTCHA**: Ensure task runs after compilation but before packaging
- **VALIDATE**: `dotnet build Lambda.GraphQL.Examples --verbosity minimal`

### CREATE Lambda.GraphQL.Tests/ReturnTypeExtractionTests.cs

- **IMPLEMENT**: Unit tests for return type extraction functionality
- **PATTERN**: Mirror TypeMapperTests.cs structure and compilation creation
- **IMPORTS**: xUnit, FluentAssertions, Microsoft.CodeAnalysis.CSharp
- **GOTCHA**: Test complex scenarios like Task<Product>, List<Product>, nullable types
- **VALIDATE**: `dotnet test --verbosity minimal`

### CREATE Lambda.GraphQL.Tests/ExtractGraphQLSchemaTaskTests.cs

- **IMPLEMENT**: Unit tests for MSBuild task functionality
- **PATTERN**: Follow MSBuild task testing patterns with mock build engine
- **IMPORTS**: xUnit, FluentAssertions, Microsoft.Build.Framework
- **GOTCHA**: Mock IBuildEngine for testing without actual MSBuild execution
- **VALIDATE**: `dotnet test --verbosity minimal`

### UPDATE Lambda.GraphQL.Tests/SdlGeneratorTests.cs

- **IMPLEMENT**: Add tests for accurate return type generation in SDL
- **PATTERN**: Extend existing test methods with return type validation
- **IMPORTS**: No new imports needed
- **GOTCHA**: Verify generated SDL contains correct return types, not hardcoded "String"
- **VALIDATE**: `dotnet test --verbosity minimal`

### CREATE Lambda.GraphQL.Tests/DiagnosticReportingTests.cs

- **IMPLEMENT**: Integration tests for diagnostic reporting during compilation errors
- **PATTERN**: Create invalid C# code and verify diagnostics are reported
- **IMPORTS**: xUnit, FluentAssertions, Microsoft.CodeAnalysis.CSharp
- **GOTCHA**: Test that diagnostics appear in compilation results, not just logged
- **VALIDATE**: `dotnet test --verbosity minimal`

---

## TESTING STRATEGY

### Unit Tests

Design unit tests with fixtures and assertions following existing xUnit + FluentAssertions pattern:
- **ReturnTypeExtractor**: Test type mapping for all supported C# return types
- **MSBuild Task**: Test schema extraction with mock assemblies and build contexts
- **Diagnostic Reporting**: Verify specific diagnostics are created for known error conditions
- **SDL Generation**: Validate accurate return types appear in generated GraphQL schema

### Integration Tests

- **End-to-End Schema Generation**: Build Lambda.GraphQL.Examples and verify complete, accurate schema output
- **MSBuild Integration**: Test that schema files are created in correct output directories
- **Error Scenarios**: Verify meaningful error messages for malformed C# code

### Edge Cases

- Generic return types: `Task<List<Product>>`, `IEnumerable<Product?>`
- Nullable reference types: `Product?`, `string?`
- Complex types: Custom classes, interfaces, enums
- Error conditions: Missing attributes, invalid method signatures, compilation failures

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
dotnet build Lambda.GraphQL.Examples --verbosity minimal
find Lambda.GraphQL.Examples/bin -name "*.graphql" -o -name "*.json"
```

### Level 4: Manual Validation

```bash
# Verify schema extraction produces files
dotnet build Lambda.GraphQL.Examples -c Release
ls -la Lambda.GraphQL.Examples/bin/Release/net6.0/

# Verify NuGet packaging still works
dotnet pack Lambda.GraphQL/Lambda.GraphQL.csproj -c Release --verbosity minimal
```

### Level 5: Additional Validation

```bash
# Verify diagnostic reporting works
# Create intentionally broken C# code and verify meaningful errors appear
```

---

## ACCEPTANCE CRITERIA

- [ ] Source generator reports specific diagnostics instead of swallowing exceptions
- [ ] SDL generation uses actual method return types instead of hardcoded "String"
- [ ] MSBuild task successfully extracts schema files to output directory
- [ ] All existing tests continue to pass (no regressions)
- [ ] New functionality has comprehensive unit test coverage (80%+)
- [ ] Integration tests verify end-to-end schema generation works correctly
- [ ] Build pipeline produces clean output with no warnings or errors
- [ ] Generated schemas are accurate and complete for example Lambda functions
- [ ] Error scenarios produce meaningful, actionable diagnostic messages

---

## COMPLETION CHECKLIST

- [ ] All tasks completed in dependency order
- [ ] Each task validation passed immediately after implementation
- [ ] All validation commands executed successfully
- [ ] Full test suite passes (unit + integration)
- [ ] No linting or type checking errors
- [ ] Manual testing confirms accurate schema generation
- [ ] MSBuild integration works in example project
- [ ] Diagnostic reporting provides meaningful error messages
- [ ] Code quality meets production standards
- [ ] Phase 1 specification fully implemented

---

## NOTES

**Design Decisions:**
- MSBuild task uses assembly metadata extraction as primary method, with source file parsing as fallback
- Diagnostic reporting uses Warning severity by default to avoid breaking builds, with Error severity for critical issues
- Return type extraction handles Task<T> unwrapping for async Lambda functions
- Backward compatibility maintained for existing attribute usage patterns

**Trade-offs:**
- MSBuild task adds build complexity but enables automated CI/CD integration
- Specific exception handling increases code size but dramatically improves debugging experience
- Return type extraction adds processing overhead but produces accurate schemas

**Future Considerations:**
- Phase 2 pipeline resolver support will build on the return type extraction infrastructure
- Advanced GraphQL features (Phase 3) will extend the diagnostic reporting system
- Performance optimization may be needed for large codebases with many Lambda functions
