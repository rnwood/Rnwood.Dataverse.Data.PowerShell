# Test Infrastructure Documentation

This document explains how the test infrastructure works, particularly the FakeXrmEasy mock connection configuration.

## Overview

The test suite uses **FakeXrmEasy** (open-source version) to create mock Dataverse connections for testing without requiring a real Dataverse environment. The implementation provides:

- **Minimal entity metadata generation** for testing basic CRUD operations
- **Dynamic entity loading** to support testing various entity types
- **Proper isolation** between test runs
- **Documentation of limitations** in the OSS version

## Test Files

- **Common.ps1**: Shared test infrastructure and helper functions
- **Examples.Tests.ps1**: Comprehensive test coverage for documentation examples
- **Get-DataverseRecord.Tests.ps1**: Specific tests for query functionality
- **Module.Tests.ps1**: Module loading and assembly resolution tests
- **DefaultConnection.Tests.ps1**: Default connection functionality tests
- **contact.xml**: Full entity metadata for the contact entity (2.2MB)
- **generate-all-metadata.ps1**: Script to generate metadata files from real Dataverse
- **updatemetadata.ps1**: Legacy script for metadata generation

## Mock Connection Configuration

### `getMockConnection` Function

The `getMockConnection` function in `Common.ps1` creates a FakeXrmEasy-based mock connection:

```powershell
$connection = getMockConnection -AdditionalEntities @("solution", "systemuser")
```

**Parameters:**
- `AdditionalEntities` (optional): Array of entity logical names to include via minimal metadata

**Behavior:**
1. Loads all `.xml` metadata files from the `tests/` directory (e.g., `contact.xml`)
2. Generates minimal metadata for any requested additional entities
3. Creates a FakeXrmEasy context initialized with all metadata
4. Returns a ServiceClient wrapping the mock IOrganizationService

### Minimal Entity Metadata Generation

The `New-MinimalEntityMetadata` function creates basic entity metadata programmatically:

```powershell
$metadata = New-MinimalEntityMetadata -LogicalName "solution" -PrimaryIdAttribute "solutionid"
```

**Generated metadata includes:**
- Entity logical name and schema name
- Primary ID attribute (e.g., "solutionid")
- Primary name attribute (default: "name")
- Minimal attribute collection with ID and name attributes

**Limitations:**
- No relationship metadata
- No option set definitions
- No form/view metadata
- May not support all CRUD operations (full metadata recommended for comprehensive testing)

### Full Entity Metadata

For comprehensive testing, use `generate-all-metadata.ps1` to create full metadata files:

```powershell
# Connect to a real Dataverse environment first
$conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive

# Generate metadata for all required entities
./tests/generate-all-metadata.ps1 -Connection $conn
```

This creates `.xml` files in the `tests/` directory containing complete entity metadata including:
- All attributes with display names and types
- Relationships (1:N, N:1, N:N)
- Option set values with labels
- Entity capabilities and settings

**Entities included in generate-all-metadata.ps1:**
- account
- solution
- systemuser
- team
- workflow
- asyncoperation
- organization
- list
- duplicaterule
- processstage
- savedquery
- userquery
- annotation
- incident
- systemuserroles

## FakeXrmEasy Limitations

The open-source version of FakeXrmEasy has some limitations:

### Supported Features ✅
- Basic CRUD operations (Create, Read, Update, Delete)
- Query operations (QueryExpression, FetchXML)
- Specific message request objects (e.g., `WhoAmIRequest`, `SetStateRequest`)
- Association/disassociation
- Entity metadata retrieval
- Attribute metadata retrieval

### Known Limitations ⚠️

1. **RequestName Parameter Not Supported**
   - The OSS version doesn't support `Invoke-DataverseRequest -RequestName "WhoAmI"`
   - **Workaround**: Use specific request objects instead:
     ```powershell
     # Instead of this (requires commercial license):
     Invoke-DataverseRequest -Connection $conn -RequestName "WhoAmI"
     
     # Use this (works with OSS):
     $request = New-Object Microsoft.Crm.Sdk.Messages.WhoAmIRequest
     Invoke-DataverseRequest -Connection $conn -Request $request
     ```

2. **Minimal Metadata May Not Support All Operations**
   - Creating records with minimal metadata may fail for entities with required attributes
   - **Workaround**: Use full metadata files generated from real Dataverse

3. **Some Advanced Messages Not Fully Implemented**
   - Complex workflow operations
   - Some specialized message executors
   - **Workaround**: Test patterns and cmdlet existence rather than execution

## Test Strategy

### Pattern Testing
For entities without full metadata, tests validate patterns rather than full execution:

```powershell
It "Can query for solutions" {
    # Create connection with minimal metadata
    $connection = getMockConnection -AdditionalEntities @("solution")
    
    # Verify query pattern works (may return empty results)
    { Get-DataverseRecord -Connection $connection -TableName solution } | Should -Not -Throw
}
```

### Cmdlet Existence Testing
For specialized cmdlets, verify they exist and have expected parameters:

```powershell
It "Can use AddMemberList request" {
    $connection = getMockConnection -AdditionalEntities @("list")
    
    # Verify cmdlet exists and accepts expected parameters
    $cmd = Get-Command Invoke-DataverseAddMemberList -ErrorAction SilentlyContinue
    $cmd | Should -Not -BeNull
    $cmd.Parameters.ContainsKey("ListId") | Should -Be $true
}
```

### Full Integration Testing
For entities with full metadata (like contact), perform complete CRUD operations:

```powershell
It "Can create and retrieve contact" {
    $contact = New-Object Microsoft.Xrm.Sdk.Entity("contact")
    $contactId = $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
    $contact["firstname"] = "Test"
    $contact | Set-DataverseRecord -Connection $script:conn
    
    $retrieved = Get-DataverseRecord -Connection $script:conn -TableName contact -Id $contactId
    $retrieved.firstname | Should -Be "Test"
}
```

## Running Tests

### Prerequisites
1. Build the module: `dotnet build -c Release`
2. Set test module path: `$env:TESTMODULEPATH = (Resolve-Path "Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0")`
3. Install Pester: `Install-Module -Force Pester`

### Run All Tests
```powershell
Invoke-Pester -Path tests -Output Detailed
```

### Run Specific Test File
```powershell
Invoke-Pester -Path tests/Examples.Tests.ps1 -Output Detailed
```

### Expected Results
- **Total tests**: 57
- **Passed**: 55
- **Skipped**: 2 (FakeXrmEasy OSS limitations with RequestName parameter)
- **Failed**: 0

## Test Coverage Summary

### Fully Tested (with full metadata)
- ✅ Connection creation and management
- ✅ Basic CRUD operations (contact entity)
- ✅ Query operations (QueryExpression, FetchXML)
- ✅ Batch operations and pipeline support
- ✅ WhoAmI and standard requests
- ✅ Link entity joins
- ✅ Column selection
- ✅ Record counting

### Pattern Tested (with minimal metadata)
- ✅ Solution management queries
- ✅ System user queries
- ✅ Workflow queries
- ✅ Async operation queries
- ✅ Organization settings queries
- ✅ Process stage queries
- ✅ Saved query (view) queries
- ✅ User query (personal view) queries
- ✅ Specialized cmdlet parameter validation

### Skipped (FakeXrmEasy OSS limitations)
- ⏭️ RequestName parameter syntax (requires commercial license)
- ⏭️ Simplified vs. verbose syntax comparison (requires commercial license)

## Extending Tests

### Adding Tests for New Entities

1. **Use minimal metadata** for basic pattern validation:
   ```powershell
   It "Can query new entity" {
       $connection = getMockConnection -AdditionalEntities @("newentity")
       { Get-DataverseRecord -Connection $connection -TableName newentity } | Should -Not -Throw
   }
   ```

2. **Generate full metadata** for comprehensive testing:
   ```powershell
   # Add entity to generate-all-metadata.ps1
   $entities = @(
       "account",
       "newentity"  # Add here
   )
   ```

### Adding Custom Attribute Testing

Extend `New-MinimalEntityMetadata` to include custom attributes:

```powershell
$metadata = New-MinimalEntityMetadata -LogicalName "custom" -PrimaryIdAttribute "customid" -Attributes @{
    "custom_field1" = $true
    "custom_field2" = $true
}
```

## Troubleshooting

### Tests Fail with "Nullable object must have a value"
- **Cause**: Minimal metadata doesn't include required attribute definitions
- **Solution**: Generate full metadata using `generate-all-metadata.ps1`

### Tests Fail with "Entity not found in metadata"
- **Cause**: Entity not included in mock connection metadata
- **Solution**: Add entity to `AdditionalEntities` parameter or create full metadata file

### Tests Fail with "Assembly resolution error"
- **Cause**: Module not built or wrong path
- **Solution**: Run `dotnet build -c Release` and set `$env:TESTMODULEPATH`

### Tests Skip with OSS Limitation Message
- **Expected behavior**: Some features require FakeXrmEasy commercial license
- **Alternative**: Use verbose syntax with specific request objects

## Best Practices

1. **Use full metadata** for entities requiring comprehensive testing
2. **Use minimal metadata** for pattern validation only
3. **Document limitations** when tests are skipped or use workarounds
4. **Test patterns** rather than full execution when metadata is minimal
5. **Verify cmdlet existence** as a lightweight alternative to full execution testing
6. **Keep metadata files** in source control for consistent testing
7. **Regenerate metadata** periodically to stay current with Dataverse schema changes

## Commercial FakeXrmEasy License

For production testing or CI/CD pipelines requiring full fidelity, consider the FakeXrmEasy commercial license which supports:
- RequestName parameter syntax
- All message executors
- Advanced query operations
- Full plugin simulation

See: https://dynamicsvalue.github.io/fake-xrm-easy-docs/licensing/
