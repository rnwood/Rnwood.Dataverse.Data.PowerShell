# Test Infrastructure Documentation

This document explains how the test infrastructure works, particularly the FakeXrmEasy mock connection configuration and SDK cmdlet testing.

## Overview

The test suite uses **FakeXrmEasy** (open-source version) with a **ProxyOrganizationService wrapper** to create mock Dataverse connections for testing without requiring a real Dataverse environment. The implementation provides:

- **ProxyOrganizationService wrapper** that intercepts and records all IOrganizationService.Execute() calls
- **Minimal entity metadata generation** for testing basic CRUD operations
- **Dynamic entity loading** to support testing various entity types
- **End-to-end SDK cmdlet testing** with request/response validation
- **Proper isolation** between test runs
- **Documentation of limitations** in the OSS version

## Test Files

- **Common.ps1**: Shared test infrastructure and helper functions
- **Examples.Tests.ps1**: Comprehensive test coverage for documentation examples
- **SdkCmdlets.Tests.ps1**: End-to-end tests for SDK cmdlets with proxy verification
- **Get-DataverseRecord.Tests.ps1**: Specific tests for query functionality
- **Module.Tests.ps1**: Module loading and assembly resolution tests
- **DefaultConnection.Tests.ps1**: Default connection functionality tests
- **contact.xml**: Full entity metadata for the contact entity (2.2MB)
- **generate-all-metadata.ps1**: Script to generate metadata files from real Dataverse
- **updatemetadata.ps1**: Legacy script for metadata generation

## ProxyOrganizationService

The `ProxyOrganizationService` class wraps FakeXrmEasy's `IOrganizationService` to enable test inspection and response stubbing:

```csharp
public class ProxyOrganizationService : IOrganizationService
{
    public IReadOnlyList<OrganizationRequest> ExecutedRequests { get; }
    public IReadOnlyList<OrganizationResponse> Responses { get; }
    public OrganizationRequest LastRequest { get; }
    public OrganizationResponse LastResponse { get; }
    
    // Stub responses for FakeXrmEasy OSS limitations
    public void StubResponse(string requestTypeName, Func<OrganizationRequest, OrganizationResponse> responseFactory)
    public void ClearStub(string requestTypeName)
    public void ClearAllStubs()
    public void ClearHistory()
}
```

**Benefits:**
- Records all Execute() calls for inspection in tests
- Enables verification of request parameters and structure
- Validates response handling
- Supports end-to-end testing of SDK cmdlets
- **Stubs responses for operations not supported by FakeXrmEasy OSS**
- Validates parameter type conversions

**Usage in Tests:**
```powershell
# Get the proxy from a mock connection
$proxy = Get-ProxyService -Connection $connection

# Stub a response for unsupported operations
$proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ExportSolutionRequest", {
    param($request)
    
    # Validate request parameters in the stub
    $request.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.ExportSolutionRequest"
    $request.SolutionName | Should -BeOfType [System.String]
    $request.Managed | Should -BeOfType [System.Boolean]
    
    # Create and return mock response
    $response = New-Object Microsoft.Crm.Sdk.Messages.ExportSolutionResponse
    $response.Results["ExportSolutionFile"] = [System.Text.Encoding]::UTF8.GetBytes("mock data")
    return $response
})

# Call the cmdlet - it will execute end-to-end
$response = Invoke-DataverseExportSolution -Connection $connection -SolutionName "Test" -Managed $false

# Verify response type as documented
$response.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.ExportSolutionResponse"
$response.ExportSolutionFile | Should -BeOfType [System.Byte[]]

# Verify the last request
$proxy.LastRequest.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.ExportSolutionRequest"
$proxy.LastRequest.SolutionName | Should -Be "Test"
$proxy.LastRequest.Managed | Should -Be $false
```

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

1. **Some Operations Not Supported by FakeXrmEasy OSS**
   - Operations like ExportSolution, ImportSolution, ExecuteWorkflow, etc.
   - **Solution**: Use response stubbing via ProxyOrganizationService
   - Tests execute end-to-end with stubbed responses
   - Stubs validate parameter conversions and return proper response types

2. **RequestName Parameter Not Supported**
   - The OSS version doesn't support `Invoke-DataverseRequest -RequestName "WhoAmI"`
   - **Workaround**: Use specific request objects instead:
     ```powershell
     # Instead of this (requires commercial license):
     Invoke-DataverseRequest -Connection $conn -RequestName "WhoAmI"
     
     # Use this (works with OSS):
     $request = New-Object Microsoft.Crm.Sdk.Messages.WhoAmIRequest
     Invoke-DataverseRequest -Connection $conn -Request $request
     ```

3. **Minimal Metadata May Not Support All Operations**
   - Creating records with minimal metadata may fail for entities with required attributes
   - **Workaround**: Use full metadata files generated from real Dataverse

## Test Strategy

### SDK Cmdlet Testing with Full Type Validation

All SDK cmdlets should be tested end-to-end with comprehensive type validation:

```powershell
It "Invoke-DataverseRetrieveEntity retrieves entity metadata" {
    # Call the SDK cmdlet
    $response = Invoke-DataverseRetrieveEntity -Connection $script:conn -LogicalName "contact" -EntityFilters All
    
    # Verify response type as documented (use FullName for precision)
    $response | Should -Not -BeNull
    $response.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.RetrieveEntityResponse"
    
    # Verify response properties match documented types
    $response.EntityMetadata | Should -Not -BeNull
    $response.EntityMetadata | Should -BeOfType [Microsoft.Xrm.Sdk.Metadata.EntityMetadata]
    $response.EntityMetadata.LogicalName | Should -Be "contact"
    
    # Verify the proxy captured the request
    $proxy = Get-ProxyService -Connection $script:conn
    $proxy.LastRequest | Should -Not -BeNull
    $proxy.LastRequest.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.RetrieveEntityRequest"
    $proxy.LastRequest.LogicalName | Should -Be "contact"
}
```

### SDK Cmdlet Testing with Response Stubbing

For operations not supported by FakeXrmEasy OSS, stub the response and validate parameter conversions:

```powershell
It "Invoke-DataverseExportSolution exports a solution" {
    $solutionName = "TestSolution"
    
    # Stub the response for FakeXrmEasy OSS limitation
    $proxy = Get-ProxyService -Connection $script:conn
    $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ExportSolutionRequest", {
        param($request)
        
        # Validate request parameters were properly converted
        $request | Should -Not -BeNull
        $request.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.ExportSolutionRequest"
        $request.SolutionName | Should -BeOfType [System.String]
        $request.SolutionName | Should -Be "TestSolution"
        $request.Managed | Should -BeOfType [System.Boolean]
        
        # Create and return mock response
        $response = New-Object Microsoft.Crm.Sdk.Messages.ExportSolutionResponse
        $response.Results["ExportSolutionFile"] = [System.Text.Encoding]::UTF8.GetBytes("fake solution content")
        return $response
    })
    
    # Call the cmdlet - executes end-to-end
    $response = Invoke-DataverseExportSolution -Connection $script:conn -SolutionName $solutionName -Managed $false
    
    # Verify response type as documented
    $response | Should -Not -BeNull
    $response.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.ExportSolutionResponse"
    
    # Verify response properties match documented types
    $response.ExportSolutionFile | Should -Not -BeNull
    $response.ExportSolutionFile | Should -BeOfType [System.Byte[]]
    
    # Verify the proxy captured the request
    $proxy.LastRequest | Should -Not -BeNull
    $proxy.LastRequest.SolutionName | Should -Be $solutionName
}
```

**Key Principles:**
- ✅ Call cmdlets end-to-end with real parameters
- ✅ Stub responses only for FakeXrmEasy OSS limitations
- ✅ Validate parameter conversions in stubs using `GetType().FullName` and `Should -BeOfType`
- ✅ Verify response types using `GetType().FullName` (not just `.Name`)
- ✅ Check all response properties match documented types
- ✅ Use proxy to inspect request parameters
- ✅ Test with actual data when possible
- ✅ No shortcuts - every cmdlet executes completely

**What to Avoid:**
- ❌ Existence-checking tests (Get-Command | Should -Not -BeNull)
- ❌ Parameter checking without execution
- ❌ Tests that only verify cmdlets exist
- ❌ Skipping execution due to FakeXrmEasy limitations (use stubbing instead)

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
- **Total tests**: 82+ (48 in Examples.Tests.ps1 + 34+ in SdkCmdlets.Tests.ps1)
- **Passed**: All tests pass
- **Skipped**: 2 (RequestName parameter syntax limitations)
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

### SDK Cmdlets Tested (end-to-end with full type validation)
- ✅ Invoke-DataverseWhoAmI
- ✅ Invoke-DataverseRetrieveVersion
- ✅ Invoke-DataverseRetrieveAllEntities (with stubbed response)
- ✅ Invoke-DataverseRetrieveEntity
- ✅ Invoke-DataverseAssign
- ✅ Invoke-DataverseAddMembersTeam
- ✅ SetState request (with stubbed response)
- ✅ Invoke-DataversePublishDuplicateRule (with stubbed response)
- ✅ Invoke-DataverseBulkDelete
- ✅ Invoke-DataverseExecuteWorkflow (with stubbed response)
- ✅ Invoke-DataverseRetrieveAttribute
- ✅ Invoke-DataverseRetrieveMultiple
- ✅ Invoke-DataverseCreate
- ✅ Invoke-DataverseUpdate
- ✅ Invoke-DataverseDelete
- ✅ Invoke-DataverseExportSolution (with stubbed response)
- ✅ Invoke-DataverseImportSolution (with stubbed response)
- ✅ Invoke-DataversePublishAllXml (with stubbed response)
- ✅ Invoke-DataverseGrantAccess (with stubbed response)
- ✅ Invoke-DataverseRevokeAccess (with stubbed response)
- ✅ Invoke-DataverseRetrievePrincipalAccess (with stubbed response)
- ✅ Invoke-DataverseAddPrivilegesRole (with stubbed response)
- ✅ Invoke-DataverseRemoveMemberList (with stubbed response)
- ✅ Invoke-DataverseCloseIncident (with stubbed response)
- ✅ Invoke-DataverseSendEmail (with stubbed response)
- ✅ Invoke-DataverseRetrieveOptionSet (with stubbed response)
- ✅ Invoke-DataverseRetrieveRelationship (with stubbed response)
- ✅ Invoke-DataverseSetParentSystemUser (with stubbed response)
- ✅ Invoke-DataverseAddToQueue (with stubbed response)
- ✅ Invoke-DataverseRemoveFromQueue (with stubbed response)
- ✅ Invoke-DataversePickFromQueue (with stubbed response)
- ✅ Invoke-DataverseExecuteAsync (with stubbed response)
- ✅ Invoke-DataverseAddUserToRecordTeam (with stubbed response)
- ✅ Invoke-DataverseBackgroundSendEmail (with stubbed response)
- ✅ And more...

All SDK cmdlet tests include:
- Full type validation with `GetType().FullName`
- Parameter conversion validation in stubs
- Response property type checking
- Request inspection via proxy

### Pattern Tested (with minimal metadata)
- ✅ Solution management queries
- ✅ System user queries
- ✅ Workflow queries
- ✅ Async operation queries
- ✅ Organization settings queries
- ✅ Process stage queries
- ✅ Saved query (view) queries
- ✅ User query (personal view) queries

### Skipped (FakeXrmEasy OSS limitations without workaround)
- ⏭️ RequestName parameter syntax (requires commercial license) - 2 tests
- ⏭️ Simplified vs. verbose syntax comparison (requires commercial license)

Note: All other FakeXrmEasy OSS limitations are now worked around using response stubbing.

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
