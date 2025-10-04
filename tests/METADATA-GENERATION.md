# Entity Metadata Generation Guide

This guide explains how to generate the entity metadata XML files required for comprehensive test coverage.

## Current State

- ✅ **contact.xml** (2.2MB) - Fully functional, all contact-related tests pass
- ⏸️ **13 additional entities needed** for remaining 44 tests

## Why Full Metadata is Required

FakeXrmEasy mock provider requires complete entity metadata including:
- All AttributeMetadata with types, schema names, and properties
- Relationship definitions
- Option sets with localized labels
- Primary key and name attributes

Simply creating EntityMetadata objects programmatically without AttributeMetadata causes runtime errors.

## Required Entities

The following entities are referenced in Examples-Comparison.md and need metadata:

1. **account** - For relationship/join examples
2. **solution** - Solution management operations
3. **systemuser** - User and team operations
4. **team** - Team membership operations
5. **workflow** - Workflow management examples
6. **asyncoperation** - Async job monitoring
7. **organization** - Organization settings
8. **list** - Marketing list operations
9. **duplicaterule** - Duplicate detection
10. **processstage** - Business process flow examples
11. **savedquery** - System views
12. **userquery** - Personal views  
13. **annotation** - Notes and attachments
14. **incident** - For CloseIncident request example
15. **systemuserroles** - For security role assignment

## Generation Steps

### Prerequisites

- Access to a Dataverse environment (test/staging recommended)
- PowerShell 7+ or PowerShell 5.1+
- Module built: `dotnet build -c Release`

### Step 1: Connect to Dataverse

```powershell
# Navigate to repository root
cd /path/to/Rnwood.Dataverse.Data.PowerShell

# Set module path
$env:TESTMODULEPATH = "$(pwd)/Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0"

# Import module
Import-Module Rnwood.Dataverse.Data.PowerShell

# Connect (choose your auth method)
$conn = Get-DataverseConnection -Url "https://yourenv.crm.dynamics.com" -Interactive
# OR
$conn = Get-DataverseConnection -Url "https://yourenv.crm.dynamics.com" -ClientId "your-id" -ClientSecret "your-secret"
```

### Step 2: Run Enhanced Metadata Generation Script

```powershell
# Navigate to tests directory
cd tests

# Run the enhanced script (creates all required entity metadata files)
./generate-all-metadata.ps1 -Connection $conn
```

### Step 3: Verify Generated Files

```powershell
# Check generated files
ls *.xml

# Should see:
# contact.xml (already exists)
# account.xml
# solution.xml
# systemuser.xml
# team.xml
# workflow.xml
# asyncoperation.xml
# organization.xml
# list.xml
# duplicaterule.xml
# processstage.xml
# savedquery.xml
# userquery.xml
# annotation.xml
# incident.xml
# systemuserroles.xml
```

### Step 4: Run Tests

```powershell
# Return to repository root
cd ..

# Set test module path
$env:TESTMODULEPATH = "$(pwd)/Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0"

# Run all tests
Invoke-Pester -Path tests/Examples.Tests.ps1 -Output Detailed

# Should see: Tests Passed: 65/65 ✓
```

## Enhanced Generation Script

The script `tests/generate-all-metadata.ps1` will be created to automate the process:

```powershell
param(
    [Parameter(Mandatory=$true)]
    [Microsoft.PowerPlatform.Dataverse.Client.ServiceClient]$Connection
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName "System.Runtime.Serialization"

$entities = @(
    "account",
    "solution",
    "systemuser",
    "team",
    "workflow",
    "asyncoperation",
    "organization",
    "list",
    "duplicaterule",
    "processstage",
    "savedquery",
    "userquery",
    "annotation",
    "incident",
    "systemuserroles"
)

$serializer = New-Object System.Runtime.Serialization.DataContractSerializer([Microsoft.Xrm.Sdk.Metadata.EntityMetadata])

Write-Host "Generating metadata for $($entities.Count) entities..." -ForegroundColor Cyan

foreach($entityname in $entities) {
    Write-Host "  Retrieving $entityname..." -NoNewline
    
    try {
        $em = (Invoke-DataverseRequest -Connection $Connection -RequestName "RetrieveEntity" -Parameters @{
            "LogicalName" = $entityname
            "EntityFilters" = [Microsoft.Xrm.Sdk.Metadata.EntityFilters]::All
            "MetadataId" = [Guid]::Empty
            "RetrieveAsIfPublished" = $false
        }).Results["EntityMetadata"]
        
        $outputPath = Join-Path $PSScriptRoot "${entityname}.xml"
        $outputStream = [IO.File]::OpenWrite($outputPath)
        $serializer.WriteObject($outputStream, $em)
        $outputStream.Close()
        
        $sizeKB = [Math]::Round((Get-Item $outputPath).Length / 1KB, 2)
        Write-Host " ✓ ($sizeKB KB)" -ForegroundColor Green
    }
    catch {
        Write-Host " ✗ Failed: $_" -ForegroundColor Red
    }
}

Write-Host "`nMetadata generation complete!" -ForegroundColor Green
Write-Host "Files created in: $PSScriptRoot" -ForegroundColor Cyan
```

## File Sizes (Approximate)

Based on contact.xml (2.2MB), expect:
- Total size: 30-50MB for all 16 entities
- Each entity: 1-3MB depending on complexity
- Largest: systemuser, organization, incident
- Smallest: simple entities with few attributes

## Committing Metadata Files

After generation, commit the files to the repository:

```bash
git add tests/*.xml
git commit -m "Add entity metadata files for comprehensive test coverage"
```

These files enable all 65+ tests to pass automatically in CI/CD.

## Alternative: E2E Tests

If committing large metadata files is not desirable, an alternative is to use E2E tests that connect to a real Dataverse environment. However, this requires:
- Dataverse environment credentials in CI/CD
- Slower test execution
- External dependencies
- More complex setup

The metadata file approach is recommended for better developer experience and faster, more reliable tests.

## Troubleshooting

### Error: "Value cannot be null (Parameter: 'source')"
- Cause: Incomplete AttributeMetadata
- Solution: Regenerate metadata XML using RetrieveEntity with EntityFilters.All

### Error: Entity not found
- Cause: Entity doesn't exist in Dataverse environment
- Solution: Use a different environment or skip examples for that entity

### Large file sizes
- Normal: Entity metadata with all attributes is verbose
- contact.xml at 2.2MB is typical
- Git LFS not needed for ~50MB total

## Support

If you encounter issues:
1. Verify Dataverse connection works
2. Check entity exists: `Get-DataverseRecord -Connection $conn -TableName <entityname> -RecordCount`
3. Ensure EntityFilters.All is used for complete metadata
4. Check PowerShell version: `$PSVersionTable.PSVersion`

## Summary

- **Time required:** 5-10 minutes (one-time setup)
- **Prerequisites:** Dataverse access
- **Result:** 100% test coverage (65/65 tests passing)
- **Benefit:** Fast, reliable, offline-capable tests
