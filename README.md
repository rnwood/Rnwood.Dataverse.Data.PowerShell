# Rnwood.Dataverse.Data.PowerShell

<img src="logo.svg" height=150/>

A PowerShell module for connecting to Microsoft Dataverse (used by Dynamics 365 and Power Apps) to query and manipulate data.

This module works in PowerShell Desktop and PowerShell Core, supporting Windows, Linux, and macOS.

## Features

- Creating, updating, upserting and deleting records including M:M records
- Simple PowerShell objects for input and output instead of complex SDK Entity classes
- Automatic data type conversion using metadata - use friendly labels for choices and names for lookups
- Automatic lookup conversion - use record names instead of GUIDs (when unique)
- On behalf of (delegation) support for create/update operations
- Multiple query methods with full support for automatic paging
- Concise hashtable-based filters with grouped logical expressions (and/or/not/xor) and arbitrary nesting
- Batching support for efficient bulk operations
- Wide variety of auth options for interactive and unattended use
- **XrmToolbox Plugin**: Embedded PowerShell console with automatic connection bridging. See [XrmToolbox Plugin README](Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin/README.md)
- Retry logic with configurable retry counts and exponential backoff

**Note**: On-premise Dataverse environments are not supported.

## Quick Start

### Installation

```powershell
# Set execution policy (one-time setup)
Set-ExecutionPolicy –ExecutionPolicy RemoteSigned –Scope CurrentUser

# Install module
Install-Module Rnwood.Dataverse.Data.PowerShell -Scope CurrentUser
```

For detailed installation instructions including versioning, see [Installation Guide](docs/getting-started/installation.md).

### Basic Usage

```powershell
# Connect to Dataverse
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -interactive

# Query records
Get-DataverseRecord -Connection $c -TableName contact -FilterValues @{ lastname = 'Smith' }

# Create a record
Set-DataverseRecord -Connection $c -TableName contact -InputObject @{ 
    firstname = 'John'
    lastname = 'Doe'
    emailaddress1 = 'john.doe@example.com'
} -CreateOnly

# Update a record
Set-DataverseRecord -Connection $c -TableName contact -Id $contactId -InputObject @{ 
    description = 'Updated via PowerShell'
}

# Delete a record
Remove-DataverseRecord -Connection $c -TableName contact -Id $contactId

# Manage web resources
# Upload a JavaScript file
Set-DataverseWebResource -Connection $c -Name "new_myscript" -Path "./script.js" -DisplayName "My Script"

# Download a web resource
Get-DataverseWebResource -Connection $c -Name "new_myscript" -Path "./downloaded-script.js"

# Upload all files from a folder
Set-DataverseWebResource -Connection $c -Folder "./webresources" -PublisherPrefix "new"
```

## Documentation

### Getting Started
- [Installation](docs/getting-started/installation.md) - How to install and update the module
- [Quick Start Guide](docs/getting-started/quickstart.md) - Basic operations and PowerShell best practices
- [Authentication Methods](docs/getting-started/authentication.md) - All supported authentication methods

### Core Concepts
- [Connection Management](docs/core-concepts/connections.md) - Default connections, named connections
- [Querying Records](docs/core-concepts/querying.md) - Filtering, paging, sorting, linking, SQL queries
- [Creating and Updating Records](docs/core-concepts/creating-updating.md) - Create, update, upsert operations
- [Deleting Records](docs/core-concepts/deleting.md) - Delete operations and SQL alternatives
- [Error Handling and Batch Operations](docs/core-concepts/error-handling.md) - Error handling and retry logic

### Advanced Topics
- [Parallelization](docs/advanced/parallelization.md) - Parallel processing for best performance
- [Solution Management](docs/advanced/solution-management.md) - Import, export, and manage solutions

### Reference
- [Cmdlet Documentation](Rnwood.Dataverse.Data.PowerShell/docs/) - Full cmdlet reference with parameters and examples

## Main Cmdlets

- [`Get-DataverseConnection`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseConnection.md) — create or retrieve a connection
- [`Get-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) — query and retrieve records
- [`Set-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md) — create, update or upsert records
- [`Remove-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseRecord.md) — delete records
- [`Get-DataverseWebResource`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseWebResource.md) — retrieve web resources with file support
- [`Set-DataverseWebResource`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseWebResource.md) — create or update web resources from files or folders
- [`Remove-DataverseWebResource`](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseWebResource.md) — delete web resources
- [`Invoke-DataverseRequest`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseRequest.md) — execute arbitrary SDK requests
- [`Invoke-DataverseSql`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseSql.md) — run SQL queries against Dataverse

The module also includes many specialized `Invoke-Dataverse*` cmdlets for specific platform operations. See the [cmdlet documentation](Rnwood.Dataverse.Data.PowerShell/docs/) for the full list.

## Testing

This module has comprehensive test coverage using Pester and FakeXrmEasy. To understand test coverage and identify gaps:

- **[Test Coverage Gap Analysis](TEST_COVERAGE_GAP_ANALYSIS.md)** — Detailed analysis of all documented features and test coverage
- **[Test Coverage Quick Reference](TEST_COVERAGE_QUICK_REFERENCE.md)** — At-a-glance summary and implementation guide

To run tests:

```bash
# Build the module
dotnet build

# Set module path
export TESTMODULEPATH=$(pwd)/Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0

# Run all tests (must use All.Tests.ps1 as entry point)
pwsh -Command "Invoke-Pester -Path tests/All.Tests.ps1 -Output Normal"
```

**Note:** Tests must be run through `tests/All.Tests.ps1` which provides necessary setup (getMockConnection, module loading, metadata). Individual test files cannot be run directly.

## Support and Contributing

- Report issues: [GitHub Issues](https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell/issues)
- View source: [GitHub Repository](https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell)
- Test coverage reports: See [TEST_COVERAGE_GAP_ANALYSIS.md](TEST_COVERAGE_GAP_ANALYSIS.md)

## License

See [LICENSE](LICENSE) file for details.
