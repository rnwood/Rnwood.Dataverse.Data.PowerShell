# End-to-End Tests (Legacy Directory)

**NOTE:** The PowerShell-based Pester E2E tests that were previously in this directory have been migrated to xUnit C# tests and **removed**. This directory is retained for historical reference but no longer contains active test files.

## Current E2E Testing

All E2E tests are now located in **`Rnwood.Dataverse.Data.PowerShell.E2ETests/`** as xUnit C# tests. These tests run against real Dataverse environments using PowerShellProcessRunner to execute PowerShell scripts in child processes.

### Why the Migration?

The project migrated from PowerShell Pester tests to xUnit C# tests for:
- **Better integration** with CI/CD pipelines
- **Consistent testing framework** (both infrastructure and E2E tests use xUnit)
- **Improved reliability** with PowerShellProcessRunner for module loading tests
- **Better IDE support** for debugging and test execution
- **Faster test discovery** and execution

## Running E2E Tests

### Prerequisites

1. Access to a Dataverse environment
2. Application registration with client ID and client secret
3. Appropriate permissions in the Dataverse environment

### Environment Setup

Set the following environment variables before running E2E tests:

```powershell
$env:E2ETESTS_URL = "https://yourorg.crm.dynamics.com"
$env:E2ETESTS_CLIENTID = "your-client-id"
$env:E2ETESTS_CLIENTSECRET = "your-client-secret"
$env:TESTMODULEPATH = (Resolve-Path "Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0")
```

### Run All E2E Tests

```powershell
# From repository root (WARNING: Takes 10+ minutes)
dotnet test ./Rnwood.Dataverse.Data.PowerShell.E2ETests/Rnwood.Dataverse.Data.PowerShell.E2ETests.csproj `
    -f net8.0 `
    --logger "console;verbosity=normal"
```

### Run Targeted E2E Tests (Recommended)

Running targeted tests is **much faster** than running the full suite:

```powershell
# Run specific test class (10-60 seconds)
dotnet test ./Rnwood.Dataverse.Data.PowerShell.E2ETests/Rnwood.Dataverse.Data.PowerShell.E2ETests.csproj `
    -f net8.0 `
    --filter "FullyQualifiedName~ModuleBasicTests" `
    --logger "console;verbosity=normal"

# Run specific test method
dotnet test ./Rnwood.Dataverse.Data.PowerShell.E2ETests/Rnwood.Dataverse.Data.PowerShell.E2ETests.csproj `
    -f net8.0 `
    --filter "FullyQualifiedName~ModuleBasicTests.ConnectionWithClientSecret_Should_ConnectSuccessfully" `
    --logger "console;verbosity=normal"
```

### Available Test Classes

E2E tests are organized by feature area:

- **Module/ModuleBasicTests.cs** - Basic module functionality, connection, queries
- **Sql/InvokeDataverseSqlTests.cs** - SQL query cmdlet tests
- **Views/ViewManipulationTests.cs** - View CRUD operations
- **Forms/FormLibraryAndEventHandlerTests.cs** - Form customization tests
- **Forms/FormManipulationTests.cs** - Form structure modification tests
- **Plugin/PluginManagementTests.cs** - Plugin management tests
- **Metadata/** - Entity, attribute, relationship metadata tests
- **Solution/SolutionComponentTests.cs** - Solution component tests
- **EnvironmentVariable/** - Environment variable tests
- **FileData/** - File data tests
- **OrganizationSettings/** - Organization settings tests
- **RecordAccess/** - Record access/sharing tests
- **Sitemap/** - Sitemap tests

## Troubleshooting

### Tests are Skipped

E2E tests are automatically skipped if environment variables are not set. Verify:
```powershell
$env:E2ETESTS_URL
$env:E2ETESTS_CLIENTID
$env:E2ETESTS_CLIENTSECRET
$env:TESTMODULEPATH
```

### Authentication Errors

- Verify client ID and client secret are correct
- Ensure the app registration has appropriate permissions in Dataverse
- Check that the URL is correct and accessible

### Module Not Found

Ensure the module is built before running tests:
```powershell
dotnet build -c Debug
$env:TESTMODULEPATH = (Resolve-Path "Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0")
```

## See Also

- **AGENTS.md** - Comprehensive development guide, test patterns, and best practices
- **Rnwood.Dataverse.Data.PowerShell.E2ETests/** - Current E2E test source code
- **Rnwood.Dataverse.Data.PowerShell.Tests/** - Infrastructure tests (faster, mock-based)
