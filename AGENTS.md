# Rnwood.Dataverse.Data.PowerShell - AI Agent Development Guide

## Repository Overview
Cross-platform PowerShell module for Microsoft Dataverse data manipulation. Targets .NET 8.0 (PowerShell Core 7+) and .NET Framework 4.6.2 (PowerShell Desktop 5.1+) for Windows/Linux/macOS. Written in C# (cmdlets) with PowerShell scripts (build/test). Core module includes Cmdlets (SDK wrappers), Loader (assembly resolution), and Module (manifest/docs) projects, plus xUnit test projects and XrmToolbox plugin projects.

## Critical Development Requirements

ALWAYS clean up temporary files in the repo. Don't generate reports as files unless asked for.

### 🔴 MANDATORY: Use the Real Dataverse Environment

**Before making ANY code changes or adding features:**
1. **EXPLORE** the existing Dataverse environment to understand current entities, attributes, and behaviors
2. **TEST** your approach manually using the module cmdlets against the real environment
3. **VALIDATE** edge cases and error scenarios in the live environment

**Environment Credentials:**
- Look for `DATAVERSE_DEV_URL`, `DATAVERSE_DEV_CLIENTID`, `DATAVERSE_DEV_CLIENTSECRET` environment variables
- If not set, check for a `.env` file in the repository root
- If `.env` doesn't exist, create it with the user's help (contains no sensitive data)

**Example exploration script:**
```powershell
# Load environment variables from .env if needed
if (Test-Path .env) {
    Get-Content .env | ForEach-Object {
        if ($_ -match '^\s*([^#][^=]+?)\s*=\s*(.+)\s*$') {
            [Environment]::SetEnvironmentVariable($matches[1], $matches[2])
        }
    }
}

# Connect to development environment
$conn = Get-DataverseConnection `
    -Url $env:DATAVERSE_DEV_URL `
    -ClientId $env:DATAVERSE_DEV_CLIENTID `
    -ClientSecret $env:DATAVERSE_DEV_CLIENTSECRET

# Explore existing data before making changes
Get-DataverseRecord -Connection $conn -TableName account -Top 5
```

### 🔴 MANDATORY: Testing Requirements

**ALL code changes MUST include comprehensive tests:**

1. **xUnit Infrastructure Tests** (Rnwood.Dataverse.Data.PowerShell.Tests/)
   - Test cmdlet logic, type conversion, error handling
   - Use FakeXrmEasy to mock IOrganizationService
   - Fast execution (1-2 minutes)
   - Follow existing test patterns in the test project

2. **xUnit E2E Tests** (Rnwood.Dataverse.Data.PowerShell.E2ETests/)
   - Test actual cmdlet behavior against real Dataverse environment
   - Use PowerShellProcessRunner to spawn child PowerShell processes
   - Follow existing test patterns (see examples below)
   - **PREFER running targeted tests** rather than the full suite (saves 10+ minutes)

**Running Targeted E2E Tests:**
```powershell
# Set up environment for E2E tests
$env:E2ETESTS_URL = $env:DATAVERSE_DEV_URL
$env:E2ETESTS_CLIENTID = $env:DATAVERSE_DEV_CLIENTID
$env:E2ETESTS_CLIENTSECRET = $env:DATAVERSE_DEV_CLIENTSECRET
$env:TESTMODULEPATH = (Resolve-Path "Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0")

# Run specific test class
dotnet test ./Rnwood.Dataverse.Data.PowerShell.E2ETests/Rnwood.Dataverse.Data.PowerShell.E2ETests.csproj `
    -f net8.0 `
    --filter "FullyQualifiedName~YourTestClassName" `
    --logger "console;verbosity=normal"

# Run specific test method
dotnet test ./Rnwood.Dataverse.Data.PowerShell.E2ETests/Rnwood.Dataverse.Data.PowerShell.E2ETests.csproj `
    -f net8.0 `
    --filter "FullyQualifiedName~YourTestClassName.YourTestMethodName" `
    --logger "console;verbosity=normal"
```

**Example E2E Test Pattern:**
```csharp
[Fact]
public async Task YourNewFeature_Should_WorkCorrectly()
{
    
    var testScript = GetConnectionScript($@"
        # Your PowerShell test code here
        $result = Get-DataverseRecord -TableName account -Top 1
        $result | ConvertTo-Json -Depth 10
    ");
    
    var runner = new PowerShellProcessRunner();
    var result = await runner.ExecuteAsync(testScript);
    
    Assert.Equal(0, result.ExitCode);
    // Add your assertions here
}
```

**Test Execution Requirements:**
- All infrastructure tests MUST pass before committing
- All E2E tests MUST pass before committing
- Document test results in commit messages (pass/fail counts)
- CI/CD pipeline will run all tests automatically

## Prerequisites and Dependencies

**Required Software:**
- .NET SDK 8.0 or later (tested with 9.0.305)
- PowerShell 5.1+ (Windows PowerShell) or PowerShell 7+ (PowerShell Core)
- Git

**Package Dependencies:**
- xUnit (testing framework)
- FakeXrmEasy (Dataverse mocking for tests)
- Microsoft.PowerPlatform.Dataverse.Client
- MarkMpn.Sql4Cds.Engine
- PowerShellStandard.Library
- PlatyPS (for help generation)

**Note:** Pester is NOT used in this project. All tests use xUnit.

## Build Instructions

### Standard Build

```powershell
# Clean build
dotnet clean

# Build entire solution (Debug configuration)
dotnet build

# Build specific configuration
dotnet build -c Release

# Build takes approximately 30-60 seconds from clean
```

### Build Verification

After building, verify the output structure:
```powershell
# Check built module location
Get-ChildItem Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0/

# Verify manifest exists
Test-Path Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0/Rnwood.Dataverse.Data.PowerShell.psd1
```

## Testing Instructions

### Infrastructure Tests (xUnit)

Fast C# tests that validate cmdlet infrastructure and internal classes using FakeXrmEasy mocks.

```powershell
# Set module path for tests (REQUIRED)
$env:TESTMODULEPATH = (Resolve-Path "Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0")

# Run infrastructure tests for .NET 8.0 (all platforms)
dotnet test ./Rnwood.Dataverse.Data.PowerShell.Tests/Rnwood.Dataverse.Data.PowerShell.Tests.csproj `
    -f net8.0 `
    --logger "console;verbosity=normal"

# Run infrastructure tests for .NET Framework 4.6.2 (Windows only)
dotnet test ./Rnwood.Dataverse.Data.PowerShell.Tests/Rnwood.Dataverse.Data.PowerShell.Tests.csproj `
    -f net462 `
    --logger "console;verbosity=normal"
```

**Execution time:** 1-2 minutes

### E2E Tests (xUnit)

Tests that run against real Dataverse environments using PowerShellProcessRunner to execute PowerShell scripts in child processes.

```powershell
# Set up E2E test environment (REQUIRED)
$env:E2ETESTS_URL = "https://yourorg.crm.dynamics.com"
$env:E2ETESTS_CLIENTID = "your-client-id"
$env:E2ETESTS_CLIENTSECRET = "your-client-secret"
$env:TESTMODULEPATH = (Resolve-Path "Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0")

# Run all E2E tests (takes 10+ minutes - use targeted tests instead!)
dotnet test ./Rnwood.Dataverse.Data.PowerShell.E2ETests/Rnwood.Dataverse.Data.PowerShell.E2ETests.csproj `
    -f net8.0 `
    --logger "console;verbosity=normal"

# PREFERRED: Run targeted E2E tests by test class
dotnet test ./Rnwood.Dataverse.Data.PowerShell.E2ETests/Rnwood.Dataverse.Data.PowerShell.E2ETests.csproj `
    -f net8.0 `
    --filter "FullyQualifiedName~ModuleBasicTests" `
    --logger "console;verbosity=normal"

# PREFERRED: Run specific test method
dotnet test ./Rnwood.Dataverse.Data.PowerShell.E2ETests/Rnwood.Dataverse.Data.PowerShell.E2ETests.csproj `
    -f net8.0 `
    --filter "FullyQualifiedName~ModuleBasicTests.ConnectionWithClientSecret_Should_ConnectSuccessfully" `
    --logger "console;verbosity=normal"
```

**Execution time:** 
- Full suite: 10+ minutes (avoid unless necessary)
- Targeted tests: 10-60 seconds per test

**Important Notes:**
- ALWAYS set `$env:TESTMODULEPATH` before running tests
- E2E tests spawn child PowerShell processes to test module loading
- Use targeted test execution to save time during development

## Project Architecture & Key Files

### Solution Structure

The solution contains 8 projects:

1. **Rnwood.Dataverse.Data.PowerShell** - Main module (manifest, docs, orchestration)
2. **Rnwood.Dataverse.Data.PowerShell.Cmdlets** - C# cmdlet implementations
3. **Rnwood.Dataverse.Data.PowerShell.Loader** - Assembly loading for both .NET runtimes
4. **Rnwood.Dataverse.Data.PowerShell.Tests** - xUnit infrastructure tests
5. **Rnwood.Dataverse.Data.PowerShell.E2ETests** - xUnit E2E tests
6. **Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin** - XrmToolbox plugin
7. **Rnwood.Dataverse.Data.PowerShell.XrmToolboxPluginLoader** - Plugin loader
8. **Rnwood.Dataverse.Data.PowerShell.XrmToolboxPluginHost** - Plugin host

### Root Directory Files

- `Rnwood.Dataverse.Data.PowerShell.sln` - Visual Studio solution file
- `README.md` - User-facing documentation with examples, installation, auth methods
- `CONTRIBUTING.md` - Contribution guidelines
- `LICENSE` - MIT license
- `.gitignore` - Standard .NET/VS ignore patterns
- `.github/workflows/publish.yml` - CI/CD workflow
- `renovate.json` - Automated dependency updates configuration
- `NuGet.Config` - Configures nuget.org as package source

### Project 1: Rnwood.Dataverse.Data.PowerShell/

**Purpose:** PowerShell module manifest and build orchestration (netstandard2.0)

**Key Files:**
- `Rnwood.Dataverse.Data.PowerShell.psd1` - Module manifest (ModuleVersion 100.0.0 replaced in CI)
- `Get-DataverseRecordsFolder.psm1` - Helper to read records from JSON files
- `Set-DataverseRecordsFolder.psm1` - Helper to write records to JSON files
- `docs/*.md` - Cmdlet documentation files generated by PlatyPS
- `updatehelp.ps1` - Runs Update-MarkdownHelpModule to refresh docs from cmdlet attributes
- `buildhelp.ps1` - Runs New-ExternalHelp to generate MAML XML from markdown
- `Rnwood.Dataverse.Data.PowerShell.csproj` - Build targets for orchestrating cmdlets, loader, and help builds

### Project 2: Rnwood.Dataverse.Data.PowerShell.Cmdlets/

**Purpose:** C# cmdlet implementations (multi-targeting net8.0;net462)

**Key Files:**
- `Commands/GetDataverseConnectionCmdlet.cs` - Creates ServiceClient with auth modes: Interactive, UsernamePassword, ClientSecret, DeviceCode, Mock
- `Commands/GetDataverseRecordCmdlet.cs` - Queries with QueryExpression/FetchXML, automatic paging
- `Commands/SetDataverseRecordCmdlet.cs` - Create/update/upsert with batching
- `Commands/RemoveDataverseRecordCmdlet.cs` - Delete records with batching
- `Commands/InvokeDataverseRequestCmdlet.cs` - Execute arbitrary OrganizationRequest
- `Commands/InvokeDataverseSqlCmdlet.cs` - SQL queries using MarkMpn.Sql4Cds.Engine
- `Commands/DataverseEntityConverter.cs` - Critical type conversion between Dataverse and PowerShell:
  - `GetPSValue()` - Entity → PSObject (OptionSetValue → label, EntityReference → PSObject)
  - `GetDataverseValue()` - PSObject → Entity (string → EntityReference by name lookup)
  - `GetAllColumnNames()` - Lists attributes, excludes system columns by default
  - Handles dates, timezone conversion, Money, Guid, PartyList, MultiSelectPicklist, State/Status codes
- `Commands/OrganizationServiceCmdlet.cs` - Base class with Connection parameter
- `Commands/CustomLogicBypassableOrganizationServiceCmdlet.cs` - Base with BypassBusinessLogicExecutionStepIds

**Dependencies:**
- Microsoft.PowerPlatform.Dataverse.Client
- MarkMpn.Sql4Cds.Engine
- PowerShellStandard.Library
- System.ServiceModel.Primitives/Http
- FakeXrmEasy.v9 (net8.0) or FakeXrmEasy (net462)

### Project 3: Rnwood.Dataverse.Data.PowerShell.Loader/

**Purpose:** Assembly loading for both .NET runtimes (multi-targeting net8.0;net462)

**Key Files:**
- `ModuleInitProvider.cs` - Implements IModuleAssemblyInitializer.OnImport()
  - For net8.0: Creates CmdletsLoadContext : AssemblyLoadContext, loads from cmdlets/net8.0/
  - For net462: Hooks AppDomain.AssemblyResolve, loads from cmdlets/net462/
  - Ensures correct SDK assemblies load regardless of pre-loaded assemblies

### Project 4: Rnwood.Dataverse.Data.PowerShell.Tests/

**Purpose:** xUnit infrastructure tests (multi-targeting net8.0;net462)

**Key Files:**
- `Rnwood.Dataverse.Data.PowerShell.Tests.csproj` - xUnit test project with FakeXrmEasy
- `Infrastructure/TestBase.cs` - Base class with CreateMockConnection(), LoadEntityMetadata()
- `Infrastructure/PowerShellProcessRunner.cs` - Helper for running PowerShell scripts in child processes
- `Infrastructure/CmdletInvoker.cs` - Documents why direct cmdlet invocation doesn't work in xUnit
- `Cmdlets/GetDataverseRecordBasicTests.cs` - Example infrastructure tests

**Testing Notes:**
- Uses FakeXrmEasy to mock IOrganizationService
- Tests cmdlet implementation details, type conversion, error handling
- Does NOT support direct PowerShell cmdlet invocation (Microsoft.PowerShell.SDK is reference-only)
- Metadata loaded from embedded XML files

### Project 5: Rnwood.Dataverse.Data.PowerShell.E2ETests/

**Purpose:** xUnit E2E tests against real Dataverse (multi-targeting net8.0;net462)

**Key Files:**
- `Rnwood.Dataverse.Data.PowerShell.E2ETests.csproj` - xUnit E2E test project
- `Infrastructure/E2ETestBase.cs` - Base class with E2ETESTS_URL/CLIENTID/CLIENTSECRET, GetConnectionScript()
- `Module/ModuleBasicTests.cs` - Basic module functionality tests
- `Sql/InvokeDataverseSqlTests.cs` - SQL cmdlet tests
- `Views/ViewManipulationTests.cs` - View manipulation tests
- `Plugin/PluginManagementTests.cs` - Plugin management tests
- `Forms/FormLibraryAndEventHandlerTests.cs` - Form library and event handler tests
- `Solution/SolutionComponentTests.cs` - Solution component tests
- `Metadata/*Tests.cs` - Entity, attribute, relationship metadata tests

**Testing Notes:**
- Uses PowerShellProcessRunner to execute PowerShell scripts in child processes
- Tests actual cmdlet behavior against real Dataverse instances
- Requires E2ETESTS_URL, E2ETESTS_CLIENTID, E2ETESTS_CLIENTSECRET environment variables

### Projects 6-8: XrmToolbox Plugin Projects

**Purpose:** XrmToolbox integration for easier getting started experience

See [Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin/README.md](Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin/README.md) for details.

## CI/CD Pipeline

**Workflow:** `.github/workflows/publish.yml`

**Build Matrix:**
- Operating Systems: `ubuntu-latest`, `windows-latest`
- PowerShell Versions: `5`, `7.4.11`, `latest`

**Pipeline Steps:**

1. **Checkout** - Fetches code with full history for tags
2. **Build** - Determines version, builds projects:
   - Release builds: Use git tag version (e.g., `v1.5.0` → `1.5.0`)
   - CI builds: Auto-increment based on conventional commits (e.g., `1.5.0-ci20241102001`)
3. **Infrastructure Tests** - Runs xUnit infrastructure tests:
   - net8.0 on all platforms
   - net462 on Windows only
4. **E2E Tests** - Runs xUnit E2E tests against real Dataverse:
   - Uses secrets: `E2ETESTS_URL`, `E2ETESTS_CLIENTID`, `E2ETESTS_CLIENTSECRET`
   - Full suite runs in CI (takes 10+ minutes)
5. **Publish** - Publishes to PowerShell Gallery on release

**Timeout:** 60 minutes (fails fast if exceeded)

## Versioning Strategy

Uses **Conventional Commits** for automatic version determination.

### Version Determination

- **Release builds** (git tags): Use tag version (e.g., `v1.5.0` → `1.5.0`)
- **CI builds** (main/PRs): Auto-increment based on conventional commits (e.g., `1.5.0-ci20241102001`)

### Conventional Commit Format

PR titles MUST use: `<type>(<scope>): <description>`

**Version Bumps:**
- `feat:` or `feat(<scope>):` → **Minor** (1.4.0 → 1.5.0)
- `fix:` or `fix(<scope>):` → **Patch** (1.4.0 → 1.4.1)
- `feat!:`, `fix!:`, or `BREAKING CHANGE:` → **Major** (1.4.0 → 2.0.0)
- Other types (`docs:`, `chore:`, `style:`, `refactor:`, `perf:`, `test:`, `build:`, `ci:`) → **Patch**

**Examples:**
```
feat: add batch delete operation
fix: resolve connection timeout issue
feat!: remove deprecated parameters
fix(auth): handle expired tokens correctly
docs: update installation instructions
```

**Tools:**
- `.github/pull_request_template.md` - PR template with format instructions
- `scripts/Get-NextVersion.ps1` - Version calculation script

## Common Development Tasks

### Adding a New Cmdlet

1. Create `Rnwood.Dataverse.Data.PowerShell.Cmdlets/Commands/YourNewCmdlet.cs`
2. Inherit from `PSCmdlet` or `OrganizationServiceCmdlet`
3. Add `[Cmdlet(VerbsCommon.Get, "YourNoun")]` and `[OutputType(typeof(YourType))]`
4. Define parameters with `[Parameter(...)]` attributes
5. Override `ProcessRecord()` or `BeginProcessing()`/`EndProcessing()` for pipeline support
6. **TEST in real Dataverse environment first** before writing tests
7. Add xUnit infrastructure tests in `Rnwood.Dataverse.Data.PowerShell.Tests/`
8. Add xUnit E2E tests in `Rnwood.Dataverse.Data.PowerShell.E2ETests/`
9. Build: `dotnet build`
10. Run `updatehelp.ps1` to generate/update markdown documentation
11. Run `buildhelp.ps1` to generate MAML help XML
12. Verify all tests pass

### Modifying Type Conversion

1. **TEST current behavior** in real Dataverse environment
2. Edit `Commands/DataverseEntityConverter.cs`
3. Add case to `GetPSValue()` for new AttributeType
4. Add logic to `GetDataverseValue()` for converting PSObject → Entity
5. Handle metadata lookups via `IOrganizationService.Execute(RetrieveEntityRequest)`
6. **TEST changes** in real Dataverse environment
7. Add xUnit infrastructure tests to validate type conversion
8. Add xUnit E2E tests to verify real-world scenarios
9. Verify all tests pass

### Updating Documentation

1. Modify cmdlet help attributes in C# code (`[Parameter(HelpMessage = "...")]`)
2. Run `updatehelp.ps1` to sync markdown with cmdlet attributes
3. Manually edit `docs/*.md` if needed for examples/notes
4. Run `buildhelp.ps1` to generate MAML XML
5. Verify help displays correctly: `Get-Help YourCmdlet -Full`

### Debugging Failed Tests

**Infrastructure Tests:**
- Verify `$env:TESTMODULEPATH` is set correctly
- Check FakeXrmEasy mock setup in test
- Review entity metadata in test project (embedded XML files)
- FakeXrmEasy has limitations - may need to adjust test approach

**E2E Tests:**
- Verify credentials are set: `$env:E2ETESTS_URL`, `$env:E2ETESTS_CLIENTID`, `$env:E2ETESTS_CLIENTSECRET`
- E2E tests spawn child PowerShell processes - check process output
- Add verbose output to PowerShell scripts in tests
- Run targeted test to isolate issue
- Check if Dataverse environment has required data/permissions

## Coding Conventions

### General Principles

- **DO NOT** create summary documents unless requested
- **ALWAYS** use conventional commit format in PR titles
- **ALWAYS** update help (cmdlet attributes and docs/*.md)
- **ALWAYS** test in real Dataverse environment before writing tests

### C# Cmdlets

- Use `[Cmdlet(Verb, "Noun")]` with approved verbs (Get, Set, Remove, Invoke)
- Use `[Parameter(Mandatory=..., Position=..., ValueFromPipeline=..., ParameterSetName=...)]`
- Use `[ValidateNotNullOrEmpty]`, `[Alias("...")]` on parameters as appropriate
- Call `ShouldProcess(target, action)` before destructive operations when `SupportsShouldProcess=true`
- Use `WriteObject(obj, enumerateCollection)` not `return` - cmdlets don't return values
- Use `WriteVerbose()`, `WriteWarning()`, `WriteError()` for messages
- Parameter names: PascalCase (e.g., `TableName`, `MatchOn`)
- Multi-line lambda/LINQ: prefer explicit blocks for readability
- **DO NOT** use try/catch unless handling specific exceptions with Hresult checking
- **DO NOT** catch and only log warnings - let exceptions bubble up unless specific handling needed

### PowerShell Scripts

- Start with `$ErrorActionPreference = "Stop"`
- Use approved verbs (`Get-Verb` shows all)
- PascalCase for param names: `param([string]$ProjectDir)`
- Use pipeline: `Get-X | Where-Object { } | ForEach-Object { }`
- Wrap paths in quotes: `"$ProjectDir/file"`

### Cmdlet Help Files

- Create help file for each cmdlet in `docs/`
- Follow PlatyPS schema: `.SYNOPSIS`, `.DESCRIPTION`, `.PARAMETER`, `.EXAMPLE`, `.NOTES`
- Run `updatehelp.ps1` to sync with cmdlet attributes before manual editing
- Run `updatehelp.ps1` after parameter changes to validate
- Include practical examples in `.EXAMPLE` sections

### xUnit Test Files

- Inherit from `TestBase` (infrastructure tests) or `E2ETestBase` (E2E tests)
- Use descriptive test names: `FeatureName_Should_BehaviorExpected`
- Follow existing test patterns in the test projects
- Infrastructure tests: Use FakeXrmEasy for mocking
- E2E tests: Use PowerShellProcessRunner for executing PowerShell scripts

## Key Behavioral Notes

- **Automatic Paging**: `Get-DataverseRecord` fetches all pages automatically using PagingCookie
- **Batching**: `Set-DataverseRecord`, `Remove-DataverseRecord` use ExecuteMultipleRequest for >1 record
- **Type Conversion**: 
  - Lookup fields accept name string (unique lookup), Id guid, or PSObject with LogicalName+Id
  - Choice fields accept label string or numeric value
- **System Columns**: Excluded by default (organizationid, createdby, modifiedby, ownerid, etc.) unless `includeSystemColumns=true`
- **Delegation**: Use `-CallerId` parameter to create/update on behalf of another user
- **Module Loading**: Loader ensures correct SDK assemblies load from cmdlets/netX.0/ regardless of pre-loaded assemblies

## Quick Reference

### Build Commands

```powershell
# Clean and build
dotnet clean
dotnet build

# Build release
dotnet build -c Release
```

### Test Commands

```powershell
# Set module path (required for all tests)
$env:TESTMODULEPATH = (Resolve-Path "Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0")

# Infrastructure tests
dotnet test ./Rnwood.Dataverse.Data.PowerShell.Tests/Rnwood.Dataverse.Data.PowerShell.Tests.csproj -f net8.0

# E2E tests (set credentials first)
$env:E2ETESTS_URL = $env:DATAVERSE_DEV_URL
$env:E2ETESTS_CLIENTID = $env:DATAVERSE_DEV_CLIENTID
$env:E2ETESTS_CLIENTSECRET = $env:DATAVERSE_DEV_CLIENTSECRET

# Run targeted E2E test
dotnet test ./Rnwood.Dataverse.Data.PowerShell.E2ETests/Rnwood.Dataverse.Data.PowerShell.E2ETests.csproj `
    -f net8.0 `
    --filter "FullyQualifiedName~YourTestClass.YourTestMethod"
```

### Help Commands

```powershell
# Update markdown help from cmdlet attributes
./updatehelp.ps1

# Build MAML XML from markdown
./buildhelp.ps1
```

### Environment Setup Commands

```powershell
# Load .env file
if (Test-Path .env) {
    Get-Content .env | ForEach-Object {
        if ($_ -match '^\s*([^#][^=]+?)\s*=\s*(.+)\s*$') {
            [Environment]::SetEnvironmentVariable($matches[1], $matches[2])
        }
    }
}

# Connect to dev environment
$conn = Get-DataverseConnection `
    -Url $env:DATAVERSE_DEV_URL `
    -ClientId $env:DATAVERSE_DEV_CLIENTID `
    -ClientSecret $env:DATAVERSE_DEV_CLIENTSECRET
```
