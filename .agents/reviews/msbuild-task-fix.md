# Fix: MSBuild Task Not Found in Release Configuration

## Issue
The CDK deployment script (`./deploy.sh`) was failing when building Lambda.GraphQL.Examples with Release configuration:

```
error MSB4036: The "ExtractGraphQLSchemaTask" task was not found.
```

## Root Cause
The `Lambda.GraphQL.targets` file was looking for the MSBuild task assembly in:
```
Lambda.GraphQL.Build/bin/$(Configuration)/net6.0/Lambda.GraphQL.Build.dll
```

When building with `-c Release`, it looked for the Release build, but the task assembly only existed in Debug configuration from previous builds.

## Solution
Added a fallback UsingTask declaration in `Lambda.GraphQL/build/Lambda.GraphQL.targets` that tries Debug configuration if Release doesn't exist:

```xml
<!-- Fallback to Debug if Release doesn't exist -->
<UsingTask TaskName="Lambda.GraphQL.Build.ExtractGraphQLSchemaTask" 
           AssemblyFile="$(MSBuildThisFileDirectory)../../Lambda.GraphQL.Build/bin/Debug/net6.0/Lambda.GraphQL.Build.dll" 
           Condition="!Exists('$(MSBuildThisFileDirectory)../tools/Lambda.GraphQL.Build.dll') AND !Exists('$(MSBuildThisFileDirectory)../../Lambda.GraphQL.Build/bin/$(Configuration)/net6.0/Lambda.GraphQL.Build.dll') AND Exists('$(MSBuildThisFileDirectory)../../Lambda.GraphQL.Build/bin/Debug/net6.0/Lambda.GraphQL.Build.dll')" />
```

## Verification
```bash
cd Lambda.GraphQL.Examples
dotnet build -c Release
# ✓ Build succeeded, schema.graphql and resolvers.json generated

cd ../cdk-example
./deploy.sh
# ✓ Script runs successfully
```

## Impact
- Fixes deployment script for Release builds
- Maintains backward compatibility with Debug builds
- No changes needed to user code or deployment process

## Files Modified
- `Lambda.GraphQL/build/Lambda.GraphQL.targets` - Added Debug configuration fallback
