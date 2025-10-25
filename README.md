<img src="logo.svg" height=150/>

This is a module for PowerShell to connect to Microsoft Dataverse (used by many Dynamics 365 and Power Apps applications as well as others) and query and manipulate data.

This module works in PowerShell Desktop and PowerShell Core, so it should work on any platform where Core is supported.

Features:
- Creating, updating, upserting and deleting records including M:M records.
- Uses simple PowerShell objects for input and output instead of complex SDK Entity classes, making it much easier to work with in PowerShell scripts.
- Automatically converts data types using metadata, allowing you to use friendly labels for choices and names for lookups etc, simplifying PowerShell usage.
- Automatic conversion for lookup type values in both input and output directions. You can use the name of the record to refer to a record you want to associate with as long as it's unique.
- On behalf of (delegation) support to create/update records on behalf of another user.
- Querying records using a variety of methods.
- Full support for returning the full result set across pages (automatic paging).
- Supports concise hashtable-based filters including grouped logical expressions (and/or), negation (`not`) and exclusive-or (`xor`) with arbitrary nesting depth.
- Batching support to create/update/upsert many records in a single request to service.
- Wide variety of auth options for both interactive and unattended use.
- **XrmToolbox Plugin**: Embedded PowerShell console directly within XrmToolbox tabs with automatic connection bridging and default connection support (no `-Connection` parameter needed). See [XrmToolbox Plugin README](Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin/README.md) for details.
- Retry logic for resilient operations against transient failures, with configurable retry counts and exponential backoff.

Non features:
- Support for connecting to on-premise environments.

# Table of Contents

- [How to install](#how-to-install)
- [Migration from Microsoft.Xrm.Data.PowerShell](#migration-from-microsoftxrmdatapowershell)
- [Quick Start and Examples](#quick-start-and-examples)
  - [PowerShell Best Practices](#powershell-best-practices)
  - [Getting Connected](#getting-connected)
    - [Default Connection](#default-connection)
    - [Authentication Methods](#authentication-methods)
  - [Main Cmdlets](#main-cmdlets)
  - [Querying Records](#querying-records)
    - [Column output and type conversions](#column-output-and-type-conversions)
    - [Filtering with simple syntax](#filtering-with-simple-syntax)
    - [Including and Excluding by Name or Id](#including-and-excluding-by-name-or-id)
    - [Querying with SQL](#querying-with-sql)
    - [Advanced Filtering with QueryExpression](#advanced-filtering-with-queryexpression)
    - [Advanced Filtering with FetchXML](#advanced-filtering-with-fetchxml)
    - [Getting total record count](#getting-total-record-count)
      - [Using -TotalRecordCount switch](#using--totalrecordcount-switch)
      - [Using -VerboseRecordCount switch](#using--verboserecordcount-switch)
    - [Sorting records](#sorting-records)
    - [Linking Related Tables](#linking-related-tables)
  - [Creating and Updating Records](#creating-and-updating-records)
    - [Input object shape and type conversion (create/update)](#input-object-shape-and-type-conversion-createupdate)
    - [Advanced Set-DataverseRecord Parameters](#advanced-set-dataverserecord-parameters)
    - [Assigning records](#assigning-records)
    - [Setting state and status](#setting-state-and-status)
    - [Alternate Keys Explanation](#alternate-keys-explanation)
    - [SQL alternative — Create / Update](#sql-alternative--create--update)
  - [Deleting Records](#deleting-records)
    - [Deleting only if the record still exists](#deleting-only-if-the-record-still-exists)
    - [SQL alternative — Delete](#sql-alternative--delete)
  - [Error Handling](#error-handling)
  - [Getting IDs of Created Records](#getting-ids-of-created-records)
  - [Batch Operations](#batch-operations)
  - [Retry Logic](#retry-logic)
  - [Parallelising work for best performance](#parallelising-work-for-best-performance)
  - [Solution Management](#solution-management)
    - [Exporting solutions](#exporting-solutions)
    - [Importing solutions](#importing-solutions)
- [Specialized Invoke-Dataverse* Cmdlets](#specialized-invoke-dataverse-cmdlets)
  - [How to Find and Use Specialized Cmdlets](#how-to-find-and-use-specialized-cmdlets)
  - [Usage Pattern](#usage-pattern)
  - [When to Use Specialized Cmdlets](#when-to-use-specialized-cmdlets)

- [Using PowerShell Standard Features](#using-powershell-standard-features)
  - [Pipeline vs ForEach-Object](#pipeline-vs-foreach-object)

  - [WhatIf and Confirm](#whatif-and-confirm)
  - [Verbose Output](#verbose-output)
  - [Error Handling](#error-handling-1)
  - [Warning Messages](#warning-messages)
  - [Tab Completion](#tab-completion)
  - [Contextual Help](#contextual-help)
  - [Command History](#command-history)
  - [Additional Resources](#additional-resources)
- [Full Cmdlet Documentation](#full-cmdlet-documentation)
- [FAQ](#faq)

- [Common Use-Cases](#common-use-cases)
  - [Using in CI/CD Pipelines](#using-in-cicd-pipelines)
    - [Azure DevOps Pipelines](#azure-devops-pipelines)
    - [GitHub Actions](#github-actions)
    - [Azure App Registration Setup](#azure-app-registration-setup)
  - [Exporting data to a file (JSON, CSV, XML, XLSX)](#exporting-data-to-a-file-json-csv-xml-xlsx)
    - [Mapping and transforming columns](#mapping-and-transforming-columns)
    - [JSON (small exports)](#json-small-exports)
    - [JSON (large streaming)](#json-large-streaming)
    - [CSV](#csv)
    - [XML](#xml)
    - [XLSX (Excel)](#xlsx-excel)
    - [SQL Server](#sql-server)
  - [Importing data from a file (JSON, CSV, XML, XLSX)](#importing-data-from-a-file-json-csv-xml-xlsx)
    - [JSON (import)](#json-import)
    - [JSON (NDJSON streaming import)](#json-ndjson-streaming-import)
    - [CSV (import)](#csv-import)
    - [XML (import)](#xml-import)
    - [XLSX (import)](#xlsx-import)
    - [SQL Server (import)](#sql-server-import)
  - [Mass updating data](#mass-updating-data)
    - [Using SQL](#using-sql)
  - [Managing data in source control](#managing-data-in-source-control)
  - [Copying data between environments](#copying-data-between-environments)

# How to install

**Requirements:**
- PowerShell Desktop 5.1+ or PowerShell Core 7.4+
- For PowerShell Core < 7.4, use an earlier version of this module

This module is not signed (donation of funds for code signing certificate are welcome). So PowerShell must be configured to allow loading unsigned scripts that you install from remote sources (the Powershell gallery).

```powershell
Set-ExecutionPolicy –ExecutionPolicy RemoteSigned –Scope CurrentUser
```
To install:
```powershell
Install-Module Rnwood.Dataverse.Data.PowerShell -Scope CurrentUser
```

> [!NOTE]
> Pinning the specific version you have tested is recommended for important script.
> Then you can test and move forwards in a controlled way.

To install a specific version:

```powershell
Install-Module Rnwood.Dataverse.Data.PowerShell -RequiredVersion 100.0.0 -Scope CurrentUser
```

To update:
```
Update-Module Rnwood.Dataverse.Data.PowerShell -Force
```

# Quick Start and Examples

### PowerShell Best Practices

> [!IMPORTANT]
> Set `$ErrorActionPreference = "Stop"` at the beginning of your scripts. This turns non-terminating errors into terminating errors so scripts stop immediately on failure:
>
> ```powershell
> # Add this at the start of your scripts
> $ErrorActionPreference = "Stop"
> ```

Without this setting, PowerShell's default behavior is to continue execution after non-terminating errors, which can lead to unexpected results - cascading failures and accidental data corruption.

### Getting Connected

Get a connection to a target Dataverse environment using the [`Get-DataverseConnection`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseConnection.md) cmdlet (also available as `Connect-DataverseConnection` alias).

Each cmdlet that interacts with Dataverse requires a `-Connection` parameter to specify which environment to use. You typically provide the connection object (e.g., `$c`) returned by [`Get-DataverseConnection`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseConnection.md).


#### Default Connection

You can set a connection as the default, so you don't have to pass [`-Connection`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-connection) to every cmdlet:

*Example: Set a default connection and use it implicitly:*
```powershell
# Set a connection as default
Connect-DataverseConnection -url https://myorg.crm11.dynamics.com -interactive -SetAsDefault

# Now you can omit -Connection from all cmdlets
Get-DataverseRecord -tablename contact
Set-DataverseRecord -tablename contact @{firstname="John"; lastname="Doe"}

# You can retrieve the current default connection
$currentDefault = Get-DataverseConnection -GetDefault
```

This is especially useful in interactive sessions and scripts where you're working with a single environment.

#### Authentication Methods

The module supports multiple authentication methods:

##### Interactive
Browser-based authentication (good for development). Omit the URL to select from available environments.

*Example: Get a connection to MYORG using interactive authentication:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -interactive
```

*Example: Get a connection by selecting from available environments:*
```powershell
$c = Get-DataverseConnection -interactive
```

##### PAC CLI Profile
Power Platform CLI (PAC) authentication profile (leverages existing PAC CLI authentication).

*Example: Using the current PAC CLI profile:*
```powershell
Get-DataverseConnection -FromPac -SetAsDefault
```

*Example: Using a specific PAC CLI profile by name or index:*
```powershell
$c = Get-DataverseConnection -FromPac -Profile "MyDevProfile"
# or by index
$c = Get-DataverseConnection -FromPac -Profile "0"
```

##### Device Code
Authentication via device code flow (good for remote/headless scenarios).

*Example: Using device code authentication:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -devicecode
```

##### Username/Password
Basic credential authentication (not recommended)

*Example: Using username and password authentication:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -username "user@domain.com" -password "mypassword"
```

##### Client Secret
Service principal authentication (good for automation).

*Example: Using client secret authentication:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -clientid "3004eb1e-7a00-45e0-a1dc-6703735eac18" -clientsecret "itsasecret"
```

##### Client Certificate
Certificate-based service principal authentication (good for secure automation).

*Example: Using client certificate from file:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -clientid "3004eb1e-7a00-45e0-a1dc-6703735eac18" -certificatepath "C:\certs\mycert.pfx" -certificatepassword "P@ssw0rd"
```

*Example: Using client certificate from Windows certificate store:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -clientid "3004eb1e-7a00-45e0-a1dc-6703735eac18" -certificatethumbprint "A1B2C3D4E5F6789012345678901234567890ABCD"
```

*Example: Using client certificate from LocalMachine store:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -clientid "3004eb1e-7a00-45e0-a1dc-6703735eac18" -certificatethumbprint "A1B2C3D4E5F6789012345678901234567890ABCD" -certificatestorelocation LocalMachine
```

##### DefaultAzureCredential
Automatic credential discovery in Azure environments (tries environment variables, managed identity, Visual Studio, Azure CLI, Azure PowerShell, and interactive browser).

*Example: Using DefaultAzureCredential in Azure environments:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -DefaultAzureCredential
```

##### ManagedIdentity
Azure managed identity authentication (system-assigned or user-assigned).

*Example: Using Managed Identity on Azure VM/Functions/App Service:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -ManagedIdentity
```

*Example: Using user-assigned managed identity:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -ManagedIdentity -ManagedIdentityClientId "12345678-1234-1234-1234-123456789abc"
```

##### Connection String
Advanced scenarios using connection strings.

*Example: Using a Dataverse SDK connection string:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -connectionstring "AuthType=ClientSecret;ClientId=3004eb1e-7a00-45e0-a1dc-6703735eac18;ClientSecret=itsasecret;Url=https://myorg.crm11.dynamics.com"
```

#### Named Connections

You can save connections with a name for easy reuse. Named connections persist authentication tokens securely using the platform's credential storage (Keychain on macOS, Credential Manager on Windows, libsecret on Linux) and save connection metadata for later retrieval.

##### Saving a Named Connection

Add the `-Name` parameter when connecting to save the connection:

*Example: Save a connection for later use:*
```powershell
# Interactive authentication - tokens are cached securely
Get-DataverseConnection -url https://myorg.crm11.dynamics.com -interactive -Name "MyOrgProd"

# Device code authentication
Get-DataverseConnection -url https://myorg.crm11.dynamics.com -devicecode -Name "MyOrgDev"

# Username/password authentication
Get-DataverseConnection -url https://myorg.crm11.dynamics.com -username "user@domain.com" -password "pass" -Name "MyOrgTest"
```

**Security Note:** By default, client secrets, certificate passwords, and user passwords are NOT saved for security reasons. You'll need to provide them again when loading the connection.

If you need to save credentials for testing or non-production scenarios, use the `-SaveCredentials` switch (NOT RECOMMENDED for production):

```powershell
# Save username/password (NOT RECOMMENDED for production)
Get-DataverseConnection -url https://myorg.crm11.dynamics.com -username "user@domain.com"****** -Name "MyOrgTest" -SaveCredentials

# Save client secret (NOT RECOMMENDED for production)
Get-DataverseConnection -url https://myorg.crm11.dynamics.com -clientid "..." -clientsecret "..." -Name "MyOrgTest" -SaveCredentials

# Save certificate with password (NOT RECOMMENDED for production)
Get-DataverseConnection -url https://myorg.crm11.dynamics.com -clientid "..." -CertificatePath "cert.pfx" -CertificatePassword "..." -Name "MyOrgCert" -SaveCredentials
```

**IMPORTANT:** Using `-SaveCredentials` stores secrets **encrypted** on disk using:
- **Windows**: Data Protection API (DPAPI) - user-specific encryption
- **Linux/macOS**: AES encryption with machine-specific key

While encrypted, this is still NOT RECOMMENDED for production use. Only use for testing or non-production scenarios.

##### Loading a Named Connection

Restore a saved connection by name. The module will use cached authentication tokens (if still valid) or prompt for re-authentication:

*Example: Load a saved connection:*
```powershell
$c = Get-DataverseConnection -Name "MyOrgProd"
# Connection restored with cached credentials
```

##### Clearing All Saved Connections

Remove all saved connections and cached tokens:

*Example: Clear all connections:*
```powershell
Get-DataverseConnection -ClearAllConnections
# All saved connections and cached tokens have been cleared.
```

##### Listing Saved Connections

View all saved named connections:

*Example: List all saved connections:*
```powershell
Get-DataverseConnection -ListConnections
# Output shows: Name, Url, AuthMethod, Username, SavedAt
```

##### Deleting a Named Connection

Remove a saved connection and its cached credentials:

*Example: Delete a saved connection:*
```powershell
Get-DataverseConnection -DeleteConnection -Name "MyOrgDev"
# Connection 'MyOrgDev' deleted successfully.
```

**Benefits of Named Connections:**
- **Convenience**: No need to remember URLs or re-authenticate frequently
- **Security**: Tokens are stored securely using platform-native credential storage
- **Multiple Environments**: Easily switch between dev, test, and production environments
- **CI/CD Friendly**: Save connections in CI/CD pipelines with service principal credentials

### Main Cmdlets

The module exposes a small set of main cmdlets for common operations. See the detailed docs linked below for full parameter lists and examples.

- [`Get-DataverseConnection`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseConnection.md) — create or retrieve a connection to a Dataverse environment
- [`Get-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) — query and retrieve records
- [`Set-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md) — create, update or upsert records (including assignment and ownership changes)
- [`Remove-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseRecord.md) — delete records
- [`Invoke-DataverseRequest`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseRequest.md) — execute arbitrary SDK requests (see note below about specialised cmdlets for the SDK included ones)
- [`Invoke-DataverseSql`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseSql.md) — run SQL queries against Dataverse (where supported)

See the full documentation for each cmdlet in the [`docs/`](Rnwood.Dataverse.Data.PowerShell/docs) folder.

Note on "Invoke-" style commands: in addition to the primary cmdlets above, this module exposes many specialised
`Invoke-Dataverse*` helper cmdlets that wrap specific SDK/platform requests (for example
[`Invoke-DataverseRetrieveTotalRecordCount`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseRetrieveTotalRecordCount.md), [`Invoke-DataverseBulkDelete`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseBulkDelete.md), [`Invoke-DataverseBulkDetectDuplicates`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseBulkDetectDuplicates.md),
[`Invoke-DataverseImportSolution`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseImportSolution.md), and many more). These are useful for single-purpose platform operations — see
their individual documentation in the `docs/` folder (for example:
[Invoke-DataverseRequest](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseRequest.md),
[Invoke-DataverseRetrieveTotalRecordCount](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseRetrieveTotalRecordCount.md),
[Invoke-DataverseBulkDelete](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseBulkDelete.md)).

For most interactive and scripting scenarios prefer the primary, typed cmdlets listed above ([`Get-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md),
[`Set-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md), [`Remove-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseRecord.md), or [`Invoke-DataverseSql`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseSql.md)) because they return friendly PSObjects and
handle paging, conversion and batching for you. Use the specialised `Invoke-Dataverse*` commands when you need a
specific platform/request behaviour that the primary cmdlets don't provide.

### Querying Records

Use [`Get-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) to retrieve records from Dataverse. By default the cmdlet returns PowerShell objects that include `Id` and `TableName` properties, and can be piped directly to other cmdlets. Each returned object also contains a property for every column returned by the query (the property name equals the column's logical name).

#### Column output and type conversions

The cmdlet converts Dataverse attribute types to PowerShell-friendly values when building the output PSObject. The following table summarises the typical output and how to control it:

| Dataverse attribute | PowerShell output | How to control / notes |
|---|---|---|
| Any requested column | Appears as a property on the returned PSObject (property name = column logical name) | Use [`-Columns`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-columns) to limit returned columns; system columns excluded by default unless [`-IncludeSystemColumns`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-includesystemcolumns) is set. |
| Lookup (EntityReference / Owner / Customer) | Friendly name string by default, or a reference-like object when raw/display behaviour is changed | Use [`-LookupValuesReturnName`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-lookupvaluesreturnname) to prefer names globally; use per-column `ColumnName:Raw` or `ColumnName:Display` in `-Columns` to override. |
| OptionSet / Status / State | Label string by default; integer value when raw output requested | Use `:Raw` to get numeric values; labels are resolved via metadata. |
| Multi-select picklist | Array of label strings by default; array of numeric values when raw output requested | Use `:Raw` per-column to request numeric values. |
| DateTime | Native `DateTime` object converted to local time | Returned as `DateTime`; include timezone-related columns (e.g., `timezonecode`) when necessary for correct interpretation. |
| Money | Decimal numeric amount | Returned as CLR `decimal` (not SDK Money). |
| PartyList / EntityCollection | Array of PSObjects (one per referenced record) | Each referenced entity is converted to a PSObject containing its columns. |
| Aliased values (from linked entities) | Preserved (raw aliased value) and appear using alias property names | Aliased properties are included as returned by the query so when using outer-join if there's not a match the properties from the joined table will be absent totally. |

If you need SDK types (for example `EntityReference`/`OptionSetValue`) request raw output for those columns using the `:Raw` suffix.

Basic examples:

```powershell
# Get the first 10 contacts
Get-DataverseRecord -Connection $c -TableName contact -Top 10

# Select specific columns
Get-DataverseRecord -Connection $c -TableName contact -Columns firstname,lastname,emailaddress1 -Top 50

The cmdlet automatically pages results for you; use [`-Top`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-top) to limit the number of records returned when testing or when you only need a subset.

*Example: Get all `contact` records using explicit connection:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -interactive
Get-DataverseRecord -connection $c -tablename contact
```

> [!TIP]
> When typing table or field names you can use PowerShell's completion — press `Tab` to cycle suggestions, `Ctrl+Space` to open the completion list, or `F1` for help on the completed token. See the [Tab Completion](#tab-completion) section below for details.

*Example: Get all `contact` records using default connection:*
```powershell
Connect-DataverseConnection -url https://myorg.crm11.dynamics.com -interactive -SetAsDefault
Get-DataverseRecord -tablename contact
```

#### Paging and Limiting How Many Records

The [`Get-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) cmdlet supports parameters to control how many records are retrieved and how they are paged:

- [`-Top`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-top): Limits the total number of records returned. Useful for testing, sampling, or when you only need a subset of data.
- [`-PageSize`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-pagesize): Specifies the number of records to retrieve per page. By default, the cmdlet uses automatic paging to retrieve all matching records efficiently.

Examples:

```powershell
# Get the first 10 contacts
Get-DataverseRecord -Connection $c -TableName contact -Top 10

# Select specific columns and limit to 50 records
Get-DataverseRecord -Connection $c -TableName contact -Columns firstname,lastname,emailaddress1 -Top 50

# Retrieve records with a specific page size for controlled batching
Get-DataverseRecord -Connection $c -TableName contact -PageSize 500
```

The cmdlet automatically pages results for you; use [`-Top`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-top) to limit the number of records returned when testing or when you only need a subset.

#### Including and Excluding by Name or Id

You can include or exclude specific records by their primary key (Id) or primary attribute value (Name):

- [`-Id`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-id): List of primary keys (IDs) of records to retrieve.
- [`-Name`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-name): List of names (primary attribute value) of records to retrieve.
- [`-ExcludeId`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-excludeid): List of record ids to exclude.

Examples:

```powershell
# Retrieve specific records by Id
Get-DataverseRecord -Connection $c -TableName contact -Id "00000000-0000-0000-0000-000000000001", "00000000-0000-0000-0000-000000000002"

# Retrieve records by name
Get-DataverseRecord -Connection $c -TableName contact -Name "John Doe", "Jane Smith"

# Exclude specific records by Id
Get-DataverseRecord -Connection $c -TableName contact -ExcludeId "00000000-0000-0000-0000-000000000003"
```

#### Filtering with simple syntax

Filtering is expressed with simple rules. Start here:

> [!TIP]
> While building filters (the [`-FilterValues`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-filtervalues) hashtables) you can use completion to help with parameter names and column names — press `Tab` to cycle, `Ctrl+Space` to show the full completion list, or `F1` for contextual help. See [Tab Completion](#tab-completion).

- Default comparison: equals. Example:

```powershell
# lastname equals 'Smith'
Get-DataverseRecord -Connection $c -TableName contact -FilterValues @{ lastname = 'Smith' }
```

- Use an explicit operator when needed (preferred syntax):

```powershell
# firstname LIKE 'Rob%'
Get-DataverseRecord -Connection $c -TableName contact -FilterValues @{ firstname = @{ operator = 'Like'; value = 'Rob%' } }
```

- Multiple fields in the same hashtable are combined with AND:

```powershell
# firstname = 'Rob' AND lastname = 'One'
Get-DataverseRecord -Connection $c -TableName contact -FilterValues @{ firstname = 'Rob'; lastname = 'One' }
```

- OR, NOT, XOR and nested logic: use grouping keys with arrays or nested hashtables. Small example for OR:

```powershell
# firstname = 'Rob' OR firstname = 'Joe'
Get-DataverseRecord -Connection $c -TableName contact -FilterValues @(@{ firstname = 'Rob' }, @{ firstname = 'Joe' })
```

*Example: Get contacts where (firstname = 'Rob' OR firstname = 'Joe') AND lastname = 'One':*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -interactive
Get-DataverseRecord -Connection $c -TableName contact -FilterValues @{
  'and' = @(
  @{ 'or' = @(
    @{
      firstname = 'Rob'
    },
    @{
      firstname = 'Joe'
    }
  ) },
  @{
    lastname = 'One'
  }
  )
}
```

*Example: Negation (NOT) — get contacts that do NOT have firstname 'Rob':*
```powershell
Get-DataverseRecord -Connection $c -TableName contact -FilterValues @{
  'not' = @{
      firstname = 'Rob'
  }
}
```

*Example: Exclusive-or (XOR) — exactly one of the subfilters matches:*
```powershell
Get-DataverseRecord -Connection $c -TableName contact -FilterValues @{
  'xor' = @(
  @{
    firstname = 'Rob'
  },
  @{
    firstname = 'Joe'
  }
  )
}
```

> [!WARNING]
> Using `xor` with many items can cause combinatorial expansion and significant performance overhead. The cmdlet enforces a limit of 8 items in an `xor` group to avoid exponential expansion. For large or complex exclusion conditions, prefer FetchXML or SQL.

Note: `xor` can cause combinatorial expansion when negated or used in complex exclude filters; the cmdlet enforces a limit of 8 items in an `xor` group. Use smaller XOR groups, FetchXML, or SQL for large conditions.

##### Excluding records

Use [`-ExcludeFilterValues`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-excludefiltervalues) to remove matching records from the result set. It accepts the same hashtable-based syntax as [`-FilterValues`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-filtervalues) (including grouping keys and explicit operator hashtables).

Examples:

```powershell
# Exclude contacts with firstname 'Rob'
Get-DataverseRecord -Connection $c -TableName contact -ExcludeFilterValues @{ firstname = 'Rob' }

# Include only lastname = 'One', but exclude firstname = 'Rob'
Get-DataverseRecord -Connection $c -TableName contact \
  -FilterValues @{ lastname = 'One' } \
  -ExcludeFilterValues @{ firstname = 'Rob' }
```

Notes:
- When both [`-FilterValues`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-filtervalues) and [`-ExcludeFilterValues`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-excludefiltervalues) are provided the include filters are applied first to select candidate records and the exclude filters are then applied to remove matches from that set.
- [`-ExcludeFilterValues`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-excludefiltervalues) supports grouping and operators just like [`-FilterValues`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-filtervalues) — for example `-ExcludeFilterValues @{'or'=@(@{a=1},@{b=2})}`.


*Example: Exclude records where firstname='Rob' or lastname='Smith':*
```powershell
Get-DataverseRecord -Connection $c -TableName contact -ExcludeFilterValues @{
  'or' = @(
    @{ firstname = 'Rob' },
    @{ lastname = 'Smith' }
  )
}
```

*Example: Include lastnames One or Two, but exclude firstname Rob:* 
```powershell
Get-DataverseRecord -Connection $c -TableName contact -FilterValues @{
  'or' = @(
  @{
    lastname = 'One'
  },
  @{
    lastname = 'Two'
  }
  )
} -ExcludeFilterValues @{
  'xor' = @(
    @{ emailaddress1 = @{
      operator = 'NotNull'
    } },
    @{ mobilephone = @{
      operator = 'NotNull'
    } }
  )
}
```

*Example: Get contacts where (firstname = 'Rob' OR firstname = 'Joe') AND lastname = 'One':*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -interactive
Get-DataverseRecord -Connection $c -TableName contact -FilterValues @{
  'and' = @(
  @{ 'or' = @(
    @{
      firstname = 'Rob'
    },
    @{
      firstname = 'Joe'
    }
  ) },
  @{
    lastname = 'One'
  }
  )
}
```

*Example: Negation (NOT) — get contacts that do NOT have firstname 'Rob':*
```powershell
Get-DataverseRecord -Connection $c -TableName contact -FilterValues @{
  'not' = @{
      firstname = 'Rob'
  }
}
```

*Example: Exclusive-or (XOR) — exactly one of the subfilters matches:*
```powershell
Get-DataverseRecord -Connection $c -TableName contact -FilterValues @{
  'xor' = @(
  @{
    firstname = 'Rob'
  },
  @{
    firstname = 'Joe'
  }
  )
}
```

Note: `xor` can cause combinatorial expansion when negated or used in complex exclude filters; the cmdlet enforces a limit of 8 items in an `xor` group. Use smaller XOR groups, FetchXML, or SQL for large conditions.

#### Advanced Filtering with QueryExpression

When you need programmatic control over complex query logic (dynamic construction, complex joins, or paging control) you can build a QueryExpression (and related SDK types) from PowerShell and execute it directly. For most interactive scenarios prefer the higher-level `-Criteria` and `-Links` parameters on [`Get-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) which accept `FilterExpression` and `DataverseLinkEntity` values and return friendly PowerShell objects.

Example: use `-Criteria` and `-Links` to express a joined query with link filtering and get PSObjects back:

```powershell
$filter = New-Object Microsoft.Xrm.Sdk.Query.FilterExpression([Microsoft.Xrm.Sdk.Query.LogicalOperator]::And)
$filter.AddCondition('lastname',[Microsoft.Xrm.Sdk.Query.ConditionOperator]::Equal,'Smith')
$filter.AddCondition('createdon',[Microsoft.Xrm.Sdk.Query.ConditionOperator]::OnOrAfter,[datetime]::Parse('2025-01-01'))

$link = @{
  'contact.parentcustomerid' = 'account.accountid'
  type = 'LeftOuter'
  alias = 'parentAccount'
  filter = @{ name = @{ operator = 'Like'; value = 'Contoso%' } }
}

Get-DataverseRecord -Connection $c -TableName contact -Columns firstname,lastname,emailaddress1 `
  -Criteria $filter -Links $link
```

Example: build a QueryExpression object when you need programmatic construction of a complex query. QueryExpression objects are useful as inputs to specialized SDK request cmdlets that accept a `Query` parameter (for example bulk-detect / bulk-delete style requests). For simple retrievals prefer [`Get-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) with `-Criteria`/`-Links` or `-FetchXml` which return friendly PSObjects and handle paging for you.

```powershell
# Build a QueryExpression for advanced programmatic scenarios
$qe = New-Object Microsoft.Xrm.Sdk.Query.QueryExpression('contact')
$qe.ColumnSet = New-Object Microsoft.Xrm.Sdk.Query.ColumnSet('firstname','lastname','emailaddress1')
$qe.Criteria.AddCondition('lastname',[Microsoft.Xrm.Sdk.Query.ConditionOperator]::Equal,'Smith')

# Pass $qe to a specialized request cmdlet that accepts a Query parameter when needed
# (many Invoke-Dataverse* request cmdlets support a Query parameter for advanced workflows).
```

Notes:
- [`Get-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) returns friendly PSObjects and automatically pages results for you when using `-TableName`/`-Criteria`/`-Links` or `-FetchXml`.
- When you use low-level SDK request cmdlets that accept a `Query` you may need to manage paging explicitly depending on the request semantics.

#### Advanced Filtering with FetchXML

FetchXML is Dataverse's canonical XML query language and supports features not easily expressed in the concise hashtable syntax: aggregates, grouping, `distinct`, advanced date operators, hierarchical queries, and more complex link-entity arrangements. It's also convenient when you want to copy queries from the Power Apps/Model-driven UI's FetchXML builder or reuse existing FetchXML snippets.

Example: linked-entity filter, ordering and date constraint:

```powershell
$fetchXml = @"
<fetch version="1.0" output-format="xml-platform" mapping="logical">
  <entity name="contact">
    <attribute name="firstname" />
    <attribute name="lastname" />
    <attribute name="emailaddress1" />
    <filter type="and">
      <condition attribute="lastname" operator="eq" value="Smith" />
      <condition attribute="createdon" operator="on-or-after" value="2025-01-01" />
    </filter>
    <order attribute="createdon" descending="true" />
    <link-entity name="account" from="accountid" to="parentcustomerid" alias="a" link-type="outer">
      <attribute name="name" />
      <filter type="and">
        <condition attribute="name" operator="like" value="Contoso%" />
      </filter>
    </link-entity>
  </entity>
</fetch>
"@

Get-DataverseRecord -Connection $c -FetchXml $fetchXml
```

Example: aggregation (grouping / SUM) — fetch returns aliased aggregate columns that you can inspect in the returned objects:

```powershell
$fetchXmlAgg = @"
<fetch aggregate="true">
  <entity name="opportunity">
    <attribute name="estimatedvalue" aggregate="sum" alias="totalvalue" />
    <attribute name="ownerid" groupby="true" alias="owner" />
  </entity>
</fetch>
"@

$results = Get-DataverseRecord -Connection $c -FetchXml $fetchXmlAgg
# Each result contains aliased aggregate columns (e.g. 'totalvalue') and grouped keys (e.g. 'owner')
```

Tip: if you prefer a QueryExpression object for programmatic manipulation you can convert FetchXML to a QueryExpression using the helper cmdlet and then pass that `QueryExpression` to specialized SDK request cmdlets that accept a `Query` parameter. For simple retrievals prefer [`Get-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) `-FetchXml` which returns PSObjects and handles paging for you.

### Querying with SQL

As an alternative to hashtable filters, QueryExpression or FetchXML, many Dataverse environments can be queried using a SQL-like syntax via the module's [`Invoke-DataverseSql`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseSql.md) cmdlet (powered by the MarkMpn.Sql4Cds engine). `Invoke-DataverseSql` executes the SQL you supply and returns PowerShell objects (one per returned row) with properties named after the returned columns.

Key behaviors and options:

- Parameter name: use `-Sql` (string) to supply the statement. The cmdlet does not accept `-Query` — use `-Sql`.
- Parameterization: use `-Parameters` (a hashtable or PSObject). When the cmdlet receives pipeline objects as `-Parameters`, it will execute the query once per input object using that object's properties as parameter values.
- Result shape: each row is emitted as a `PSObject` where column names become properties.
- DML and confirmations: the cmdlet supports `ShouldProcess` for DML operations and will prompt/obey `-WhatIf`/`-Confirm` when the SQL includes INSERT/UPDATE/DELETE operations. It also exposes verbose progress and informational messages during execution.
- Performance and control: you can tune execution with `-Timeout` (seconds, default 600), `-BatchSize`, and `-MaxDegreeOfParallelism` for large or parallel workloads.
- Behavior toggles: `-BypassCustomPluginExecution`, `-UseBulkDelete`, `-ReturnEntityReferenceAsGuid`, and `-UseLocalTimezone` expose Sql4Cds-specific behaviors (see docs).

Examples:

```powershell
# Simple SELECT returning PSObjects
Invoke-DataverseSql -Connection $c -Sql "SELECT TOP 100 fullname, emailaddress1 FROM contact WHERE statecode = 0 ORDER BY createdon DESC"

# Parameterised query (single execution)
@$params = @{ lastname = 'Wood' }
Invoke-DataverseSql -Connection $c -Sql "SELECT TOP 1 createdon FROM Contact WHERE lastname=@lastname" -Parameters $params

# Pipeline parameterisation: execute a parameterised query once per input object
@(@{ lastname = 'Wood' }, @{ lastname = 'Cat2' }) | Invoke-DataverseSql -Connection $c -Sql "SELECT TOP 1 lastname, createdon FROM Contact WHERE lastname=@lastname"

# Return lookup values as GUIDs
Invoke-DataverseSql -Connection $c -Sql "SELECT TOP 10 parentcustomerid FROM contact" -ReturnEntityReferenceAsGuid
```

See the `Invoke-DataverseSql` cmdlet documentation for full parameter details and examples: [Invoke-DataverseSql](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseSql.md).

When to prefer which approach:
- Use the module's concise hashtable filters (`-FilterValues` / `-Criteria` / `-Links`) with [`Get-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) for most interactive scripts — you get PSObjects, automatic paging and convenient conversion.
- Use `-FetchXml` when you need aggregates, complex grouping, or you want to reuse FetchXML authored in other tools (Power Apps, Advanced Find).
- Use `QueryExpression` when you need to construct queries programmatically with fine-grained SDK control (for example custom paging strategies, programmatic link-entity construction, or integration with other SDK requests). Pass the resulting `QueryExpression` to appropriate SDK request cmdlets that accept a `Query` parameter when needed.

#### Specialized Invoke-Dataverse* Cmdlets

The module includes many specialized `Invoke-Dataverse*` cmdlets that wrap specific Dataverse SDK requests. These cmdlets provide direct access to platform operations like bulk operations, metadata queries, and administrative tasks.

##### How to Find and Use Specialized Cmdlets

To see all available specialized cmdlets:
```powershell
Get-Command -Module Rnwood.Dataverse.Data.PowerShell -Name "Invoke-Dataverse*"
```

Common categories include:
- **Bulk Operations**: [`Invoke-DataverseBulkDelete`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseBulkDelete.md), [`Invoke-DataverseBulkDetectDuplicates`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseBulkDetectDuplicates.md)
- **Metadata**: [`Invoke-DataverseRetrieveEntity`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseRetrieveEntity.md), [`Invoke-DataverseRetrieveAllEntities`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseRetrieveAllEntities.md)
- **Security**: [`Invoke-DataverseAddPrincipalToQueue`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseAddPrincipalToQueue.md), [`Invoke-DataverseAssign`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseAssign.md)
- **Campaigns**: [`Invoke-DataverseAddItemCampaign`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseAddItemCampaign.md), [`Invoke-DataverseAddMembersTeam`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseAddMembersTeam.md)
- **And many more...**

##### Usage Pattern

Most specialized cmdlets follow this pattern:
1. Accept a `-Connection` parameter
2. Take specific parameters for the operation
3. Return structured results as PSObjects

Example: Retrieve entity metadata
```powershell
$metadata = Invoke-DataverseRetrieveEntity -Connection $c -LogicalName contact
$metadata.Attributes | Where-Object { $_.IsPrimaryId } | Select-Object LogicalName
```

Example: Bulk delete records
```powershell
# Define criteria for bulk delete
$criteria = New-Object Microsoft.Xrm.Sdk.Query.QueryExpression('contact')
$criteria.Criteria.AddCondition('createdon', [Microsoft.Xrm.Sdk.Query.ConditionOperator]::LessThan, [datetime]::Parse('2024-01-01'))

# Execute bulk delete
$result = Invoke-DataverseBulkDelete -Connection $c -Query $criteria -JobName "OldContactsCleanup" -SendEmailNotification $false
```

##### When to Use Specialized Cmdlets

Use specialized cmdlets when:
- You need operations not covered by the core CRUD cmdlets ([`Get-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md), [`Set-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md), [`Remove-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseRecord.md))
- Performing bulk operations that affect many records
- Working with metadata, security, or administrative tasks
- You have existing SDK code or FetchXML that you want to reuse

For detailed documentation on each cmdlet, use `Get-Help` or refer to the docs folder.

### Getting total record count

If you need to know how many records exist (for reporting or planning queries) use the dedicated SDK request cmdlet which is far more efficient than retrieving all records and counting them locally.

Example: retrieve total record counts for one or more entities (returns a response containing an EntityRecordCountCollection):

```powershell
$response = Invoke-DataverseRetrieveTotalRecordCount -Connection $c -EntityNames contact,account

# Enumerate results
$response.EntityRecordCountCollection | ForEach-Object { "$($_.Key): $($_.Value)" }

# Get count for a single entity (if present)
$response.EntityRecordCountCollection | Where-Object { $_.Key -eq 'contact' } | ForEach-Object { $($_.Value) }
```

Notes:
- The request uses the platform RetrieveTotalRecordCount API and returns counts for the entities you request (counts are typically for the last 24 hours for partitioned data sources depending on the platform behavior).
- Use this when you only need counts rather than full record data — it avoids the network and memory cost of retrieving every record.

#### Using -TotalRecordCount switch

[`Get-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) supports the `-TotalRecordCount` switch parameter, which returns the total number of records matching your query (without retrieving all records). When used, the cmdlet outputs the count as an integer.

**Example:**
```powershell
$count = Get-DataverseRecord -Connection $c -TableName contact -TotalRecordCount
Write-Host "Total contacts: $count"
```

Notes:
- The switch works with all supported query types (`-TableName`, `-FilterValues`, `-FetchXml`, etc.).
- No record data is returned—only the count.
- Unlike `Invoke-DataverseRetrieveTotalRecordCount`, this method pages through all matching records to get an exact, real-time count. This is less performant for large tables, but provides up-to-date results.
- Use when you need the current count for reporting or planning queries.
- For large tables, prefer `Invoke-DataverseRetrieveTotalRecordCount` for faster, approximate counts unless you require real-time accuracy.

#### Using -VerboseRecordCount switch

[`Get-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) supports the `-VerboseRecordCount` switch parameter, which provides detailed information about the number of records processed and returned during queries. This is useful for monitoring query statistics and understanding the performance characteristics of your queries.

**Example:**
```powershell
# Use verbose record count for detailed query statistics
Get-DataverseRecord -Connection $c -TableName contact -VerboseRecordCount
```

Notes:
- The switch provides detailed information about records processed and returned
- Useful for monitoring query statistics and performance analysis
- Can be combined with other parameters for comprehensive query insights

### Sorting records

Use the `-OrderBy` parameter on [`Get-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) to request sorting. Provide one or more attribute logical names separated by commas. Append a trailing `-` to an attribute name to request descending order for that attribute.

Examples:

```powershell
# Sort ascending by firstname
Get-DataverseRecord -Connection $c -TableName contact -OrderBy firstname

# Sort descending by firstname and get top 5
Get-DataverseRecord -Connection $c -TableName contact -OrderBy firstname- -Top 5

# Sort by firstname then lastname (lastname descending)
Get-DataverseRecord -Connection $c -TableName contact -OrderBy firstname, lastname-
```

Notes:
- Sorting is applied before paging and top limits, so `-OrderBy` with `-Top` returns the top N rows from the sorted result set.
- If you require complex ordering or computed sort keys use SQL (where supported) or FetchXML with explicit order clauses.

#### Linking Related Tables

The `-Links` parameter supports joining related tables in queries using a simplified hashtable syntax. This is useful for filtering or selecting data from related entities.

*Example: Get contacts and join to their parent account:*
```powershell
Get-DataverseRecord -Connection $c -TableName contact -Links @{
    'contact.accountid' = 'account.accountid'
}
```

*Example: Left outer join with an alias:*
```powershell
Get-DataverseRecord -Connection $c -TableName contact -Links @{
    'contact.accountid' = 'account.accountid'
    type = 'LeftOuter'
    alias = 'parentAccount'
}
```

*Example: Join with filter on linked entity (only include contacts from accounts starting with 'Contoso'):*
```powershell
Get-DataverseRecord -Connection $c -TableName contact -Links @{
    'contact.accountid' = 'account.accountid'
    filter = @{
        name = @{ operator = 'Like'; value = 'Contoso%' }
        statecode = @{ operator = 'Equal'; value = 0 }
    }
}
```

*Example: Multiple joins:*
```powershell
Get-DataverseRecord -Connection $c -TableName contact -Links @(
    @{ 'contact.accountid' = 'account.accountid'; type = 'LeftOuter' },
    @{ 'contact.ownerid' = 'systemuser.systemuserid' }
)
```

*Example: Nested/child joins using `links` key (join account then join account.owner to systemuser):*
```powershell
Get-DataverseRecord -Connection $c -TableName contact -Links @{
  'contact.accountid' = 'account.accountid'
  'links' = @(
    @{ 'account.ownerid' = 'systemuser.systemuserid'; type = 'LeftOuter'; alias = 'accountOwner' }
  )
}
```

Notes:
- The `links` key may be a single hashtable or an array of hashtables and will be applied as child joins to the linked entity.
- Child link hashtables support the same simplified keys as top-level link hashtables: `type`, `alias`, `filter`, and may themselves contain further `links` recursively.

The simplified syntax supports:
- **Link specification**: `'fromTable.fromAttribute' = 'toTable.toAttribute'`
- **type** (optional): `'Inner'` (default) or `'LeftOuter'`
- **alias** (optional): String alias for the linked entity
- **filter** (optional): Hashtable with filter conditions (same format as `-FilterValues`)

### Creating and Updating Records

Use [`Set-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md) to create new records or update existing ones. You can pass a single hashtable, a list of hashtables, or pipeline objects. Use `-PassThru` to return the created/updated records (including their Ids). The cmdlet expects each input object to expose properties whose names match the Dataverse logical names for the target table's columns — those properties are mapped to Dataverse attributes during conversion.

#### Input object shape and type conversion (create/update)

When converting your input object into a Dataverse Entity the cmdlet performs type-aware conversions for each property based on the table's metadata (see `DataverseEntityConverter`). The table below summarises accepted input formats and controls:

| Target column / type | Accepted input formats | Notes / controls |
|---|---|---|
| Lookup (EntityReference / Owner / Customer) | GUID, PSObject/hashtable with `Id` + `TableName`/`EntityName`/`LogicalName`, or unique name string | The cmdlet will attempt to resolve name strings to records. Use `-LookupColumns` (hashtable) to control which column is used for lookup resolution; prefer GUIDs for deterministic results. |
| OptionSet / Status / State | Label string (case-insensitive) or numeric value | Labels are resolved via metadata; numeric values accepted directly. Errors thrown if label/value not found. |
| MultiSelectPicklist | Array of label strings or numeric values | Each item is converted to an OptionSetValue for the platform. |
| Date/time | `DateTime` or parseable date string | Supplied DateTimes are interpreted with local semantics and converted to UTC for the server. Include `timezonecode` when needed for special entities. |
| Money | Numeric value or parseable string | Converted to SDK `Money` wrapper. |
| Numeric types (Integer, Decimal, Double, BigInt, Boolean) | numeric or parseable string | Converted to appropriate CLR type; empty values set to null. |
| PartyList | Collection of objects (activityparty rows) | Each item converted to an `activityparty` entity; input must be enumerable. |
| Id property | GUID or parseable GUID string | Used to target updates when present. |

Errors during conversion (for example ambiguous lookups, invalid option labels, malformed GUIDs or unparsable dates) raise a `FormatException` which the cmdlet surfaces as an error record that includes the original input object for easy correlation.

Guidance to avoid unexpected conversions:

- Prefer GUIDs for lookups when possible.
- Use `-LookupColumns` to control lookup resolution when names are not unique or you need to match on an alternate column (for example an external id).
- Use `-IgnoreProperties` to skip undesired properties on the input object.
- Use `-UpdateAllColumns` (requires `-Id`) to skip the per-record retrieve-and-compare step and send all supplied columns as the update payload (trades retrieval cost for potentially larger update payloads).

Conversion happens per input object (per record) as the pipeline is processed; when importing many records be mindful of per-record lookup queries and prefer batching, pre-querying existing Ids, or supplying deterministic identifiers to reduce network calls and improve throughput.

Basic examples:

```powershell
# Create a single contact
Set-DataverseRecord -Connection $c -TableName contact -InputObject @{ firstname = 'John'; lastname = 'Doe' } -CreateOnly

# Create and return the created record (capture Id)
$created = @{ name = 'Contoso Ltd' } | Set-DataverseRecord -Connection $c -TableName account -CreateOnly -PassThru
Write-Host "Created account with Id: $($created.Id)"

# Update an existing record by Id
Set-DataverseRecord -Connection $c -TableName contact -Id '00000000-0000-0000-0000-000000000000' -InputObject @{ description = 'Updated description' }

# Upsert using match-on (create if not exists, update if exists)
@{ fullname = 'Jane Smith'; emailaddress1 = 'jane.smith@contoso.com' } |
  Set-DataverseRecord -Connection $c -TableName contact -MatchOn fullname -PassThru
```

Key notes:

- Use `-CreateOnly` to prevent updates (fail if record exists).
- Use `-MatchOn` to upsert based on one or more fields.
- Use `-BatchSize` to control batching behavior when processing many records.

#### Advanced Set-DataverseRecord Parameters

The [`Set-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md) cmdlet supports several advanced parameters to fine-tune create/update/upsert behavior:

- `-NoUpdateColumns`: A list of column names to exclude from updates. Useful when you want to update only specific fields without affecting others.
- `-LookupColumns`: A hashtable specifying which columns to use for resolving lookup values when multiple options exist.
- `-MatchOn`: Specifies one or more fields to match against for upsert operations, allowing creation if no match exists or update if a match is found.
- `-Upsert`: Forces an upsert operation using alternate keys defined on the table, letting Dataverse decide whether to create or update.

Examples:

```powershell
# Exclude certain columns from updates
Set-DataverseRecord -Connection $c -TableName contact -Id '00000000-0000-0000-0000-000000000000' -InputObject @{
    firstname = 'Updated Name'
    description = 'Updated description'
} -NoUpdateColumns description

# Specify lookup resolution columns
Set-DataverseRecord -Connection $c -TableName contact -InputObject @{
    firstname = 'John'
    lastname = 'Doe'
    parentcustomerid = 'Contoso Ltd'
} -LookupColumns @{ parentcustomerid = 'name' }

# Upsert using specific match fields
Set-DataverseRecord -Connection $c -TableName contact -InputObject @{
    emailaddress1 = 'john.doe@contoso.com'
    firstname = 'John'
} -MatchOn emailaddress1

# Force upsert using alternate keys
Set-DataverseRecord -Connection $c -TableName contact -InputObject @{
    emailaddress1 = 'jane.smith@contoso.com'
    firstname = 'Jane'
} -Upsert
```

### Assigning records

If your input object contains an `ownerid` property the cmdlet will perform an assignment after the main create/update. `ownerid` accepts the same forms as other lookup inputs: a GUID, a PSObject/hashtable with `Id` and `TableName`/`LogicalName` (or an `EntityReference`), or a name string which the cmdlet will try to resolve. Assignments are executed with `AssignRequest` and are batched when `-BatchSize` &gt; 1.

### Setting state and status

To change a record's status include `statuscode` (and optionally `statecode`) on the input object. Both `statuscode` and `statecode` accept either the numeric value or the display label. If only `statuscode` is supplied the cmdlet will infer the matching `statecode` from the table metadata and then issue a `SetStateRequest` after the main create/update. State/status changes are executed separately and are batched when `-BatchSize` &gt; 1.

How the cmdlet decides whether to create, update or upsert:

- With the `-Upsert` switch:
  - The cmdlet issues an `UpsertRequest` so Dataverse decides whether to create or update based on alternate keys.
  - When using `-Upsert` you must provide a single `-MatchOn` list that matches an alternate key defined on the table (the cmdlet validates this).
  - `-NoCreate` and `-NoUpdate` are not supported together with `-Upsert`.

- With the `-CreateOnly` switch:
  - The cmdlet always attempts to create new records and does not check for an existing match. Use this when you know records do not already exist.

- With the `-NoUpdate` switch:
  - The cmdlet will check for an existing record but will not update it; it will only create new records when no match is found.

- With the `-NoCreate` switch:
  - The cmdlet will check for an existing record but will not create one; it will only update when a match is found.

- Default behaviour (no special switches):
  - The cmdlet checks for an existing record by:
    - Primary ID if provided via the `-Id` parameter or an `Id` property on the input object.
    - Each `-MatchOn` column set in order (if `-MatchOn` is specified).
  - If an existing record is found the cmdlet retrieves the current values and removes unchanged columns (via an internal comparison) before issuing an `UpdateRequest`; it skips the update entirely when no changes are detected. Note: this means that even when an `Id` is supplied the cmdlet will ordinarily perform a retrieve to fetch the existing record for comparison (see below for exceptions).
  - If no existing record is found the cmdlet issues a `CreateRequest`.

#### Alternate Keys Explanation

Alternate keys in Dataverse allow you to uniquely identify records using fields other than the primary key (Id). They are defined at the table level in Dataverse and enable efficient upsert operations.

When using `-Upsert` with [`Set-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md), the cmdlet leverages alternate keys to let Dataverse handle the create-or-update decision. You must specify a `-MatchOn` parameter that exactly matches one of the alternate keys defined on the table.

Example: If a `contact` table has an alternate key on `emailaddress1`, you can upsert using that field:

```powershell
Set-DataverseRecord -Connection $c -TableName contact -InputObject @{
    emailaddress1 = 'john.doe@contoso.com'
    firstname = 'John'
    lastname = 'Doe'
} -Upsert -MatchOn emailaddress1
```

Benefits of using alternate keys:
- Avoids duplicate records by ensuring uniqueness on specified fields
- Enables efficient bulk upserts without pre-checking existence
- Supports complex matching scenarios beyond simple Id-based operations

To define alternate keys, use the Dataverse table designer or SDK to create them on your tables.

Performance note (important):

- The retrieve-and-compare step is performed per input record and is not currently batched — this per-record lookup can be the primary performance cost when updating many existing records even if most end up unchanged.
- If you already have the record `Id` and want to avoid the per-record retrieval, use `-UpdateAllColumns` (requires `-Id`) to issue an update containing all supplied columns without first retrieving the existing record. This trades the retrieve cost for always sending the supplied columns in the update.
- If you know inputs are new records, use `-CreateOnly` to skip existence checks entirely — this is the fastest create path.
-- `-MatchOn` performs queries to find matches and so also incurs lookups; `-Upsert` issues a Dataverse `UpsertRequest` (a platform upsert) and avoids the cmdlet's explicit existence check, but requires a valid alternate key and has different semantics — test in your workload to compare performance.
- For large workloads prefer batching (`-BatchSize`) and consider pre-querying existing records in bulk (for example, retrieve all matching keys/Ids in a single query and set `Id` on your input objects) so you can send direct updates or create batches rather than relying on per-record existence checks.

### SQL alternative — Create / Update

For bulk or ad-hoc creations and updates you can use [`Invoke-DataverseSql`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseSql.md) with `INSERT` and `UPDATE` statements. This is useful when you need mutations expressed in SQL (for example bulk field fixes, mass updates derived from joins, or scripted migrations).

What to know:
- Use `-Sql` to supply the statement and `-Parameters` to pass values (or stream parameter objects via the pipeline so the statement runs once per input object).
- Control throughput with `-BatchSize` and `-MaxDegreeOfParallelism` for large workloads.
- By default platform business logic (plugins/workflows) runs; use `-BypassCustomPluginExecution` to skip custom plugins when appropriate.
- The cmdlet supports `ShouldProcess` for DML: `-WhatIf` and `-Confirm` work for safety.
- Use `-Timeout` to increase command timeout for long-running operations.

Examples:

```powershell
# Parameterised UPDATE run once
Invoke-DataverseSql -Connection $c -Sql "UPDATE Contact SET description = @desc WHERE contactid = @id" -Parameters @{ desc = 'Updated'; id = $guid } -WhatIf

# Pipeline-driven updates: run once per pipeline object
@(@{ id = $id1; desc = 'A' }, @{ id = $id2; desc = 'B' }) |
  Invoke-DataverseSql -Connection $c -Sql "UPDATE Contact SET description = @desc WHERE contactid = @id"

# INSERT with OUTPUT (returning created id where supported by engine)
Invoke-DataverseSql -Connection $c -Sql "INSERT INTO account (name) OUTPUT INSERTED.accountid AS AccountId VALUES(@name)" -Parameters @{ name = 'NewCo' }
```

Notes:
- Prefer the module's typed [`Set-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md) for typical create/update flows — it performs per-record conversion, lookup resolution and minimizes accidental data issues. Use SQL for controlled, bulk, or complex mutations where SQL expresses the operation more clearly or efficiently.

### Deleting Records

> [!WARNING]
> Deleting records is irreversible. Always preview deletions with `-WhatIf` and/or require confirmation with `-Confirm` when running destructive operations.

Use [`Remove-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseRecord.md) to delete records by Id or via the pipeline. `-WhatIf` and `-Confirm` are supported to preview or require confirmation.

Basic examples:

```powershell
# Delete a single record by Id
Remove-DataverseRecord -Connection $c -TableName contact -Id '00000000-0000-0000-0000-000000000000'

# Delete records returned from a query (prompt for confirmation)
Get-DataverseRecord -Connection $c -TableName contact -FilterValues @{ lastname = 'TestUser' } |
  Remove-DataverseRecord -Connection $c -Confirm

# Preview deletes with WhatIf
Get-DataverseRecord -Connection $c -TableName contact -FilterValues @{ lastname = 'TestUser' } |
  Remove-DataverseRecord -Connection $c -WhatIf

# Batch delete with a specific batch size
Get-DataverseRecord -Connection $c -TableName account -Top 500 |
  Remove-DataverseRecord -Connection $c -BatchSize 50 -WhatIf
```

When deleting many records, batching is used to improve performance. Errors are returned per-record so you can correlate failures to the original inputs.

#### Deleting only if the record still exists

- [`-IfExists`](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseRecord.md#-ifexists): Only attempts to delete the record if it exists, avoiding errors when the record may have already been deleted. As standard that's an error.

Examples:

```powershell
# Delete only if the record exists
Remove-DataverseRecord -Connection $c -TableName contact -Id '00000000-0000-0000-0000-000000000000' -IfExists
```

##### SQL alternative — Delete

You can perform deletes using [`Invoke-DataverseSql`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseSql.md) (DELETE statements). For large deletes consider `-UseBulkDelete`. DML via SQL honours `ShouldProcess` so `-WhatIf`/`-Confirm` are supported. Example:

```powershell
Invoke-DataverseSql -Connection $c -Sql "DELETE FROM Contact WHERE statuscode = 2"  -WhatIf
```

### Error Handling

When working with batch operations, errors don't stop processing - all records are attempted and errors are collected. You can correlate errors back to the specific input records that failed.

*Example: Handle errors in batch operations:*
```powershell
$records = @(
    @{ firstname = "John"; lastname = "Doe" }
    @{ firstname = "Jane"; lastname = "Smith" }
)

$errors = @()
$records | Set-DataverseRecord -Connection $c -TableName contact -CreateOnly `
    -ErrorVariable +errors -ErrorAction SilentlyContinue

# Process errors - each error's TargetObject contains the input that failed
foreach ($err in $errors) {
    Write-Host "Failed: $($err.TargetObject.firstname) $($err.TargetObject.lastname)"
    Write-Host "Error: $($err.Exception.Message)"
}
```

The `Exception.Message` contains full server response including ErrorCode, Message, TraceText, and InnerFault details for troubleshooting.

To stop on first error instead, use `-BatchSize 1` with `-ErrorAction Stop`.















### Solution Management

You can manage Dataverse solutions from this module. Prefer the high-level `Export-DataverseSolution` and `Import-DataverseSolution` cmdlets for common operations. For advanced control use the `Invoke-` variants (`Invoke-DataverseExportSolution`, `Invoke-DataverseExportSolutionAsync`, `Invoke-DataverseImportSolution`, `Invoke-DataverseImportSolutionAsync`) documented in the `docs/` folder.

#### Exporting solutions

- `Export-DataverseSolution` exports a solution and can save it to disk or output to the pipeline. It supports including solution settings and reports progress for long-running exports.

Examples:

```powershell
# Export unmanaged solution to file
Export-DataverseSolution -Connection $c -SolutionName "MySolution" -OutFile "C:\Exports\MySolution.zip"

# Export managed solution and capture bytes
$b = Export-DataverseSolution -Connection $c -SolutionName "MySolution" -Managed -PassThru
[System.IO.File]::WriteAllBytes("C:\Exports\MySolution_managed.zip", $b)
```

#### Importing solutions

- `Import-DataverseSolution` imports a solution file with intelligent by default logic. By default, it automatically determines the best import method:
  - If the solution doesn't exist, performs a regular import
  - If the solution exists and is managed, performs a stage-and-upgrade operation
  - If the solution exists and is unmanaged, performs a regular import (upgrade)
- Use `-Mode NoUpgrade` to force a regular import regardless of solution status
- Use `-Mode StageAndUpgrade` to explicitly perform a stage-and-upgrade operation
- Use `-Mode HoldingSolution` to import as a holding solution for upgrade
- See the full parameter reference: [Import-DataverseSolution](Rnwood.Dataverse.Data.PowerShell/docs/Import-DataverseSolution.md).

Examples:

```powershell
# Intelligent import (default behavior - automatically chooses best method)
Import-DataverseSolution -Connection $c -InFile "C:\Solutions\MySolution.zip"

# Force regular import (no upgrade logic)
Import-DataverseSolution -Connection $c -InFile "C:\Solutions\MySolution.zip" -Mode NoUpgrade

# Explicitly perform stage-and-upgrade
Import-DataverseSolution -Connection $c -InFile "C:\Solutions\MySolution.zip" -Mode StageAndUpgrade

# Import as holding solution
Import-DataverseSolution -Connection $c -InFile "C:\Solutions\MySolution.zip" -Mode HoldingSolution

# Import from bytes instead of file
Import-DataverseSolution -Connection $c -SolutionBytes $bytes
```



Examples:

```powershell
# Import and overwrite unmanaged customisations, then publish included workflows
Invoke-DataverseImportSolution -Connection $c -InFile "C:\Solutions\MySolution.zip" -OverwriteUnmanagedCustomizations -PublishWorkflows

# Import as a holding solution for staged upgrade (unless the solution is not already present, when it will just be imported)
Invoke-DataverseImportSolution -Connection $c -InFile "C:\Solutions\MySolution.zip" -HoldingSolution $true
```

##### Handling Connection References and Environment Variables

When importing solutions that contain connection references (for API connections) or environment variables (custom settings), you must provide values for these components unless they already exist in the target environment with values set. The cmdlet validates this by default to prevent unexpected behaviour of your solution after import if these values are missing.

**Connection References:**
- You must supply the connection ID (GUID) for each connection reference schema name.
- If not provided and not already configured in the environment, the import will fail.

**Environment Variables:**
- You must supply the value for each environment variable schema name.
- If not provided and not already set in the environment, the import will fail.

Examples:

```powershell
# Import with connection references and environment variables
Import-DataverseSolution -Connection $c -InFile "C:\Solutions\MySolution.zip" `
    -ConnectionReferences @{
        'new_sharepointconnection' = '12345678-1234-1234-1234-123456789012'
        'new_sqlconnection' = '87654321-4321-4321-4321-210987654321'
    } `
    -EnvironmentVariables @{
        'new_apiurl' = 'https://api.production.example.com'
        'new_apikey' = 'prod-key-12345'
    }

# Skip validation if you want to ignore for some reason
Import-DataverseSolution -Connection $c -InFile "C:\Solutions\MySolution.zip" `
    -SkipConnectionReferenceValidation -SkipEnvironmentVariableValidation
```

**Notes:**
- Connection reference values must be valid connection IDs from the target environment which the user importing the solution has access to.
- Environment variable values are strings that will be set during import.


