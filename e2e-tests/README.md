# End-to-End Tests

This directory contains end-to-end tests that run against a real Dataverse environment. These tests validate the module's functionality in a live environment.

## CI/CD Execution

In the CI/CD pipeline, all e2e tests run in parallel for faster execution:
- Each test file runs in its own background job
- **Metadata tests** (EntityMetadata, AttributeMetadata, RelationshipMetadata, OptionSetMetadata) only run on Windows Latest with PowerShell 7+
- All other tests run on all matrix combinations (Windows/Linux × PowerShell 5/7.4.11/latest)
- Results are aggregated and reported with file-level granularity

## Prerequisites

E2E tests require:
1. Access to a Dataverse environment
2. An application registration with client ID and client secret
3. Appropriate permissions in the Dataverse environment

## Environment Setup

Set the following environment variables before running e2e tests:

```powershell
$env:E2ETESTS_URL = "https://yourorg.crm.dynamics.com"
$env:E2ETESTS_CLIENTID = "your-client-id"
$env:E2ETESTS_CLIENTSECRET = "your-client-secret"
```

## Running E2E Tests

### Run All E2E Tests

```powershell
# From repository root
Invoke-Pester -Output Detailed -Path e2e-tests
```

### Run Specific Test File

```powershell
# Run only module tests
Invoke-Pester -Output Detailed -Path e2e-tests/Module.Tests.ps1

# Run only form manipulation tests
Invoke-Pester -Output Detailed -Path e2e-tests/FormManipulation.Tests.ps1
```

## Test Files

### Module.Tests.ps1
General module functionality tests including:
- Connection establishment
- Basic record queries
- SQL queries
- Help system validation
- Parallel processing with Invoke-DataverseParallel
- Web resource management

### FormManipulation.Tests.ps1
Comprehensive form manipulation test that exercises all form-related cmdlets:

**Test Coverage:**
- Form creation (Set-DataverseForm)
- Form retrieval and verification (Get-DataverseForm)
- Tab creation, updates, and removal (Set-DataverseFormTab, Get-DataverseFormTab, Remove-DataverseFormTab)
- Section creation and removal (Set-DataverseFormSection, Get-DataverseFormSection, Remove-DataverseFormSection)
- Control creation, updates, and removal (Set-DataverseFormControl, Get-DataverseFormControl, Remove-DataverseFormControl)
- Publishing after all modifications (Set-DataverseForm -Publish tests the -Publish parameter)
- Multi-column tab layouts (OneColumn, TwoColumns, ThreeColumns)
- Different control types (Standard, Lookup, Email)
- Control properties (IsRequired, Disabled)

**Features:**
- Uses unique test identifiers to avoid conflicts with concurrent test runs
- Includes pre-cleanup to remove leftover forms from previous failed runs
- Comprehensive validation at each step
- Detailed logging of all operations
- Proper cleanup of all created resources

**Cleanup Behavior:**
The form manipulation test automatically cleans up:
1. **Before test**: Removes any leftover forms matching pattern `E2ETestForm-%`
2. **After test**: Removes all created controls, sections, tabs, and forms

This ensures the test can run reliably even if previous runs failed or were interrupted.

### RecordAccess.Tests.ps1
Tests record-level security operations:
- Granting record access
- Testing record access
- Getting record access
- Removing record access

### InvokeDataverseRequest.Tests.ps1
Tests for executing arbitrary organization requests.

### PluginManagement.Tests.ps1
Tests for plugin management operations.

### EntityMetadata.Tests.ps1
Tests for entity metadata operations (runs only on Windows Latest).

### AttributeMetadata.Tests.ps1
Tests for attribute metadata operations (runs only on Windows Latest).

### RelationshipMetadata.Tests.ps1
Tests for relationship metadata operations (runs only on Windows Latest).

### OptionSetMetadata.Tests.ps1
Tests for option set metadata operations (runs only on Windows Latest).

### SolutionComponent.Tests.ps1
Tests for solution component operations.

### AppModule.Tests.ps1
Tests for app module operations.

### Sitemap.Tests.ps1
Tests for sitemap operations.

## Expected Test Duration

- **Module.Tests.ps1**: ~30-60 seconds (depending on network and environment)
- **FormManipulation.Tests.ps1**: ~60-120 seconds (includes form metadata operations)
- **Other tests**: Varies by test complexity and network conditions
- **Total (sequential)**: ~5-10 minutes
- **Total (parallel in CI)**: ~2-3 minutes (significant speedup)

## Notes

- E2E tests spawn child PowerShell processes to test module loading in isolation
- Tests use the built module from `Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0/`
- Tests create temporary copies of the module to avoid file locking issues
- Form manipulation test creates forms on the `account` entity
- All test artifacts are cleaned up automatically

## Troubleshooting

### Authentication Errors
- Verify your client ID and client secret are correct
- Ensure the app registration has appropriate permissions in Dataverse
- Check that the URL is correct and accessible

### Form Creation Failures
- Ensure your user/app has permissions to create and modify forms
- Verify the `account` entity exists in your environment
- Check that form management operations are not blocked by security roles

### Leftover Test Data
The form manipulation test includes automatic cleanup of leftover data from previous runs. If you need to manually clean up:

```powershell
# List test forms
Get-DataverseRecord -Connection $connection -TableName systemform -FilterValues @{
    "name:Like" = "E2ETestForm-%"
    "objecttypecode" = "account"
} -Columns formid, name

# Delete specific form by ID
Remove-DataverseForm -Connection $connection -Id $formId -Confirm:$false
```
