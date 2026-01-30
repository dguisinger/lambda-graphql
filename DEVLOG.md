# Development Log - Lambda.GraphQL

**Project**: Lambda.GraphQL - GraphQL Schema Generation for AWS AppSync  
**Duration**: January 8-11, 2026 + January 28, 30, 2026  
**Total Time**: ~20 hours  
**Developer**: Solo project with Kiro CLI assistance

## Project Overview

Lambda.GraphQL generates GraphQL schemas from C# Lambda functions for AWS AppSync using Roslyn source generators. The key innovation is a two-stage generation approach (Roslyn → assembly metadata → MSBuild extraction) that provides compile-time type safety while working around Roslyn's file writing restrictions.

---

## Development Timeline

### Day 1 - January 8, 2026 (3.5 hours)
**Foundation & Architecture**

Established project structure with three packages:
- `Lambda.GraphQL` - Attributes and build integration
- `Lambda.GraphQL.SourceGenerator` - Roslyn incremental generator
- `Lambda.GraphQL.Build` - MSBuild task for schema extraction

**Key Architectural Decision**: Two-stage generation approach
- Roslyn generator embeds schema in assembly metadata (can't write files directly)
- MSBuild task extracts to `schema.graphql` and `resolvers.json` post-build
- Alternative considered: Runtime reflection (rejected for AOT compatibility)

Created Kiro CLI steering documents (`product.md`, `structure.md`, `tech.md`) to maintain consistent AI context across sessions.

### Day 2 - January 9, 2026 (4 hours)
**Core Implementation**

Implemented Roslyn source generator with incremental pipeline:
- Attribute-based type discovery
- GraphQL type mapping (C# → GraphQL SDL)
- Resolver configuration extraction
- Assembly metadata embedding

Built MSBuild task using `MetadataLoadContext` to read assembly without loading into current AppDomain.

**Challenge**: Roslyn security model blocks file I/O. Solution: Embed in assembly, extract via MSBuild.

### Day 3 - January 10, 2026 (4 hours)
**Advanced Features**

Added AppSync-specific features:
- Union types with `[GraphQLUnion]`
- AWS scalar types (AWSDateTime, AWSEmail, AWSJSON, etc.)
- Auth directives (`@aws_cognito_user_pools`, `@aws_api_key`)
- Subscription support
- Custom scalar attributes (`[GraphQLTimestamp]`)

Implemented comprehensive test suite (84 tests) including property-based testing with FsCheck.

### Day 4 - January 11, 2026 (3 hours)
**Documentation & Examples**

Created comprehensive documentation:
- Getting Started guide
- API Reference
- Advanced Features guide
- AWS Integration guide
- Architecture documentation

Built example project demonstrating all features with realistic Lambda functions.

### Day 5 - January 28, 2026 (1.5 hours)
**Polish & CDK Integration**

Created production-ready CDK deployment example:
- TypeScript CDK stack consuming generated manifest
- Automated Lambda function creation from resolver config
- AppSync API setup with auth and logging
- Infrastructure-as-code from C# attributes

### Day 6 - January 30, 2026 (1.5 hours)
**AppSync Validation & Bug Fixes**

Tested generated schema in actual AppSync console, discovered critical issues:

**Bug 1: AWS Scalar Type Mapping**
- Problem: Generated `IPAddress` and `Uri` instead of `AWSIPAddress` and `AWSURL`
- Root cause: `TypeMapper` used simple type names without full qualification
- Fix: Changed to `SymbolDisplayFormat.FullyQualifiedFormat` for accurate type resolution

**Bug 2: Invalid Enum Directives**
- Problem: AppSync rejected `@aws_cognito_user_pools` on enum types
- Fix: Removed directive generation for enums (only valid on objects/fields)

**Bug 3: Lambda Annotations Architecture Mismatch**
- Problem: Assumed data sources could be shared across Lambda functions
- Reality: Each `[LambdaFunction]` method becomes a separate Lambda
- Fix: Auto-generate unique data source per function, updated CDK accordingly

**Bug 4: Unused Runtime Field**
- Problem: Manifest included confusing `runtime: "APPSYNC_JS"` field
- Fix: Removed entirely (AppSync resolver runtime is CDK concern, not C# code)

Created `docs/resolver-manifest.md` documenting Lambda Annotations architecture and CDK integration patterns.

---

## Day 7 - January 30, 2026 (Afternoon) - Lambda Annotations Integration & Deployment Testing
**Time Spent**: 3 hours

### Lambda Annotations Configuration Extraction

**Issue**: CDK was hardcoding Lambda configuration (memory, timeout) instead of using values from `[LambdaFunction]` attribute.

**Implementation**:
- Added extraction of `MemorySize`, `Timeout`, `ResourceName`, `Role`, and `Policies` from `[LambdaFunction]` attribute
- Extended `ResolverInfo` model with Lambda configuration properties
- Updated manifest generator to include configuration in `resolvers.json`
- Modified CDK to use extracted values with sensible defaults

**Example**:
```csharp
[LambdaFunction(MemorySize = 1024, Timeout = 30)]
[GraphQLQuery("getProduct")]
public Task<Product> GetProduct(string id) { }
```
Generates: `{ "memorySize": 1024, "timeout": 30 }` in manifest, which CDK uses for Lambda configuration.

### Data Source Name Management

**Issue**: Manual `DataSource` parameter caused confusion and potential conflicts with Lambda Annotations' one-function-per-method architecture.

**Solution**:
- Made `DataSource` optional on `[GraphQLResolver]` attribute
- Auto-generate unique names (`{MethodName}DataSource`) when not specified
- Added compile-time validation to detect duplicate data source names pointing to different Lambda functions
- Provides clear error message if conflicts detected

**Rationale**: Each Lambda function needs its own data source in AppSync. Auto-generation ensures uniqueness while allowing manual override for special cases.

### AppSync Resolver Payload Handling

**Issue**: Lambda functions were failing with deserialization errors because AppSync was sending full context object but Lambda expected simple parameters.

**Root Cause**: Lambda Annotations with `ANNOTATIONS_HANDLER` expects payload to match method signature exactly.

**Solution**:
- Added `UsesLambdaContext` flag to track if Lambda uses `ILambdaContext` parameter
- Generate different AppSync resolver code based on Lambda signature:
  - **Simple parameters**: Send `ctx.arguments` directly (or single value for single-arg methods)
  - **Uses ILambdaContext**: Send full context with `field`, `arguments`, `source`, `identity`, `request`
- CDK generates appropriate JavaScript resolver code based on flag

**Example**:
```csharp
// Simple - sends just "1234"
public Task<Product> GetProduct(string id)

// Context-aware - sends full AppSync context
public Task<Product> GetProduct(string id, ILambdaContext context)
```

### JSON Serialization Alignment

**Issue**: GraphQL field renamed with `[GraphQLField("displayName")]` but Lambda returned `{ Name: "..." }` causing null errors.

**Solution**: Added `[JsonPropertyName("displayName")]` to ensure JSON serialization matches GraphQL schema field names.

**Learning**: When renaming GraphQL fields, must also control JSON serialization to match.

### Code Organization

- Extracted Lambda functions from `AdvancedTypes.cs` into separate `AdvancedFunctions.cs`
- Removed unnecessary `GeneratedTest` file generation
- Cleaned up example project structure for clarity

### Deployment Validation

Successfully deployed to AWS AppSync and verified:
- ✅ Schema uploaded correctly
- ✅ Data sources created (one per Lambda function)
- ✅ Resolvers configured with correct payload handling
- ✅ Lambda functions deployed with extracted configuration
- ✅ GraphQL queries execute successfully

---

## Final Statistics

- **Total Time**: ~20 hours
- **Lines of Code**: ~3,500 (source) + 6,500 (documentation)
- **Tests**: 84 (100% passing)
- **Packages**: 3 (main, source generator, build task)
- **Documentation Files**: 9
- **Example Projects**: 2 (Lambda functions, CDK deployment)
- **Deployed & Tested**: AWS AppSync with working resolvers

---

## Key Technical Insights

💡 **Two-stage generation** (Roslyn → assembly metadata → MSBuild) elegantly solves Roslyn's file writing constraints while maintaining compile-time safety

💡 **Roslyn symbol resolution** requires `SymbolDisplayFormat.FullyQualifiedFormat` for accurate type mapping, especially with AWS scalar types

💡 **Lambda Annotations architecture** means one Lambda function per method, not shared handlers - critical for correct data source mapping

💡 **CDK manifest-driven deployment** enables complete infrastructure-as-code from C# attributes, bridging .NET and AWS CDK worlds

💡 **AppSync resolver payload format** must match Lambda Annotations expectations - single values for single parameters, objects for multiple parameters

💡 **JSON serialization alignment** critical when renaming GraphQL fields - use `[JsonPropertyName]` to match schema

💡 **Kiro CLI steering documents** provide consistent AI context across multi-day projects, essential for maintaining architectural coherence

---

## Retrospective

### What Went Well
✅ Clean architecture with proper separation of concerns  
✅ Comprehensive feature set covering all major AppSync capabilities  
✅ Zero build warnings, 100% test pass rate  
✅ Production-ready CDK deployment example  
✅ Real-world AppSync validation caught critical bugs before release  
✅ Successfully deployed and tested end-to-end with AWS AppSync  
✅ Lambda Annotations configuration flows through to deployed infrastructure  

### Challenges Overcome
🔧 Roslyn file writing restrictions → Two-stage generation approach  
🔧 Type resolution accuracy → FullyQualifiedFormat discovery  
🔧 Lambda Annotations routing → Architecture deep-dive and CDK fixes  
🔧 AppSync schema validation → Real-world testing revealed edge cases  
🔧 Payload deserialization → Context-aware resolver generation  
🔧 JSON serialization → Field name alignment with GraphQL schema  

---

**Status**: Complete, deployed, and tested. All validation issues resolved, comprehensive documentation, production-ready examples with working AWS deployment.
