# Rnwood.Dataverse.Data.PowerShell

<img src="logo.svg" height=150/>

A PowerShell module for connecting to Microsoft Dataverse (used by Dynamics 365 and Power Apps) to query and manipulate data. 

This module works in PowerShell Desktop and PowerShell Core, supporting Windows, Linux, and macOS.

## Features

- Creating, updating, upserting and deleting records including M:M records
- **View management**: Create, update, retrieve, and delete system and personal views with FetchXML or simplified filter syntax
- **App module management**: Create, update, retrieve, and delete model-driven apps
- Simple PowerShell objects for input and output instead of complex SDK Entity classes
- Automatic data type conversion using metadata - use friendly labels for choices and names for lookups
- Automatic lookup conversion - use record names instead of GUIDs (when unique)
- On behalf of (delegation) support for create/update operations
- Multiple query methods with full support for automatic paging
- Concise hashtable-based filters with grouped logical expressions (and/or/not/xor) and arbitrary nesting
- Batching support for efficient bulk operations
- **Comprehensive metadata CRUD operations** — Create, read, update, and delete entities, attributes, option sets, and relationships with full coverage of all attribute types and relationship types (OneToMany, ManyToMany)
- **Global metadata caching** — Optional shared cache for improved performance when working with metadata
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
```

For more advanced scenarios including view management and app module management, see the [documentation](#documentation) section below.

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
- [View Management](docs/core-concepts/view-management.md) - Create, update, and manage system and personal views
- [App Module Management](docs/core-concepts/app-module-management.md) - Create, update, and manage model-driven apps
- [Working with Metadata](docs/core-concepts/metadata.md) - Reading and managing schema (entities, attributes, relationships, option sets)
- [Error Handling and Batch Operations](docs/core-concepts/error-handling.md) - Error handling and retry logic
- [Environment Variables and Connection References](docs/core-concepts/environment-variables-connection-references.md) - Managing configuration and connections

### Advanced Topics
- [Parallelization](docs/advanced/parallelization.md) - Parallel processing for best performance
- [Solution Management](docs/advanced/solution-management.md) - Import, export, and manage solutions

### Reference
- [Cmdlet Documentation](Rnwood.Dataverse.Data.PowerShell/docs/) - Full cmdlet reference with parameters and examples

## Main Cmdlets

### Record Management
- [`Get-DataverseConnection`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseConnection.md) — create or retrieve a connection
- [`Get-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) — query and retrieve records
- [`Set-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md) — create, update or upsert records
- [`Remove-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseRecord.md) — delete records

### View Management
- [`Get-DataverseView`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseView.md) — retrieve system and personal views
- [`Set-DataverseView`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseView.md) — create or update views with FetchXML or simplified filters
- [`Remove-DataverseView`](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseView.md) — delete views

### App Module Management
- [`Get-DataverseAppModule`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseAppModule.md) — retrieve app modules (model-driven apps)
- [`Set-DataverseAppModule`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseAppModule.md) — create or update app modules
- [`Remove-DataverseAppModule`](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseAppModule.md) — delete app modules

### App Module Component Management
- [`Get-DataverseAppModuleComponent`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseAppModuleComponent.md) — retrieve components included in an app (entities, forms, views, etc.)
- [`Set-DataverseAppModuleComponent`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseAppModuleComponent.md) — add or update a component within an app
- [`Remove-DataverseAppModuleComponent`](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseAppModuleComponent.md) — remove a component from an app

### Advanced Operations
- [`Invoke-DataverseRequest`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseRequest.md) — execute arbitrary SDK requests
- [`Invoke-DataverseSql`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseSql.md) — run SQL queries against Dataverse

### Sitemap Management

- [`Get-DataverseSitemap`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseSitemap.md) — retrieve sitemap navigation definitions
- [`Set-DataverseSitemap`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseSitemap.md) — create or update sitemap navigation
- [`Remove-DataverseSitemap`](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseSitemap.md) — delete sitemap navigation
- [`Get-DataverseSitemapEntry`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseSitemapEntry.md) — retrieve sitemap entries (Areas, Groups, SubAreas)
- [`Add-DataverseSitemapEntry`](Rnwood.Dataverse.Data.PowerShell/docs/Add-DataverseSitemapEntry.md) — add new navigation entries to sitemap
- [`Set-DataverseSitemapEntry`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseSitemapEntry.md) — update existing navigation entries
- [`Remove-DataverseSitemapEntry`](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseSitemapEntry.md) — remove navigation entries from sitemap

### Environment Variables
- [`Get-DataverseEnvironmentVariableDefinition`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseEnvironmentVariableDefinition.md) — query environment variable definitions
- [`Set-DataverseEnvironmentVariableDefinition`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseEnvironmentVariableDefinition.md) — create or update environment variable definitions
- [`Remove-DataverseEnvironmentVariableDefinition`](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseEnvironmentVariableDefinition.md) — remove environment variable definitions
- [`Get-DataverseEnvironmentVariableValue`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseEnvironmentVariableValue.md) — query environment variable values
- [`Set-DataverseEnvironmentVariableValue`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseEnvironmentVariableValue.md) — set environment variable values
- [`Remove-DataverseEnvironmentVariableValue`](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseEnvironmentVariableValue.md) — remove environment variable values

### Connection References
- [`Get-DataverseConnectionReference`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseConnectionReference.md) — query connection references
- [`Set-DataverseConnectionReference`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseConnectionReference.md) — create or update connection reference values
- [`Remove-DataverseConnectionReference`](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseConnectionReference.md) — remove connection references

### Metadata Operations
- [`Get-DataverseEntityMetadata`](docs/core-concepts/metadata.md) — retrieve entity metadata
- [`Get-DataverseAttributeMetadata`](docs/core-concepts/metadata.md) — retrieve attribute metadata
- [`Get-DataverseOptionSetMetadata`](docs/core-concepts/metadata.md) — retrieve option set values
- [`Get-DataverseRelationshipMetadata`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRelationshipMetadata.md) — retrieve relationship metadata
- [`Set-DataverseEntityMetadata`](docs/core-concepts/metadata.md) — create or update entities
- [`Set-DataverseAttributeMetadata`](docs/core-concepts/metadata.md) — create or update attributes (all types)
- [`Set-DataverseOptionSetMetadata`](docs/core-concepts/metadata.md) — create or update global option sets
- [`Set-DataverseRelationshipMetadata`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRelationshipMetadata.md) — create relationships (OneToMany, ManyToMany)
- [`Remove-DataverseEntityMetadata`](docs/core-concepts/metadata.md) — delete entities
- [`Remove-DataverseAttributeMetadata`](docs/core-concepts/metadata.md) — delete attributes
- [`Remove-DataverseRelationshipMetadata`](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseRelationshipMetadata.md) — delete relationships

### Metadata Cache Management
- `Clear-DataverseMetadataCache` — clear the metadata cache

### Additional Operations
For operations not covered by the cmdlets above, use [`Invoke-DataverseRequest`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseRequest.md) with SDK request objects to execute any Dataverse SDK operation directly.

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

### Contributing

When submitting pull requests, please use **Conventional Commits** format in your PR title to enable automatic versioning:

**Format:** `<type>(<scope>): <description>`

**Examples:**
- `feat: add batch delete operation` — Minor version bump (1.4.0 → 1.5.0)
- `fix: resolve connection timeout` — Patch version bump (1.4.0 → 1.4.1)
- `feat!: remove deprecated parameters` — Major version bump (1.4.0 → 2.0.0)

See [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines.

## License

See [LICENSE](LICENSE) file for details.
