# Rnwood.Dataverse.Data.PowerShell - GitHub Copilot Instructions

## Project Overview

This is a cross-platform PowerShell module for connecting to Microsoft Dataverse (used by Dynamics 365 and Power Apps) and manipulating data. The module works on both PowerShell Desktop (.NET Framework) and PowerShell Core (.NET 6+), enabling cross-platform support on Windows, Linux, and macOS.

## Architecture

The project consists of three main components:

1. **Rnwood.Dataverse.Data.PowerShell.Cmdlets** (`Rnwood.Dataverse.Data.PowerShell.Cmdlets/`)
   - C# project containing the actual PowerShell cmdlet implementations
   - Targets both .NET 6.0 (for PowerShell Core) and .NET Framework 4.6.2 (for PowerShell Desktop)
   - Main cmdlet classes in `Commands/` directory:
     - `GetDataverseConnectionCmdlet.cs` - Creates connections to Dataverse environments
     - `GetDataverseRecordCmdlet.cs` - Queries records from Dataverse
     - `SetDataverseRecordCmdlet.cs` - Creates/updates records in Dataverse
     - `RemoveDataverseRecordCmdlet.cs` - Deletes records from Dataverse
     - `InvokeDataverseRequestCmdlet.cs` - Executes arbitrary Dataverse API requests
     - `InvokeDataverseSqlCmdlet.cs` - Executes SQL queries against Dataverse using Sql4Cds
   - `DataverseEntityConverter.cs` - Handles conversion between PowerShell objects and Dataverse entities

2. **Rnwood.Dataverse.Data.PowerShell.Loader** (`Rnwood.Dataverse.Data.PowerShell.Loader/`)
   - Module loader that handles assembly resolution for both .NET Framework and .NET Core
   - Implements `IModuleAssemblyInitializer` to set up assembly loading context
   - Ensures correct SDK assemblies are loaded when the module is imported

3. **Rnwood.Dataverse.Data.PowerShell** (`Rnwood.Dataverse.Data.PowerShell/`)
   - PowerShell module manifest (`.psd1`) and documentation
   - `docs/` directory contains markdown documentation for each cmdlet
   - Helper scripts: `buildhelp.ps1` and `updatehelp.ps1` for generating help documentation

## Build Instructions

### Prerequisites
- .NET SDK 6.0 or later
- PowerShell 5.1+ (for Windows) or PowerShell 7+ (for cross-platform)

### Building the Project

```bash
# Build the Release configuration
dotnet build -c Release ./Rnwood.Dataverse.Data.PowerShell/Rnwood.Dataverse.Data.PowerShell.csproj
```

The output will be in:
- `Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0/` - Module files
- `Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0/cmdlets/net6.0/` - .NET 6.0 cmdlet assemblies
- `Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0/cmdlets/net462/` - .NET Framework cmdlet assemblies

### Development Build

```bash
# Build Debug configuration for development
dotnet build -c Debug
```

## Testing

The project uses Pester for testing:

```powershell
# Install Pester if needed
Install-Module -Force Pester

# Run unit tests
$env:TESTMODULEPATH = "path/to/out/Rnwood.Dataverse.Data.PowerShell"
Invoke-Pester -Output Detailed -Path tests

# Run E2E tests (requires Dataverse environment credentials)
Invoke-Pester -Output Detailed -Path e2e-tests
```

Test files:
- `tests/Module.Tests.ps1` - Module loading and basic functionality tests
- `tests/Get-DataverseRecord.Tests.ps1` - Tests for record retrieval cmdlet
- `e2e-tests/Module.Tests.ps1` - End-to-end tests against a real Dataverse environment

## Coding Conventions

### C# Code
- Follow standard C# naming conventions
- Cmdlet classes inherit from `PSCmdlet` or appropriate base classes
- Use `[Cmdlet]` attribute with proper verb-noun naming (e.g., `Get-DataverseConnection`)
- Parameter sets are used to handle different authentication and operation modes
- All cmdlets should support PowerShell conventions (`-WhatIf`, `-Confirm`, `-Verbose` where appropriate)

### PowerShell Code
- Use approved PowerShell verbs (Get, Set, Remove, Invoke, etc.)
- Parameter names use PascalCase
- Follow PowerShell best practices for pipeline support
- Scripts should set `$ErrorActionPreference = "Stop"` for proper error handling

### Type Conversion
- The `DataverseEntityConverter` class handles bidirectional conversion between:
  - PowerShell objects and Dataverse `Entity` objects
  - Simple types (strings, numbers, dates) and Dataverse attribute types
  - Lookup values (by name or ID) and `EntityReference` objects
  - Choice values (by label or value) and `OptionSetValue` objects
- System columns are excluded by default but can be included with parameters

## Key Dependencies

- **Microsoft.PowerPlatform.Dataverse.Client** (v1.2.3) - Main Dataverse client SDK
- **Microsoft.Identity.Client** (v4.67.0) - Authentication (MSAL)
- **MarkMpn.Sql4Cds.Engine** (v10.1.0) - SQL query support for Dataverse
- **FakeXrmEasy** (v3.7.0) - Used for unit testing with fake Dataverse context

## Documentation

- Documentation is generated using PlatyPS from the cmdlet help attributes
- Markdown files in `Rnwood.Dataverse.Data.PowerShell/docs/` contain the cmdlet documentation
- Run `updatehelp.ps1` to update markdown documentation from cmdlet changes
- Run `buildhelp.ps1` to generate external help files (MAML XML) from markdown

## Common Development Tasks

### Adding a New Cmdlet

1. Create a new C# class in `Rnwood.Dataverse.Data.PowerShell.Cmdlets/Commands/`
2. Inherit from `PSCmdlet` or appropriate base class (e.g., `OrganizationServiceCmdlet`)
3. Add `[Cmdlet]` attribute with approved verb and noun
4. Implement parameters with appropriate attributes
5. Override `ProcessRecord()`, `BeginProcessing()`, or `EndProcessing()` as needed
6. Build the project
7. Import the module and test the cmdlet
8. Run `updatehelp.ps1` to generate documentation
9. Update the documentation markdown file as needed

### Modifying Type Conversion

- Locate the conversion logic in `DataverseEntityConverter.cs`
- The `GetPSValue()` method converts Dataverse values to PowerShell objects
- The `GetDataverseValue()` method converts PowerShell objects to Dataverse values
- Add special cases for new attribute types as needed
- Ensure metadata lookups are performed for lookups and choice values

### Running the Module Locally

```powershell
# Build and import the module
dotnet build -c Debug
Import-Module ./Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0/Rnwood.Dataverse.Data.PowerShell.psd1 -Force

# Test a cmdlet
$connection = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive
Get-DataverseRecord -Connection $connection -TableName systemuser
```

## CI/CD Pipeline

The project uses GitHub Actions (`.github/workflows/publish.yml`):
- Builds on both Windows and Ubuntu with multiple PowerShell versions (5.1, 7.4.11, latest)
- Runs Pester tests on all configurations
- Runs E2E tests against a real Dataverse environment
- Publishes to PowerShell Gallery on release

## Important Notes

- The module is not code-signed (donations welcome!)
- Users must set execution policy to allow unsigned scripts: `Set-ExecutionPolicy RemoteSigned -Scope CurrentUser`
- The module supports various authentication methods: interactive, username/password, client secret, device code
- Automatic paging is implemented for queries that return large result sets
- Batching is supported for bulk create/update/delete operations
- The module does NOT support on-premise Dataverse environments

## Key Features to Maintain

1. **Cross-platform compatibility** - Must work on both .NET Framework and .NET Core
2. **Idiomatic PowerShell** - Use PowerShell conventions, support pipeline, implement common parameters
3. **Type conversion** - Automatic conversion between PowerShell and Dataverse types
4. **Paging** - Automatic handling of paginated results
5. **Batching** - Efficient bulk operations
6. **Multiple auth methods** - Support various authentication scenarios

## Troubleshooting

- Assembly loading issues are handled by the Loader project's `ModuleInitProvider`
- Different assembly load contexts are used for .NET Core vs .NET Framework
- SDK assemblies are loaded from `cmdlets/net6.0/` or `cmdlets/net462/` depending on runtime
- Build warnings about .NET Framework compatibility are expected for some dependencies
