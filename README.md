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
- Full support for automatic paging
- Concise PowerShell friendly hashtable based filters with grouped logical expressions (and/or/not/xor) and arbitrary nesting
- Batching and parallelisation support for efficient bulk operations
- Auto retries support in many cmdlets
- Comprehensive metadata operations
    - Create, read, update, and delete entities, attributes, option sets, and relationships
    - manipulate model-driven apps, forms, views
    - manipulate solutions and solution components

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

# Upload all files from a folder (only if newer)
Set-DataverseWebResource -Connection $c -Folder "./webresources" -PublisherPrefix "new" -IfNewer

# Download all JavaScript web resources
Get-DataverseWebResource -Connection $c -WebResourceType 3 -Folder "./downloaded"
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
- [Managing Web Resources](docs/core-concepts/web-resources.md) - Upload, download, and manage web resources with file system integration
- [Record Access Management](docs/core-concepts/record-access-management.md) - Test, grant, list, and revoke record-level access rights
- [View Management](docs/core-concepts/view-management.md) - Create, update, and manage system and personal views
- [App Module Management](docs/core-concepts/app-module-management.md) - Create, update, and manage model-driven apps
- [Working with Metadata](docs/core-concepts/metadata.md) - Reading and managing schema (entities, attributes, relationships, option sets)
- [Solution Component Management](docs/core-concepts/solution-component-management.md) - Managing individual components within solutions
- [Error Handling and Batch Operations](docs/core-concepts/error-handling.md) - Error handling and retry logic
- [Environment Variables and Connection References](docs/core-concepts/environment-variables-connection-references.md) - Managing configuration and connections

### Advanced Topics
- [Parallelization](docs/advanced/parallelization.md) - Parallel processing for best performance
- [Solution Management](docs/advanced/solution-management.md) - Import, export, and manage solutions

### Reference
- [Cmdlet Documentation](Rnwood.Dataverse.Data.PowerShell/docs/) - Full cmdlet reference with parameters and examples

## Main Cmdlets

### Data Operations
- [`Get-DataverseConnection`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseConnection.md) — create or retrieve a connection
- [`Get-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) — query and retrieve records
- [`Set-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md) — create, update or upsert records
- [`Remove-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseRecord.md) — delete records

### Record Access Management
- [`Test-DataverseRecordAccess`](Rnwood.Dataverse.Data.PowerShell/docs/Test-DataverseRecordAccess.md) — test access rights a principal has for a record
- [`Get-DataverseRecordAccess`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecordAccess.md) — list all principals with shared access to a record
- [`Set-DataverseRecordAccess`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecordAccess.md) — grant or modify access rights for a principal on a record
- [`Remove-DataverseRecordAccess`](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseRecordAccess.md) — revoke access rights from a principal on a record

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

### Web Resource Management
- [`Get-DataverseWebResource`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseWebResource.md) — retrieve web resources with file support
- [`Set-DataverseWebResource`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseWebResource.md) — create or update web resources from files or folders
- [`Remove-DataverseWebResource`](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseWebResource.md) — delete web resources

### Advanced Operations
- [`Invoke-DataverseRequest`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseRequest.md) — execute arbitrary SDK requests
- [`Invoke-DataverseSql`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseSql.md) — run SQL queries against Dataverse

### Sitemap Management

**Multilingual Support:** Sitemap cmdlets support multilingual titles and descriptions using LCID-based dictionaries, enabling proper localization for Areas, Groups, and SubAreas.

- [`Get-DataverseSitemap`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseSitemap.md) — retrieve sitemap navigation definitions
- [`Set-DataverseSitemap`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseSitemap.md) — create or update sitemap navigation
- [`Remove-DataverseSitemap`](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseSitemap.md) — delete sitemap navigation
- [`Get-DataverseSitemapEntry`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseSitemapEntry.md) — retrieve sitemap entries (Areas, Groups, SubAreas) with multilingual titles
- [`Set-DataverseSitemapEntry`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseSitemapEntry.md) — create or update navigation entries with multilingual support
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

### Form Management
- [`Get-DataverseForm`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseForm.md) — retrieve form definitions with optional FormXml parsing
- [`Set-DataverseForm`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseForm.md) — create or update forms with FormXml support
- [`Remove-DataverseForm`](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseForm.md) — delete forms from entities

### Form Component Management
**Tabs:**
- [`Get-DataverseFormTab`] — retrieve tabs from forms
- [`Set-DataverseFormTab`] — create or update form tabs
- [`Remove-DataverseFormTab`] — delete tabs from forms

**Sections:**
- [`Get-DataverseFormSection`] — retrieve sections from form tabs
- [`Set-DataverseFormSection`] — create or update form sections
- [`Remove-DataverseFormSection`] — delete sections from forms

**Controls:**
- [`Get-DataverseFormControl`] — retrieve controls from form sections
- [`Set-DataverseFormControl`] — create or update form controls (supports all standard control types and raw XML)
- [`Remove-DataverseFormControl`] — delete controls from forms

### Additional Operations
For operations not covered by the cmdlets above, use [`Invoke-DataverseRequest`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseRequest.md) with SDK request objects to execute any Dataverse SDK operation directly. The cmdlet supports two main approaches:
- **Request parameter set**: Pass SDK request objects directly (returns raw SDK response objects)
- **NameAndInputs parameter set**: Specify request name and parameters as hashtable (returns converted PSObject by default, use `-Raw` for raw response)

See the [Invoke-DataverseRequest documentation](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseRequest.md) for details on response conversion and parameter sets.

## Support and Contributing

- Report issues: [GitHub Issues](https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell/issues)
- View source: [GitHub Repository](https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell)

### Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines.

## License

See [LICENSE](LICENSE) file for details.
