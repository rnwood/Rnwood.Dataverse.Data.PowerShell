# Get-DataverseConnection Test Suite Organization

This document describes the new test file organization for Get-DataverseConnection cmdlet tests.

## Files Created

### 1. `Get-DataverseConnection.ps1`
**Status:** Already existed  
**Contains:** Comprehensive tests for the Get-DataverseConnection cmdlet covering:
- Default connection functionality (SetAsDefault, GetDefault, default connection usage)
- Named connections (Save, retrieve, list, delete)
- Certificate authentication (parameter validation, certificate loading)
- Parameter validation and error handling

### 2. `Get-DataverseConnection-Default.ps1` (NEW)
**Purpose:** Split out Default Connection tests for better organization  
**Contains:**
- SetAsDefault switch functionality
- GetDefault parameter set
- Default connection usage by other cmdlets
- Error handling when no default is set
- Connect-DataverseConnection alias validation
- GetDefault parameter set independence
- SetAsDefault works with Mock parameter set
- AccessToken parameter set compatibility

**Key Tests:**
- `It "SetAsDefault switch stores connection as default"`
- `It "GetDefault returns error when no default is set"`
- `It "Cmdlets use default connection when -Connection not provided"`
- `It "Cmdlets error when no connection provided and no default set"`
- `It "Connect-DataverseConnection alias works"`

### 3. `Get-DataverseConnection-Certificate.ps1` (NEW)
**Purpose:** Split out Certificate Authentication tests for better organization  
**Contains:**
- Certificate parameter set availability
- ClientCertificate parameter type validation
- Thumbprint parameter handling
- Certificate authentication prerequisites (TenantId, ClientId, Url)
- Certificate store location and name parameters
- Multiple certificate parameter sets (ClientCertificate and Thumbprint)
- Certificate error handling

**Key Tests:**
- `It "Certificate parameter set uses thumbprint for authentication"`
- `It "ClientCertificate parameter accepts certificate object"`
- `It "Thumbprint parameter accepts string"`
- `It "Certificate parameter set requires TenantId"`
- `It "Certificate parameter set requires ClientId"`
- `It "Certificate parameter set requires Url"`
- `It "Multiple certificate parameter sets exist"`

### 4. `Get-DataverseConnection-Named.ps1` (NEW)
**Purpose:** Split out Named Connections tests for better organization  
**Contains:**
- Named connection listing (List parameter set)
- Saving connections with SaveAs
- Retrieving saved connections by name
- ForceRefresh parameter with named connections
- Remove-DataverseConnection cmdlet functionality
- Connection persistence across PowerShell sessions
- Special characters in connection names
- Multiple named connections storage
- SetAsDefault with named connections
- Case-insensitive name matching

**Key Tests:**
- `It "Get-DataverseConnection lists all named connections"`
- `It "List parameter shows stored connections"`
- `It "SaveAs stores connection with name"`
- `It "Name parameter retrieves saved connection"`
- `It "SaveAs parameter set works with Mock"`
- `It "Persisted connection survives new PowerShell session"`
- `It "Multiple named connections can be stored"`
- `It "SetAsDefault works with named connections"`

## Test Execution

### Running All Tests
Tests are automatically discovered and executed by the `All.Tests.ps1` runner:

```powershell
# Set module path
$env:TESTMODULEPATH = (Resolve-Path "Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0")

# Run all tests
Invoke-Pester -Output Detailed -Path tests
```

### Running Specific Test Suite
Individual test files can be run directly:

```powershell
# Run default connection tests
Invoke-Pester -Output Detailed -Path tests/Get-DataverseConnection-Default.ps1

# Run certificate tests
Invoke-Pester -Output Detailed -Path tests/Get-DataverseConnection-Certificate.ps1

# Run named connection tests
Invoke-Pester -Output Detailed -Path tests/Get-DataverseConnection-Named.ps1
```

## How Tests Are Organized

The test files use the following naming convention to ensure they are discovered by `All.Tests.ps1`:

- Files ending in `.ps1` (without `.Tests.ps1`) are automatically included
- Files matching the patterns `generate-*.ps1`, `updatemetadata.ps1`, `All.Tests.ps1`, or `*.Tests.ps1` are excluded
- The `getMockConnection` helper function from `All.Tests.ps1` is available in all test files

## Supporting Infrastructure

### `All.Tests.ps1`
- Provides `getMockConnection()` helper function
- Loads metadata from `contact.xml`
- Sets up module paths for testing
- Automatically discovers and includes all test files

### `contact.xml`
- Contains serialized EntityMetadata for the 'contact' entity
- Used by `getMockConnection()` to create mock connections
- 2.2MB file containing complete metadata structure

## Test Coverage

The split test files provide comprehensive coverage for:

1. **Default Connection Tests** (Get-DataverseConnection-Default.ps1)
   - Connection lifecycle management
   - Default connection usage across cmdlets
   - Error handling for missing defaults

2. **Certificate Authentication Tests** (Get-DataverseConnection-Certificate.ps1)
   - Multiple certificate authentication methods
   - Parameter validation and requirements
   - Certificate store integration

3. **Named Connections Tests** (Get-DataverseConnection-Named.ps1)
   - Connection storage and retrieval
   - Session persistence
   - Connection list management
   - Multiple connection scenarios

## Integration Notes

- All files follow the same testing patterns and conventions
- Use `Pester` framework with `Describe` and `It` blocks
- Use `Should` assertions for test validation
- Child PowerShell processes for isolation when needed
- Mock connections via FakeXrmEasy for unit testing

## Future Enhancements

Potential areas for additional test coverage:

- Interactive authentication scenarios (requires user input)
- Device code flow testing
- Client secret authentication
- Access token retrieval and caching
- Connection timeout and retry scenarios
- Multi-tenant scenarios
- Error recovery and resilience
