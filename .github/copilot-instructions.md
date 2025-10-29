# Rnwood.Dataverse.Data.PowerShell - Copilot Agent Instructions

## Repository Summary
Cross-platform PowerShell module (~206MB, 1339 files) for Microsoft Dataverse data manipulation. Targets .NET 6.0 (PowerShell Core 7+) and .NET Framework 4.6.2 (PowerShell Desktop 5.1+) for Windows/Linux/macOS. Written in C# (cmdlets) with PowerShell scripts (build/test). Three projects: Cmdlets (SDK wrappers), Loader (assembly resolution), Module (manifest/docs).

## Build Instructions - VALIDATED AND WORKING

### Prerequisites
- .NET SDK 6.0+ (tested with 9.0.305)
- PowerShell 5.1+ or PowerShell 7+
- Pester module for testing

### Complete Build Sequence (Total time: ~30 seconds from clean)
```bash
# 1. Clean (takes 1-3 seconds)
dotnet clean

# 2. Build cmdlets (takes 13 seconds - includes restore on first build)
dotnet build 


### Testing Sequence

**⚠️ CRITICAL: Always use filtered tests for development iteration**
- Full test suite (396 tests) takes **5+ minutes** and may timeout
- Use filtered tests for **~20-30 seconds** runtime
- Only run full suite for final validation before commit

**RECOMMENDED: Run Filtered Tests (Fast - 20-30 seconds):**
```powershell
# 1. Set module path to built output (REQUIRED)
$env:TESTMODULEPATH = (Resolve-Path "Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0")

# 2. Install Pester if not present (first time only)
Install-Module -Force -Scope CurrentUser Pester -MinimumVersion 5.0

# 3. Run specific test groups using filters
$config = New-PesterConfiguration
$config.Run.Path = 'tests/All.Tests.ps1'  # MUST use All.Tests.ps1 for proper setup
$config.Run.PassThru = $true
$config.Output.Verbosity = 'Normal'  # Shows summary and failures
$config.Should.ErrorAction = 'Continue'

# Filter to specific test groups - examples:
$config.Filter.FullName = '*Remove-DataverseRecord - IfExists Flag*'
# $config.Filter.FullName = '*WhatIf*Confirm*'
# $config.Filter.FullName = '*Set-DataverseRecord*NoUpdateColumns*'
# $config.Filter.FullName = '*Get-DataverseRecord*FetchXml*'

$result = Invoke-Pester -Configuration $config

# Display summary
Write-Host ""
Write-Host "Test Summary:"
Write-Host "  Total:   $($result.TotalCount)"
Write-Host "  Passed:  $($result.PassedCount)"
Write-Host "  Failed:  $($result.FailedCount)"
Write-Host "  Skipped: $($result.SkippedCount)"

# Show failed test details if any
if ($result.FailedCount -gt 0) {
    Write-Host ""
    Write-Host "Failed Tests:"
    foreach ($test in $result.Failed) {
        Write-Host "  - $($test.ExpandedPath)"
        Write-Host "    $($test.ErrorRecord.Exception.Message)"
    }
    exit 1
}
```

**Full Test Suite (Slow - 5+ minutes, use sparingly):**
```powershell
# Set module path first (REQUIRED)
$env:TESTMODULEPATH = (Resolve-Path "Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0")

# Run ALL tests - takes 5+ minutes, use only for final validation
$config = New-PesterConfiguration
$config.Run.Path = 'tests/All.Tests.ps1'
$config.Run.PassThru = $true
$config.Output.Verbosity = 'Normal'
$config.Should.ErrorAction = 'Continue'

$result = Invoke-Pester -Configuration $config
```

**E2E Tests:**
```powershell
# E2E tests require real Dataverse environment credentials
$env:E2ETESTS_URL = "https://yourorg.crm.dynamics.com"
$env:E2ETESTS_CLIENTID = "your-client-id"
$env:E2ETESTS_CLIENTSECRET = "your-client-secret"
Invoke-Pester -Output Detailed -Path e2e-tests
```

**TEST NOTES:**
- Tests copy module to temp directory to avoid file locking issues
- Tests use FakeXrmEasy to mock Dataverse IOrganizationService
- `tests/contact.xml` contains serialized EntityMetadata for mock connection
- Tests spawn child PowerShell processes to test module loading
- ALWAYS set $env:TESTMODULEPATH before running tests
- **⚠️ ALWAYS use tests/All.Tests.ps1 as entry point** - individual test files will fail without setup
- **⚠️ Full test suite takes 5+ minutes** - use filtered tests during development

**TESTING REQUIREMENTS FOR CODE CHANGES:**
- **ALL code changes MUST include tests** that validate the new functionality
- **ALL tests MUST pass** before committing changes
- **Use filtered tests during development** to avoid timeouts (20-30 seconds vs 5+ minutes)
- Run tests using the Testing Sequence above after building
- For documentation changes with code examples:
  - Add tests in `tests/Cmdletname-maybeasuffix.ps1` that validate the example patterns
  - Test both the verbose and simplified syntax variants where applicable
  - Tests should use the mock provider with FakeXrmEasy
  - If an entity is not in `tests/contact.xml`, either:
    - Create test data in the test itself using SDK Entity objects
    - Or document that the example is tested manually/in E2E tests
- Expected test execution time: 20-30 seconds for filtered tests, 5+ minutes for full suite
- Tests may fail if entities beyond 'contact' are queried without creating them first
- **Document test results** in commits showing pass/fail counts
- CI/CD pipeline will run all tests - ensure local tests pass first

**COMMON TEST FILTERS:**
```powershell
# Filter examples for quick iteration:
$config.Filter.FullName = '*Remove-DataverseRecord*'      # All Remove-DataverseRecord tests
$config.Filter.FullName = '*IfExists*'                    # All IfExists flag tests
$config.Filter.FullName = '*WhatIf*Confirm*'              # All WhatIf/Confirm tests
$config.Filter.FullName = '*Set-DataverseRecord*NoUpdate*' # NoUpdate flag tests
$config.Filter.FullName = '*FetchXml*'                    # FetchXml query tests
$config.Filter.FullName = '*Get-DataverseRecord - Basic*' # Basic Get tests
```

## Project Architecture & Key Files

### Root Directory Files
- `Rnwood.Dataverse.Data.PowerShell.sln` - Solution with 3 projects
- `README.md` - 126 lines, examples of CRUD operations, installation, auth methods
- `.gitignore` - Standard .NET/VS ignore patterns
- `.github/workflows/publish.yml` - CI/CD: builds on Windows+Ubuntu × PS 5/7.4.11/latest, runs tests, publishes to Gallery on release
- `renovate.json` - Automated dependency updates

### Project 1: Rnwood.Dataverse.Data.PowerShell.Cmdlets/
**Purpose:** C# cmdlet implementations (multi-targeting net6.0;net462)  
**Key Files:**
- `Rnwood.Dataverse.Data.PowerShell.Cmdlets.csproj` - Multi-target project, CopyLocalLockFileAssemblies=true
- `Commands/GetDataverseConnectionCmdlet.cs` - Creates ServiceClient with 5 auth modes: Interactive, UsernamePassword, ClientSecret, DeviceCode, Mock
- `Commands/GetDataverseRecordCmdlet.cs` - Queries with QueryExpression/FetchXML, automatic paging, outputs PSObjects
- `Commands/SetDataverseRecordCmdlet.cs` - Create/update/upsert with batching (OrganizationServiceProxy.Execute with ExecuteMultipleRequest)
- `Commands/RemoveDataverseRecordCmdlet.cs` - Delete records with batching support
- `Commands/InvokeDataverseRequestCmdlet.cs` - Execute arbitrary OrganizationRequest
- `Commands/InvokeDataverseSqlCmdlet.cs` - SQL queries using MarkMpn.Sql4Cds.Engine
- `Commands/DataverseEntityConverter.cs` - 864 lines, critical type conversion:
  - `GetPSValue()` - Entity → PSObject (OptionSetValue → label, EntityReference → PSObject with Id/LogicalName/Name)
  - `GetDataverseValue()` - PSObject → Entity (string → EntityReference by name lookup, label → OptionSetValue)
  - `GetAllColumnNames()` - Lists attributes, excludes system columns unless includeSystemColumns=true
  - Handles dates with timezone conversion, Money, Guid, PartyList, MultiSelectPicklist, State/Status codes
- `Commands/OrganizationServiceCmdlet.cs` - Base class with Connection parameter
- `Commands/CustomLogicBypassableOrganizationServiceCmdlet.cs` - Base with BypassBusinessLogicExecutionStepIds

**Dependencies (from .csproj):**
- Microsoft.PowerPlatform.Dataverse.Client 1.2.3
- MarkMpn.Sql4Cds.Engine 10.1.0
- PowerShellStandard.Library 5.1.1
- System.ServiceModel.Primitives/Http 4.10.3
- FakeXrmEasy.v9 3.7.0 (net6.0) or 2.8.0 (net462)

### Project 2: Rnwood.Dataverse.Data.PowerShell.Loader/
**Purpose:** Assembly loading for both .NET runtimes (multi-targeting net6.0;net462)  
**Key Files:**
- `ModuleInitProvider.cs` - Implements IModuleAssemblyInitializer.OnImport()
  - For net6.0: Creates CmdletsLoadContext : AssemblyLoadContext, loads from cmdlets/net6.0/
  - For net462: Hooks AppDomain.AssemblyResolve, loads from cmdlets/net462/
  - Ensures Microsoft.Xrm.Sdk and other SDK assemblies load from correct location

### Project 3: Rnwood.Dataverse.Data.PowerShell/
**Purpose:** PowerShell module manifest and build orchestration (netstandard2.0)  
**Key Files:**
- `Rnwood.Dataverse.Data.PowerShell.psd1` - Module manifest, ModuleVersion 100.0.0 (replaced in CI from git tag)
- `Get-DataverseRecordsFolder.psm1` - Helper to read records from JSON files in folder
- `Set-DataverseRecordsFolder.psm1` - Helper to write records to JSON files in folder
- `docs/*.md` - 9 cmdlet documentation files (Get-DataverseConnection, Get-DataverseRecord, Set-DataverseRecord, Remove-DataverseRecord, Invoke-DataverseRequest, Invoke-DataverseSql, Get/Set-DataverseRecordsFolder, Get-DataverseWhoAmI)
- `updatehelp.ps1` - Runs Update-MarkdownHelpModule to refresh docs from cmdlet attributes (REQUIRES PowerShell Gallery access)
- `buildhelp.ps1` - Runs New-ExternalHelp to generate MAML XML from markdown to en-GB/
- `Rnwood.Dataverse.Data.PowerShell.csproj` - Build targets:
  - `BuildCmdlets` - Builds Cmdlets project, copies bin/$(Configuration)/** to $(OutDir)/cmdlets/
  - `BuildLoader` - Builds Loader project, copies bin/$(Configuration)/** to $(OutDir)/loader/
  - `BuildHelp` - Runs updatehelp.ps1 then buildhelp.ps1 (DependsOnTargets="BuildCmdlets", inputs/outputs for incremental build)

### Test Files
- `tests/All.Tests..ps1` - Shared setup: copies module to temp, sets PSModulePath, getMockConnection() using FakeXrmEasy with contact.xml metadata then includes all test files
- `tests/contact.xml` - 2.2MB serialized EntityMetadata for 'contact' entity (used by DataContractSerializer in tests)
- `tests/updatemetadata.ps1` - Script to regenerate contact.xml from real environment
- `e2e-tests/Module.Tests.ps1` - Connects to real Dataverse with client secret, queries systemuser table, runs SQL

## CI/CD Pipeline (.github/workflows/publish.yml)
**Matrix:** os=[ubuntu-latest, windows-latest] × powershell_version=['5', '7.4.11', 'latest'] × publish=[true/false once]  
**Steps:**
1. Checkout, install PowerShell version
2. Build: Sets version from $env:GITHUB_REF if tag, builds main project, copies to out/
3. Test (pwsh): Sets $env:TESTMODULEPATH, installs Pester, runs tests, checks $LASTEXITCODE
4. Test (powershell on Windows PS5): Same as above but with powershell.exe
5. E2E Test: Sets E2ETESTS_* env vars from secrets, runs e2e-tests
6. Publish (if matrix.publish && release): Runs Publish-Module to PowerShell Gallery

## Common Development Tasks

Always check help is up to date in cmdlets helpmessages and docs/*.MD files.

Use conventional commit messages and be specific about the error or improvement.

### Adding a New Cmdlet
1. Create `Rnwood.Dataverse.Data.PowerShell.Cmdlets/Commands/YourNewCmdlet.cs`
2. Inherit from PSCmdlet or OrganizationServiceCmdlet
3. Add `[Cmdlet(VerbsCommon.Get, "YourNoun")]` and `[OutputType(typeof(YourType))]`
4. Define parameters with `[Parameter(Mandatory=true, ParameterSetName="ByX")]`
5. Override ProcessRecord() or BeginProcessing()/EndProcessing() for pipeline
6. Build: `dotnet build -c Release ./Rnwood.Dataverse.Data.PowerShell.Cmdlets/Rnwood.Dataverse.Data.PowerShell.Cmdlets.csproj`
7. Run updatehelp.ps1 to generate markdown (or manually create docs/YourCmdlet.md)
8. Build main project to generate MAML help
9. Test with mock connection in tests/

### Modifying Type Conversion
- Edit `DataverseEntityConverter.cs`
- Add case to `GetPSValue()` for new AttributeType (e.g., case AttributeTypeCode.BigInt)
- Add logic to `GetDataverseValue()` for converting PSObject property to Dataverse value
- Handle metadata lookups (EntityMetadata, AttributeMetadata) via IOrganizationService.Execute(RetrieveEntityRequest)
- Test with FakeXrmEasy mock - add metadata to tests/contact.xml if needed

### Debugging Failed Tests
- Tests copy module to %TEMP%/[GUID]/Rnwood.Dataverse.Data.PowerShell
- Check $env:TESTMODULEPATH is set correctly
- Tests spawn child pwsh processes - add verbose output to see what's happening
- FakeXrmEasy limitations: doesn't support all operations, may need to mock additional metadata

## Coding Conventions

### C# Cmdlets
- Use `[Cmdlet(Verb, "Noun")]` with approved verbs (Get, Set, Remove, Invoke)
- Use `[Parameter(Mandatory=$bool, Position=$int, ValueFromPipeline=$bool, ParameterSetName=$string)]`
- Use `[ValidateNotNullOrEmpty]`, `[Alias("AliasName")]` on parameters
- Call `ShouldProcess(target, action)` before destructive operations when SupportsShouldProcess=true
- Use `WriteObject(obj, enumerateCollection)` not return - cmdlets don't return values
- Use `WriteVerbose()`, `WriteWarning()`, `WriteError()` for messages
- Parameter names: PascalCase (e.g., TableName, MatchOn)
- Multi-line lambda/LINQ: prefer explicit blocks for readability

### PowerShell Scripts
- Start with `$ErrorActionPreference = "Stop"`
- Use approved verbs (Get-Verb shows all)
- PascalCase for param names: `param([string]$ProjectDir)`
- Use pipeline: `Get-X | Where-Object { } | ForEach-Object { }`
- Wrap paths in quotes: `"$ProjectDir/file"`

## Key Behavioral Notes
- **Automatic Paging**: GetDataverseRecord fetches all pages automatically using PagingCookie
- **Batching**: SetDataverseRecord, RemoveDataverseRecord use ExecuteMultipleRequest for >1 record
- **Type Conversion**: Lookup fields accept name string (unique lookup), Id guid, or PSObject with LogicalName+Id
- **Choice Fields**: Accept label string or numeric value
- **System Columns**: Excluded by default (organizationid, createdby, modifiedby, ownerid, etc.) unless includeSystemColumns
- **Delegation**: Use -CallerId parameter to create/update on behalf of another user
- **Module Loading**: Loader ensures correct SDK assemblies from cmdlets/netX.0/ regardless of pre-loaded assemblies

## README Summary (126 lines)
- PowerShell module for Dataverse (Dynamics 365, Power Apps) data manipulation
- Features: CRUD, M:M records, batching, paging, delegation, type conversion, multiple auth methods
- Installation: `Install-Module Rnwood.Dataverse.Data.PowerShell -Scope CurrentUser` (requires RemoteSigned execution policy)
- Quick start: `$c = Get-DataverseConnection -url https://org.crm.dynamics.com -interactive; Get-DataverseRecord -connection $c -tablename contact`
- Main cmdlets: Get-DataverseConnection, Get-DataverseRecord, Set-DataverseRecord, Remove-DataverseRecord, Invoke-DataverseRequest, Invoke-DataverseSql
- Does NOT support on-premise Dataverse
