 
# Rnwood.Dataverse.Data.PowerShell

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

- [Specialized Invoke-Dataverse* Cmdlets](#specialized-invoke-dataverse-cmdlets)
  - [How to Find and Use Specialized Cmdlets](#how-to-find-and-use-specialized-cmdlets)
  - [Usage Pattern](#usage-pattern)
  - [When to Use Specialized Cmdlets](#when-to-use-specialized-cmdlets)

- [Using PowerShell Standard Features](#using-powershell-standard-features)
  - [Pipeline vs ForEach-Object](#pipeline-vs-foreach-object)
  - [Parallelising work for best performance](#parallelising-work-for-best-performance)
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

##### Device Code
Authentication via device code flow (good for remote/headless scenarios).

*Example: Using device code authentication:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -devicecode
```

##### Username/Password
Basic credential authentication.

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

### SQL alternative — Delete

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

See full documentation: [Set-DataverseRecord Error Handling](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md#example-12-handle-errors-in-batch-operations) | [Remove-DataverseRecord Error Handling](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseRecord.md#example-3-handle-errors-in-batch-delete-operations)

### Getting IDs of Created Records

Use the `-PassThru` parameter to get the IDs of newly created records, which is useful for linking records or tracking what was created.

*Example: Capture IDs of created records:*
```powershell
$contacts = @(
    @{ firstname = "John"; lastname = "Doe" }
    @{ firstname = "Jane"; lastname = "Smith" }
)

$created = $contacts | Set-DataverseRecord -Connection $c -TableName contact -CreateOnly -PassThru

foreach ($record in $created) {
    Write-Host "Created: $($record.firstname) $($record.lastname) with ID: $($record.Id)"
}
```

*Example: Link records using PassThru:*
```powershell
# Create parent and capture its ID
$account = @{ name = "Contoso Ltd" } | 
    Set-DataverseRecord -Connection $c -TableName account -CreateOnly -PassThru

# Create child linked to parent
$contact = @{ 
    firstname = "John"
    lastname = "Doe"
    parentcustomerid = $account.Id
} | Set-DataverseRecord -Connection $c -TableName contact -CreateOnly -PassThru
```

See full documentation: [Set-DataverseRecord PassThru Examples](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md#example-15-get-ids-of-created-records-using-passthru)

### Batch Operations

By default, [`Set-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md) and [`Remove-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseRecord.md) automatically batch operations when processing multiple records (default batch size is 100). This improves performance by reducing round trips to the server.

Key behaviors:

- Batching uses `ExecuteMultipleRequest` with `ContinueOnError = true` - all records are attempted even if some fail
- Errors include the original input object as `TargetObject` for correlation
- Use `-BatchSize 1` to disable batching and stop on first error
- Use `-BatchSize <number>` to control batch size for performance tuning

*Example: Control batch size:*
```powershell
# Create 500 records in batches of 50
$records = 1..500 | ForEach-Object {
    @{ name = "Account $_" }
}

$records | Set-DataverseRecord -Connection $c -TableName account -BatchSize 50 -CreateOnly
```

See full documentation: [Set-DataverseRecord Batching](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md#example-7-control-batch-size) | [Remove-DataverseRecord Batching](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseRecord.md)

 





## Migration from Microsoft.Xrm.Data.PowerShell

If you're migrating from `Microsoft.Xrm.Data.PowerShell`, see the [Examples Comparison Guide](Examples-Comparison.md) which shows side-by-side examples of common operations in both modules.

 
## Using PowerShell Standard Features

This module follows PowerShell conventions and supports all standard PowerShell features. Here's how to use them effectively with Dataverse operations.

### Pipeline vs ForEach-Object

PowerShell cmdlets in this module support the **pipeline**, which is the idiomatic and recommended way to process multiple records. Understanding how to use the pipeline effectively is key to writing efficient PowerShell scripts.

#### Understanding the Pipeline

When you pipe objects from one cmdlet to another, PowerShell **streams** the objects one at a time. This is memory-efficient and allows processing to begin immediately without waiting for all records to be retrieved.

*Example: Using the pipeline to process records as they're retrieved:*
```powershell
# Good: Pipeline streams records one at a time
Get-DataverseRecord -Connection $c -TableName contact |
  Where-Object { $_.emailaddress1 -like "*@contoso.com" } |
  Set-DataverseRecord -Connection $c
```

The pipeline example works because [`Get-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) outputs PowerShell objects (PSObjects) where each property corresponds to a field from the Dataverse record. These objects flow through the pipeline to [`Set-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md), which accepts them as input via the `-InputObject` parameter. The `Where-Object` in the middle is able to filter them using these properties.

Each object emitted by [`Get-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) has properties `Id` and `TableName`. This is why we don't need to specify those parameters. Each record received through the pipeline will set them.

#### Pipeline vs ForEach-Object vs foreach Loop

**Pipeline (Recommended for most use cases):**
- ✅ **Streams** data - processes records as they arrive
- ✅ Memory efficient - doesn't load all records into memory at once
- ✅ Starts processing immediately
- ✅ Works with large datasets
- ✅ Can be interrupted with Ctrl+C

*Example: Pipeline processing:*
```powershell
# Pipeline: Streams each record through the pipeline
Get-DataverseRecord -Connection $c -TableName contact -Top 1000 |
  ForEach-Object { 
    Write-Host "Processing: $($_.fullname)"
    $_ 
  } |
  Set-DataverseRecord -Connection $c
```

**foreach statement (Use when you need all data in memory):**
- ❌ **Fetches all** items before processing starts
- ❌ Loads entire collection into memory
- ❌ Must wait for all records to be retrieved
- ✅ Allows random access to collection
- ✅ Supports break/continue

*Example: foreach statement:*
```powershell
# foreach: Loads ALL 1000 records into memory first
$contacts = Get-DataverseRecord -Connection $c -TableName contact -Top 1000
foreach ($contact in $contacts) {
  Write-Host "Processing: $($contact.fullname)"
  Set-DataverseRecord -Connection $c -InputObject $contact
}
```

**ForEach-Object cmdlet (Hybrid approach):**
- ✅ Works in the pipeline (streams data)
- ✅ Memory efficient
- ✅ Access to `-Begin`, `-Process`, `-End` blocks
- ✅ Can use `$_` automatic variable

#### Seeing Which Parameters Accept Pipeline Input

To see which parameters accept pipeline input, use `Get-Help`:

```powershell
# See full help including parameter details
Get-Help Set-DataverseRecord -Full

# Look for "Accept pipeline input: True" in parameter descriptions
Get-Help Set-DataverseRecord -Parameter InputObject
```

You can also check the parameter attributes in the help:
```powershell
(Get-Command Set-DataverseRecord).Parameters['InputObject'].Attributes
```

#### Best Practices

**✅ DO:**
- Use the pipeline for processing multiple records - it's memory efficient and idiomatic PowerShell
- Use `ForEach-Object` when you need setup/cleanup (`-Begin`/`-End` blocks)
- Check which parameters accept pipeline input with `Get-Help -Full`

**❌ DON'T:**
- Use foreach statement unless you need all data in memory or random access
- Collect all records into a variable before processing unless necessary
- Use `.ForEach()` method when pipeline would work (it also loads all into memory)

**Learn more:**
- [Microsoft Docs: about_Pipelines](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_pipelines)
- [Microsoft Docs: about_Foreach](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_foreach)
- [Microsoft Docs: ForEach-Object](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/foreach-object)
- [PowerShell Team Blog: ForEach and Where Magic Methods](https://devblogs.microsoft.com/powershell/foreach-and-where-magic-methods/)

### Parallelising work for best performance

When processing many records you can use parallelism to reduce elapsed time. Use parallelism when network latency or per-request processing dominates total time, but be careful to avoid overwhelming the Dataverse service (throttling).

**Recommended: Use [`Invoke-DataverseParallel`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseParallel.md)** — This module provides a built-in cmdlet that handles connection cloning, chunking, and parallel execution for you. It works on both PowerShell 5.1 and PowerShell 7+.

Example with `Invoke-DataverseParallel`:

```powershell
$connection = Get-DataverseConnection -url 'https://myorg.crm.dynamics.com' -ClientId $env:CLIENT_ID -ClientSecret $env:CLIENT_SECRET

# Get records and update them in parallel
Get-DataverseRecord -Connection $connection -TableName contact -Top 1000 |
  Invoke-DataverseParallel -Connection $connection -ChunkSize 50 -MaxDegreeOfParallelism 8 -ScriptBlock {
    # $_ is the current record
    # Cloned connection is automatically available as default
    $_.emailaddress1 = "updated-$($_.contactid)@example.com"
    Set-DataverseRecord -TableName contact -InputObject $_ -UpdateOnly
  }
```

**Alternative: PowerShell 7+ built-in parallelism** — For advanced scenarios or if you need more control, you can use `ForEach-Object -Parallel` or `Start-ThreadJob`. See the official docs for details:

- ForEach-Object (`-Parallel`) — https://learn.microsoft.com/powershell/module/microsoft.powershell.core/foreach-object?view=powershell-7.5
- Parallel execution guidance and comparisons — https://learn.microsoft.com/powershell/scripting/dev-cross-plat/performance/parallel-execution

Key guidance:
- Split work into chunks that align with `-BatchSize` — process a chunk per worker rather than single records. Chunking reduces round trips, bounds memory use, and reduces per-item overhead in parallel scenarios.
- Prefer streaming via the pipeline (e.g. [`Get-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) | Get-Chunks -ChunkSize n | ...) and avoid collecting the entire result set into an intermediate variable before processing.
- Clone an existing connection inside each runspace using the connection's `.Clone()` method rather than sharing a single connection object between runspaces. Cloning preserves authentication/context while providing an independent connection instance.
- Use `-ThrottleLimit` (or equivalent) to limit concurrent requests and avoid API throttling.
- Combine parallel processing with `-BatchSize` on [`Set-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md) / [`Remove-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseRecord.md) to reduce round-trips.

Example: chunking helper and `ForEach-Object -Parallel`. Create a parent connection, split records into chunks that match your batch size, then process one chunk per worker.

```powershell
# Helper: split an array into chunks
function Get-Chunks {
  [CmdletBinding()]
  param(
    [Parameter(ValueFromPipeline=$true, Position=0)]
    $InputObject,
    [Parameter(Mandatory, Position=1)]
    [int] $ChunkSize
  )

  begin { $buffer = @() }
  process {
    $buffer += $InputObject
    if ($buffer.Count -ge $ChunkSize) {
      # Emit the chunk as a single array object
      Write-Output (,$buffer)
      $buffer = @()
    }
  }
  end {
    if ($buffer.Count -gt 0) { Write-Output (,$buffer) }
  }
}

# Create a parent connection and determine chunk size
$parentConn = Get-DataverseConnection -url 'https://myorg.crm.dynamics.com' -ClientId $env:CLIENT_ID -ClientSecret $env:CLIENT_SECRET -TenantId $env:TENANT_ID
$chunkSize = 50

# Stream records, chunk them and process each chunk in parallel (no intermediate record/chunk variables)
Get-DataverseRecord -Connection $parentConn -TableName contact -Top 1000 |
  Get-Chunks -ChunkSize $chunkSize |
  ForEach-Object -Parallel {
    $ErrorActionPreference = 'Stop'
    $conn = $using:parentConn.Clone()
    $_ | Set-DataverseRecord -Connection $conn -BatchSize $using:chunkSize -Verbose
  } -ThrottleLimit 8
```


Example: use thread jobs for background tasks — create chunks first and pass chunk size into each job so jobs process chunked batches:

```powershell
$parent = Get-DataverseConnection -url 'https://myorg.crm.dynamics.com' -ClientId $env:CLIENT_ID -ClientSecret $env:CLIENT_SECRET -TenantId $env:TENANT_ID
$chunkSize = 50

# Stream records, create thread jobs per chunk and collect jobs in $jobs
$jobs = Get-DataverseRecord -Connection $parent -TableName contact -Top 1000 |
  Get-Chunks -ChunkSize $chunkSize |
  ForEach-Object {
    Start-ThreadJob -ArgumentList $_, $parent, $chunkSize -ScriptBlock {
      param($items, $parentConn, $size)
      # Clone the parent connection inside the job
      $conn = $parentConn.Clone()
      $items | Set-DataverseRecord -Connection $conn -BatchSize $size
    }
  }

# Wait and collect results
Receive-Job -Job $jobs -Wait -AutoRemoveJob
```

When to avoid parallelism:
- Small numbers of records where the overhead of cloning connections outweighs gains
- Operations that must be strictly ordered or transactional


### WhatIf and Confirm

The `-WhatIf` parameter lets you preview what would happen without actually making changes. The `-Confirm` parameter prompts for confirmation before each operation.

**Supported cmdlets:** [`Set-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md), [`Remove-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseRecord.md), and all `Invoke-Dataverse*` cmdlets that modify data.

*Example: Preview record creation with -WhatIf:*
```powershell
# See what would be created without actually creating it
Set-DataverseRecord -Connection $c -TableName contact -InputObject @{
    firstname = "John"
    lastname = "Doe"
} -CreateOnly -WhatIf
```

*Example: Get confirmation prompt before deleting:*
```powershell
# Prompt for confirmation before each delete
Get-DataverseRecord -Connection $c -TableName contact -FilterValues @{ lastname = "TestUser" } |
    Remove-DataverseRecord -Connection $c -Confirm
```

*Example: Batch operations with WhatIf:*
```powershell
# Preview all updates that would be made
$records = Get-DataverseRecord -Connection $c -TableName account -Top 10
$records | ForEach-Object { $_.name = "$($_.name) - Updated" }
$records | Set-DataverseRecord -Connection $c -WhatIf
```

**Learn more:**
- [Microsoft Docs: about_CommonParameters](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_commonparameters)
- [Microsoft Docs: Supporting -WhatIf and -Confirm](https://learn.microsoft.com/powershell/scripting/developer/cmdlet/how-to-request-confirmations)

### Verbose Output

Use the `-Verbose` parameter to see detailed information about what the cmdlet is doing. This is especially useful for troubleshooting and understanding complex operations.

*Example: See detailed information during record creation:*
```powershell
Set-DataverseRecord -Connection $c -TableName contact -InputObject @{
    firstname = "Jane"
    lastname = "Smith"
} -CreateOnly -Verbose
```

*Example: Monitor batch operations with verbose output:*
```powershell
# See progress as records are processed in batches
$records = 1..50 | ForEach-Object { @{ name = "Account $_" } }
$records | Set-DataverseRecord -Connection $c -TableName account -BatchSize 10 -Verbose
```

**Learn more:**
- [Microsoft Docs: Write-Verbose](https://learn.microsoft.com/powershell/module/microsoft.powershell.utility/write-verbose)
- [Microsoft Docs: about_CommonParameters](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_commonparameters)

### Error Handling

PowerShell provides powerful error handling through `-ErrorAction`, `-ErrorVariable`, and the `$ErrorActionPreference` preference variable.

*Example: Stop on first error:*
```powershell
# Stop immediately if any error occurs
Set-DataverseRecord -Connection $c -TableName contact -InputObject @{
    firstname = "Test"
} -ErrorAction Stop
```

*Example: Continue on errors and collect them:*
```powershell
# Continue processing and collect errors for review
$errors = @()
$records = Get-DataverseRecord -Connection $c -TableName contact -Top 10
$records | Set-DataverseRecord -Connection $c -ErrorVariable +errors -ErrorAction SilentlyContinue

# Review what failed
foreach ($err in $errors) {
    Write-Host "Failed: $($err.TargetObject.Id) - $($err.Exception.Message)"
}
```

*Example: Use Try-Catch with Stop:*
```powershell
try {
    Set-DataverseRecord -Connection $c -TableName contact -InputObject @{
        firstname = "John"
        lastname = "Doe"
    } -CreateOnly -ErrorAction Stop
    Write-Host "Record created successfully"
} catch {
    Write-Host "Failed to create record: $_"
}
```

**Learn more:**
- [Microsoft Docs: about_CommonParameters](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_commonparameters)
- [Microsoft Docs: about_Preference_Variables](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_preference_variables)
- [Microsoft Docs: about_Try_Catch_Finally](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_try_catch_finally)

### Warning Messages

Use `-WarningAction` to control how warning messages are handled.

*Example: Suppress warnings:*
```powershell
# Run without showing warnings
Get-DataverseRecord -Connection $c -TableName contact -WarningAction SilentlyContinue
```

*Example: Treat warnings as errors:*
```powershell
# Stop execution if a warning occurs
Get-DataverseRecord -Connection $c -TableName contact -WarningAction Stop
```

**Learn more:**
- [Microsoft Docs: about_CommonParameters](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_commonparameters)
- [Microsoft Docs: Write-Warning](https://learn.microsoft.com/powershell/module/microsoft.powershell.utility/write-warning)

#### $InformationPreference

Controls when informational messages are displayed (messages from `Write-Information`). Use `Write-Information` in scripts to emit contextual informational messages that can be controlled by consumers.

```powershell
# Show informational messages
$InformationPreference = "Continue"

# Hide informational messages (default)
$InformationPreference = "SilentlyContinue"
```

**Useful for:** Emitting structured informational messages that can be enabled during debugging or hidden in production.

#### $ProgressPreference

Controls whether progress bars are displayed. Hiding progress can improve script performance in non-interactive scenarios.

```powershell
# Hide progress bars (improves performance in scripts)
$ProgressPreference = "SilentlyContinue"

# Show progress bars (default)
$ProgressPreference = "Continue"
```

**Useful for:** Speeding up scripts by eliminating progress bar rendering overhead, especially in CI/CD pipelines.

### Tab Completion

PowerShell provides intelligent tab completion for cmdlet names, parameter names, and even parameter values in many cases.

**How to use:**
- Type a few letters of a cmdlet name and press `Tab` to cycle through matching cmdlets
- Type a parameter name (with `-`) and press `Tab` to complete it
- Press `Tab` after `-TableName` to see available table names (when connected)
- Press `Ctrl+Space` to see all available completions

*Example workflow:*
```powershell
# Type "Get-Dat" and press Tab -> completes to "Get-DataverseRecord"
# Add "-Tab" and press Tab -> completes to "-TableName"
# Add "-Col" and press Tab -> completes to "-Columns"

Get-DataverseRecord -Connection $c -TableName contact -Columns firstname, lastname
```

**Learn more:**
- [Microsoft Docs: about_Tab_Expansion](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_tab_expansion)
- [Microsoft Docs: TabCompletion](https://learn.microsoft.com/powershell/scripting/learn/shell/tab-completion)

### Contextual Help

PowerShell provides built-in help that you can access without leaving the command line.

**Get help for a cmdlet:**
```powershell
# Basic help
Get-Help Get-DataverseRecord

# Detailed help with examples
Get-Help Get-DataverseRecord -Detailed

# Full help including technical details
Get-Help Get-DataverseRecord -Full

# Just show examples
Get-Help Get-DataverseRecord -Examples

# Online version (opens in browser)
Get-Help Get-DataverseRecord -Online
```

**Use F1 for instant help:**
- Type a cmdlet name and press `F1` to open help
- Complete a parameter name and press `F1` to jump to that parameter's documentation

**Search for help topics:**
```powershell
# Find all cmdlets in this module
Get-Command -Module Rnwood.Dataverse.Data.PowerShell

# Search help for keywords
Get-Help *Dataverse* 

# Find cmdlets that work with records
Get-Command *Record* -Module Rnwood.Dataverse.Data.PowerShell
```

**Learn more:**
- [Microsoft Docs: Get-Help](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/get-help)
- [Microsoft Docs: about_Comment_Based_Help](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_comment_based_help)
- [Microsoft Docs: Updatable Help System](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_updatable_help)

### Command History

PowerShell keeps a history of commands you've run, making it easy to recall and modify them.

**Basic history navigation:**
- Press `↑` (up arrow) to recall previous commands
- Press `↓` (down arrow) to move forward through history
- Press `F8` after typing a few letters to search history for matching commands

**View and search history:**
```powershell
# View recent command history
Get-History

# Get a specific command by ID
Get-History -Id 10

# Search for commands containing "Dataverse"
Get-History | Where-Object { $_.CommandLine -like "*Dataverse*" }

# Re-run a command by ID
Invoke-History -Id 10
```

**Advanced history with PSReadLine (PowerShell 5.1+ / Core):**
- Press `Ctrl+R` for interactive reverse search
- Type to search, press `Ctrl+R` again to see older matches
- Press `Enter` to execute or `Esc` to edit

**Save and load history:**
```powershell
# Save history to a file
Get-History | Export-Clixml -Path ./dataverse-history.xml

# Load history from a file
Import-Clixml -Path ./dataverse-history.xml | Add-History
```

**Learn more:**
- [Microsoft Docs: about_History](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_history)
- [Microsoft Docs: PSReadLine Module](https://learn.microsoft.com/powershell/module/psreadline/)
- [GitHub: PSReadLine Key Bindings](https://github.com/PowerShell/PSReadLine#usage)

### Additional Resources

**General PowerShell Learning:**
- [Microsoft Learn: PowerShell Scripting](https://learn.microsoft.com/powershell/scripting/overview)
- [PowerShell Documentation](https://learn.microsoft.com/powershell/)
- [PowerShell Gallery: Find More Modules](https://www.powershellgallery.com/)

**Video Tutorials:**
- [Microsoft Learn: PowerShell for Beginners](https://learn.microsoft.com/shows/powershell-for-beginners/)
- [PowerShell.org: YouTube Channel](https://www.youtube.com/powershellorg)

**Community and Blogs:**
- [PowerShell.org Community](https://powershell.org/)
- [PowerShell Team Blog](https://devblogs.microsoft.com/powershell/)
- [Reddit: r/PowerShell](https://www.reddit.com/r/PowerShell/)

# Full Cmdlet Documentation
You can see documentation using the standard PowerShell help and autocompletion systems.

To see a complete list of cmdlets:
```powershell
get-command -Module Rnwood.Dataverse.Data.PowerShell
```

To see the help for an individual cmdlet (e.g `get-dataverseconnection`):
```powershell
get-help get-dataverseconnection -detailed
```

You can also simply enter the name of the cmdlet at the PS prompt and then press `F1`. 

Pressing `tab` completes parameter names (press again to see more suggestions), and you can press `F1` to jump to the help for those one you have the complete name.

[You can also view the documentation for the latest development version here](Rnwood.Dataverse.Data.PowerShell/docs). Note that this may not match the version you are running. Use the above preferably to get the correct and matching help for the version you are running.

## FAQ
### Why another module? What's wrong with `Microsoft.Xrm.Data.PowerShell`?
`Microsoft.Xrm.Data.PowerShell` is a popular module by a Microsoft employee - note that is it not official or supported by MS, although it is released under the open source MS-PL license.

It has the following drawbacks/limitations:
- It is only supported on the .NET Framework, not .NET Core and .NET 5.0 onwards. This means officially it works on Windows only and in PowerShell Desktop not PowerShell Core. For example you can not use it on Linux-based CI agents.
  **`Rnwood.Dataverse.Data.PowerShell` works on both .NET Framework and .NET Core and so will run cross-platform.**
- It is a thin wrapper around the Dataverse SDK and doesn't use idiomatic PowerShell conventions. This makes it harder to use. For example the caller is expected to understand and implement paging or it might get incomplete results.
  **`Rnwood.Dataverse.Data.PowerShell` tries to fit in with PowerShell conventions to make it easier to write scripts. `WhatIf`, `Confirm` and `Verbose` are implemented. Paging is automatic.

### Why not just script calls to the REST API?
Basic operations are easy using the REST API, but there are many features that are not straightforward to implement, or you might forget to handle (e.g. result paging). This module should be simpler to consume then the REST API directly.

This module also emits and consumes typed values (dates, numbers) instead of just strings. Doing this makes it easier to work with other PowerShell commands and modules.


## Common Use-Cases

This section describes common real-world scenarios where the module is typically used. It groups practical examples and guidance for automation, deployments, and other operational tasks so you can find the recommended patterns quickly.

### Using in CI/CD Pipelines

This module can be used in CI/CD pipelines for automated deployments and data operations. Here's how to use it in Azure DevOps and GitHub Actions.

#### Azure DevOps Pipelines

**Prerequisites:**
- Install the module in your pipeline
- Use service principal (client secret) authentication
- Store credentials in Azure DevOps Variable Groups or Key Vault

*Example: Azure Pipeline YAML with secure variables:*
```yaml
# azure-pipelines.yml
trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'  # or 'windows-latest' for PowerShell Desktop

variables:
  - group: 'dataverse-dev'  # Variable group containing DATAVERSE_URL, CLIENT_ID, CLIENT_SECRET, TENANT_ID

steps:
  - task: PowerShell@2
    displayName: 'Install Dataverse Module'
    inputs:
      targetType: 'inline'
      script: |
        Install-Module -Name Rnwood.Dataverse.Data.PowerShell -Force -Scope CurrentUser
        
  - task: PowerShell@2
    displayName: 'Run Dataverse Operations'
    inputs:
      targetType: 'inline'
      script: |
        $ErrorActionPreference = "Stop"
        
        # Connect using service principal from pipeline variables
        $c = Get-DataverseConnection `
          -url "$(DATAVERSE_URL)" `
          -ClientId "$(CLIENT_ID)" `
          -ClientSecret "$(CLIENT_SECRET)" `
          -TenantId "$(TENANT_ID)"
        
        # Your operations here
        $contacts = Get-DataverseRecord -Connection $c -TableName contact -Top 10
        Write-Host "Retrieved $($contacts.Count) contacts"
        
        # Example: Update records
        $contacts | ForEach-Object {
          $_.description = "Updated by pipeline on $(Get-Date)"
          $_
        } | Set-DataverseRecord -Connection $c
    env:
      DATAVERSE_URL: $(DATAVERSE_URL)
      CLIENT_ID: $(CLIENT_ID)
      CLIENT_SECRET: $(CLIENT_SECRET)
      TENANT_ID: $(TENANT_ID)
```

**Setting up Variable Groups:**

1. In Azure DevOps, go to **Pipelines** > **Library**
2. Create a new **Variable Group** (e.g., `dataverse-dev`, `dataverse-prod`)
3. Add variables:
   - `DATAVERSE_URL`: Your Dataverse URL (e.g., `https://myorg.crm.dynamics.com`)
   - `CLIENT_ID`: Application (client) ID from Azure App Registration
   - `CLIENT_SECRET`: Client secret from Azure App Registration (**mark as secret** 🔒)
   - `TENANT_ID`: Azure AD tenant ID
4. Link the variable group to your pipeline

**For production environments:** Consider using [Azure Key Vault integration](https://learn.microsoft.com/azure/devops/pipelines/release/azure-key-vault) to store secrets securely.

**Learn more:**
- [Azure DevOps: Define variables](https://learn.microsoft.com/azure/devops/pipelines/process/variables)
- [Azure DevOps: Variable groups](https://learn.microsoft.com/azure/devops/pipelines/library/variable-groups)
- [Azure DevOps: Use Azure Key Vault secrets](https://learn.microsoft.com/azure/devops/pipelines/release/azure-key-vault)

#### GitHub Actions

**Prerequisites:**
- Install the module in your workflow
- Use service principal (client secret) authentication
- Store credentials in GitHub Secrets or Environment Secrets

*Example: GitHub Actions workflow with secure secrets:*
```yaml
# .github/workflows/dataverse-deploy.yml
name: Dataverse Operations

on:
  push:
    branches: [ main ]
  workflow_dispatch:

jobs:
  deploy:
    runs-on: ubuntu-latest  # or windows-latest for PowerShell Desktop
    
    # Use environment for environment-specific secrets
    environment: development
    
    steps:
      - name: Checkout code
        uses: actions/checkout@v4
        
      - name: Install Dataverse Module
        shell: pwsh
        run: |
          Install-Module -Name Rnwood.Dataverse.Data.PowerShell -Force -Scope CurrentUser
          
      - name: Run Dataverse Operations
        shell: pwsh
        env:
          DATAVERSE_URL: ${{ secrets.DATAVERSE_URL }}
          CLIENT_ID: ${{ secrets.CLIENT_ID }}
          CLIENT_SECRET: ${{ secrets.CLIENT_SECRET }}
          TENANT_ID: ${{ secrets.TENANT_ID }}
        run: |
          $ErrorActionPreference = "Stop"
          
          # Connect using service principal from secrets
          $c = Get-DataverseConnection `
            -url $env:DATAVERSE_URL `
            -ClientId $env:CLIENT_ID `
            -ClientSecret $env:CLIENT_SECRET `
            -TenantId $env:TENANT_ID
          
          # Your operations here
          $contacts = Get-DataverseRecord -Connection $c -TableName contact -Top 10
          Write-Host "Retrieved $($contacts.Count) contacts"
          
          # Example: Create/update records
          $contacts | ForEach-Object {
            $_.description = "Updated by GitHub Actions on $(Get-Date)"
            $_
          } | Set-DataverseRecord -Connection $c -WhatIf  # Remove -WhatIf when ready
```

**Setting up GitHub Secrets:**

1. Go to your repository **Settings** > **Secrets and variables** > **Actions**
2. Add **Repository secrets** or **Environment secrets**:
   - `DATAVERSE_URL`: Your Dataverse URL (e.g., `https://myorg.crm.dynamics.com`)
   - `CLIENT_ID`: Application (client) ID from Azure App Registration
   - `CLIENT_SECRET`: Client secret from Azure App Registration
   - `TENANT_ID`: Azure AD tenant ID

**Using Environments for multiple stages:**

```yaml
# Deploy to different environments
jobs:
  deploy-dev:
    runs-on: ubuntu-latest
    environment: development
    steps:
      # ... uses secrets.DATAVERSE_URL from development environment
      
  deploy-prod:
    runs-on: ubuntu-latest
    environment: production
    needs: deploy-dev  # Runs after dev deployment
    steps:
      # ... uses secrets.DATAVERSE_URL from production environment
```

**Learn more:**
- [GitHub Actions: Using secrets](https://docs.github.com/actions/security-guides/using-secrets-in-github-actions)
- [GitHub Actions: Using environments](https://docs.github.com/actions/deployment/targeting-different-environments/using-environments-for-deployment)
- [GitHub Actions: Workflow syntax](https://docs.github.com/actions/using-workflows/workflow-syntax-for-github-actions)

#### Azure App Registration Setup

Both Azure DevOps and GitHub Actions require an Azure App Registration for service principal authentication:

1. **Create App Registration:**
   - Go to [Azure Portal](https://portal.azure.com) > **Azure Active Directory** > **App registrations**
   - Click **New registration**, give it a name (e.g., `Dataverse-CI-CD`)
   - Click **Register**

2. **Create Client Secret:**
   - In your app registration, go to **Certificates & secrets**
   - Click **New client secret**, add a description, set expiration
   - **Copy the secret value immediately** (you can't see it again)

3. **Grant Permissions in Dataverse:**
  - Go to [Power Platform Admin Center](https://admin.powerplatform.microsoft.com/)
  - Select your environment > **Settings** > **Users + permissions** > **Application users**
  - Click **New app user**, select your app registration
  - Assign appropriate security role (e.g., System Administrator for full access)

> [!CAUTION]
> Prefer the principle of least privilege when assigning roles to application users. Grant only the minimum permissions required for the automation to function to reduce blast radius if credentials are compromised.

4. **Note down:**
   - Application (client) ID
   - Directory (tenant) ID  
   - Client secret value

**Learn more:**
- [Microsoft Docs: Register an application](https://learn.microsoft.com/power-apps/developer/data-platform/walkthrough-register-app-azure-active-directory)
- [Microsoft Docs: Application user authentication](https://learn.microsoft.com/power-apps/developer/data-platform/use-single-tenant-server-server-authentication)

## Exporting data to a file (JSON, CSV, XML, XLSX)

Common scenarios require exporting Dataverse data for reporting, backups, or offline analysis. Below are practical examples and tips for exporting to JSON, CSV, XML and Excel (XLSX).

- Prefer selecting only the columns you need with `-Columns` or `Select-Object` to reduce payload size and make output files easier to consume.
- For lookup or complex fields (EntityReference, OptionSet, Money, PartyList), select or project the sub-properties you want (for example `parentcustomerid.Name` or `statuscode` label) so files contain simple values.
- For very large datasets, export in chunks (see `Get-Chunks` helper above) to avoid high memory use.
General guidance:
- Prefer selecting only the columns you need with `-Columns` or `Select-Object` to reduce payload size and make output files easier to consume.
-- Before exporting, narrow the result set using the module's filtering options — see [Filtering with simple syntax](#filtering-with-simple-syntax) and the `-ExcludeFilterValues` examples (see [Excluding records](#excluding-records)).
- For lookup or complex fields (EntityReference, OptionSet, Money, PartyList), select or project the sub-properties you want (for example `parentcustomerid.Name` or `statuscode` label) so files contain simple values.
- For very large datasets, export in chunks (see `Get-Chunks` helper above) to avoid high memory use.

### Mapping and transforming columns
```powershell
# Rename columns and perform simple transformations with Select-Object calculated properties
Get-DataverseRecord -Connection $c -TableName contact -Columns firstname,lastname,birthdate,parentcustomerid,gendercode |
  Select-Object \
    @{Name='FullName';Expression={ "$($_.firstname) $($_.lastname)" }}, \
    @{Name='BirthDateIso';Expression={ if ($_.birthdate) { $_.birthdate.ToString('yyyy-MM-dd') } else { $null } }}, \
    @{Name='AccountName';Expression={ ($_.parentcustomerid -ne $null) ? $_.parentcustomerid.Name : $null }}, \
    @{Name='Gender';Expression={ $_.gendercode }} |
  Export-Csv -Path .\contacts_transformed.csv -NoTypeInformation -Encoding UTF8

# For JSON exports, map to friendlier property names then serialize
Get-DataverseRecord -Connection $c -TableName contact -Columns firstname,lastname,emailaddress1 |
  Select-Object @{Name='FullName';Expression={ "$($_.firstname) $($_.lastname)" }}, emailaddress1 |
  ConvertTo-Json -Depth 5 | Set-Content .\contacts_transformed.json -Encoding UTF8

# Streamed mapping for very large exports: transform per-record and write NDJSON
$out = '.\contacts_ndjson_transformed.jsonl'
Get-DataverseRecord -Connection $c -TableName contact -Columns firstname,lastname,emailaddress1 |
  ForEach-Object {
    $obj = [PSCustomObject]@{
      FullName = "$($_.firstname) $($_.lastname)"
      Email = $_.emailaddress1
    }
    $obj | ConvertTo-Json -Depth 5 -Compress | Add-Content -Path $out -Encoding UTF8
  }

# Option set / status mapping: if you need stable labels use a mapping table
$statusMap = @{ 0 = 'Active'; 1 = 'Inactive' }
Get-DataverseRecord -Connection $c -TableName contact -Columns statuscode |
  Select-Object @{Name='StatusLabel';Expression={ $statusMap[$_.statuscode] -or $_.statuscode }} |
  Export-Csv -Path .\contacts_status.csv -NoTypeInformation -Encoding UTF8
```
- Prefer selecting only the columns you need with `-Columns` or `Select-Object` to reduce payload size and make output files easier to consume.
- For lookup or complex fields (EntityReference, OptionSet, Money, PartyList), select or project the sub-properties you want (for example `parentcustomerid.Name` or `statuscode` label) so files contain simple values.
- For very large datasets, export in chunks (see `Get-Chunks` helper above) to avoid high memory use.

### JSON (small exports)
```powershell
# Small dataset: simple and human-readable
$contacts = Get-DataverseRecord -Connection $c -TableName contact -Top 1000 -Columns firstname,lastname,emailaddress1
$contacts | ConvertTo-Json -Depth 5 | Set-Content -Path .\contacts.json -Encoding UTF8
```

### JSON (large streaming)
```powershell
# Avoid loading all records into memory by streaming and writing a valid JSON array incrementally
$out = '.\contacts_large.json'
'[' | Set-Content -Path $out -Encoding UTF8
$first = $true
Get-DataverseRecord -Connection $c -TableName contact -Columns firstname,lastname,emailaddress1 |
  ForEach-Object {
    if (-not $first) { ',' | Add-Content -Path $out -Encoding UTF8 }
    ($_ | ConvertTo-Json -Depth 5 -Compress) | Add-Content -Path $out -Encoding UTF8
    $first = $false
  }
']' | Add-Content -Path $out -Encoding UTF8
```

### CSV
```powershell
# Basic CSV export (Excel-friendly):
Get-DataverseRecord -Connection $c -TableName contact -Columns firstname,lastname,emailaddress1 |
  Select-Object firstname,lastname,emailaddress1 |
  Export-Csv -Path .\contacts.csv -NoTypeInformation -Encoding UTF8

# Large CSV export (write in chunks using Get-Chunks helper to avoid high memory use):
$path = '.\contacts_big.csv'
if (Test-Path $path) { Remove-Item $path }
$first = $true
Get-DataverseRecord -Connection $c -TableName contact -Columns firstname,lastname,emailaddress1 |
  Get-Chunks -ChunkSize 1000 |
  ForEach-Object {
    if ($first) {
      $_ | Select-Object firstname,lastname,emailaddress1 | Export-Csv -Path $path -NoTypeInformation -Encoding UTF8
      $first = $false
    } else {
      $_ | Select-Object firstname,lastname,emailaddress1 | Export-Csv -Path $path -NoTypeInformation -Encoding UTF8 -Append
    }
  }
```

### XML
```powershell
# PowerShell-typed XML (round-trippable) - good for PowerShell consumers
Get-DataverseRecord -Connection $c -TableName contact -Top 500 |
  Export-Clixml -Path .\contacts.xml

# If you need a custom XML format, project the properties you want and use ConvertTo-Xml or a custom serializer.
```

### XLSX (Excel)
```powershell
# Use the popular ImportExcel module to write .xlsx files without needing Excel on the host
Install-Module -Name ImportExcel -Scope CurrentUser -Force

Get-DataverseRecord -Connection $c -TableName contact -Columns firstname,lastname,emailaddress1 |
  Select-Object firstname,lastname,emailaddress1 |
  Export-Excel -Path .\contacts.xlsx -AutoSize -WorksheetName 'Contacts'

# For very large exports, write in chunks and append rows (ImportExcel supports -Append):
$path = '.\contacts_large.xlsx'
Get-DataverseRecord -Connection $c -TableName contact -Columns firstname,lastname,emailaddress1 |
  Get-Chunks -ChunkSize 500 |
  ForEach-Object -Begin { $first = $true } -Process {
    if ($first) {
      $_ | Export-Excel -Path $path -WorksheetName 'Contacts' -AutoSize
      $first = $false
    } else {
      $_ | Export-Excel -Path $path -WorksheetName 'Contacts' -Append
    }
  }
```

### Other tips
- Encoding: prefer UTF8. If Excel on Windows shows garbled characters, try UTF8 with BOM or use ImportExcel which handles encoding well.
- Date/time: Export date/times as ISO strings (for CSV/JSON) or keep as native DateTime (for Clixml/Excel) so consuming tools parse them reliably.
- Lookups and choices: project friendly columns, for example:
  Get-DataverseRecord -Connection $c -TableName contact -Columns firstname,lastname,parentcustomerid,gendercode |
    Select-Object firstname,lastname,@{Name='AccountName';Expression={$_.parentcustomerid.Name}},@{Name='Gender';Expression={$_.gendercode}} |
    Export-Csv -Path .\contacts_mapped.csv -NoTypeInformation -Encoding UTF8

These patterns should cover the most common export scenarios for reporting, migration, or analysis.

### SQL Server
```powershell
# Option A: small-to-medium volume using SqlServer module (Invoke-Sqlcmd / Write-SqlTableData)
Install-Module -Name SqlServer -Scope CurrentUser -Force

# Query Dataverse and write rows into a SQL Server table (simple INSERT per-row via Write-SqlTableData)
$rows = Get-DataverseRecord -Connection $c -TableName contact -Columns firstname,lastname,emailaddress1

# Ensure table exists and has a matching schema. For small volumes, Write-SqlTableData is convenient.
$rows | Select-Object firstname,lastname,emailaddress1 |
  ForEach-Object {
    # Convert to a hashtable or DataRow-like object expected by Write-SqlTableData
    [PSCustomObject]@{ firstname=$_.firstname; lastname=$_.lastname; emailaddress1=$_.emailaddress1 } |
      Write-SqlTableData -ServerInstance 'sqlserver\instance' -Database 'MyDb' -SchemaName dbo -TableName Contacts
  }

# Option B: high-volume using SqlBulkCopy (fastest for large exports)
function ConvertTo-DataTable($objects) {
  $dt = New-Object System.Data.DataTable
  if ($objects.Count -eq 0) { return $dt }
  # create columns from first object properties
  $first = $objects[0]
  foreach ($prop in $first.PSObject.Properties.Name) { $null = $dt.Columns.Add($prop) }
  foreach ($obj in $objects) {
    $row = $dt.NewRow()
    foreach ($prop in $obj.PSObject.Properties.Name) { $row[$prop] = $obj.$prop }
    $dt.Rows.Add($row)
  }
  return $dt
}

$data = Get-DataverseRecord -Connection $c -TableName contact -Columns firstname,lastname,emailaddress1 | Select-Object firstname,lastname,emailaddress1
$dt = ConvertTo-DataTable $data

$bulkConn = 'Server=tcp:sqlserver.database.windows.net,1433;Initial Catalog=MyDb;User ID=sqluser;Password=Secret;'
[System.Reflection.Assembly]::LoadWithPartialName('System.Data') | Out-Null
$sqlConn = New-Object System.Data.SqlClient.SqlConnection $bulkConn
$sqlConn.Open()
$bulk = New-Object System.Data.SqlClient.SqlBulkCopy($sqlConn)
$bulk.DestinationTableName = 'dbo.Contacts'
$bulk.WriteToServer($dt)
$sqlConn.Close()
```

Notes:
- Ensure columns and data types in SQL Server match or are compatible with values exported from Dataverse.
- For deterministic mapping include primary keys (GUIDs) when possible so you can correlate Dataverse records to SQL rows.
- For complex transforms, perform them in PowerShell before writing to SQL (see "Mapping and transforming columns" above).


## Importing data from a file (JSON, CSV, XML, XLSX)

Importing data is equally common. This section shows safe, practical patterns to read files, map columns to Dataverse fields, handle lookups and choices, and import efficiently in bulk.

General guidance:
- Validate and preview data before writing to Dataverse (use `-WhatIf`, `-Top`, or import a small sample file first).
- Map column names from files to Dataverse logical names and normalise data types (dates, guids, numbers) before calling [`Set-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md).
- For lookups prefer providing the target record Id where possible. If using names, ensure uniqueness or narrow the import with `-MatchOn`/`-Upsert` patterns.
- Use `-BatchSize` to control batching and `Get-Chunks` helper for memory-friendly large imports.

### JSON (import)
```powershell
# Simple JSON import (small file):
$items = Get-Content -Path .\contacts.json -Raw | ConvertFrom-Json

# Map fields to Dataverse logical names and normalise types
$mapped = $items | ForEach-Object {
  [PSCustomObject]@{
    TableName = 'contact'
    firstname = $_.firstName
    lastname  = $_.lastName
    emailaddress1 = $_.email
    birthdate = if ($_.birthdate) { [datetime]$_.birthdate } else { $null }
  }
}

# Preview what would be created
$mapped | Set-DataverseRecord -Connection $c -CreateOnly -WhatIf

# Create records (batched)
$mapped | Set-DataverseRecord -Connection $c -CreateOnly -BatchSize 100 -Verbose
```

### JSON (NDJSON streaming import)
```powershell
# If you have a very large JSON file, use newline-delimited JSON (NDJSON) and stream it line-by-line
$path = '.\contacts_ndjson.jsonl'
Get-Content -Path $path -Encoding UTF8 |
  ForEach-Object -Begin { $buffer = @() } -Process {
    $obj = $_ | ConvertFrom-Json
    $buffer += [PSCustomObject]@{
      TableName = 'contact'
      firstname = $obj.firstName
      lastname = $obj.lastName
      emailaddress1 = $obj.email
    }
    if ($buffer.Count -ge 500) {
      $buffer | Set-DataverseRecord -Connection $c -CreateOnly -BatchSize 100
      $buffer = @()
    }
  } -End { if ($buffer.Count) { $buffer | Set-DataverseRecord -Connection $c -CreateOnly -BatchSize 100 } }
```

### CSV (import)
```powershell
# Basic CSV import (Excel-exported CSV):
$rows = Import-Csv -Path .\contacts.csv -Encoding UTF8

# Map and normalise columns (handle dates and lookup names)
$mapped = $rows | ForEach-Object {
  [PSCustomObject]@{
    TableName = 'contact'
    firstname = $_.FirstName
    lastname  = $_.LastName
    emailaddress1 = $_.Email
    parentcustomerid = if ($_.AccountName) { $_.AccountName } else { $null } # name lookup allowed if unique
    birthdate = if ($_.BirthDate) { [datetime]$_.BirthDate } else { $null }
  }
}

# Preview
$mapped | Select-Object -First 5 | Set-DataverseRecord -Connection $c -CreateOnly -WhatIf

# Import in bulk (stream via pipeline, chunk and batch)
$mapped | Get-Chunks -ChunkSize 500 | ForEach-Object {
  $_ | Set-DataverseRecord -Connection $c -CreateOnly -BatchSize 100
}

# Pipeline one-liner: read CSV and write to Dataverse in stream/batches
Import-Csv -Path .\contacts.csv -Encoding UTF8 |
  ForEach-Object { [PSCustomObject]@{ TableName='contact'; firstname=$_.FirstName; lastname=$_.LastName; emailaddress1=$_.Email } } |
  Get-Chunks -ChunkSize 500 |
  ForEach-Object { $_ | Set-DataverseRecord -Connection $c -CreateOnly -BatchSize 100 }
```

### XML (import)
```powershell
# If the file was created with Export-Clixml (round-trippable):
$objects = Import-Clixml -Path .\contacts.xml

# Map and import
$objects | ForEach-Object {
  # example: convert imported object to proper field names
  [PSCustomObject]@{
    TableName = 'contact'
    firstname = $_.firstname
    lastname  = $_.lastname
    emailaddress1 = $_.emailaddress1
  }
} | Set-DataverseRecord -Connection $c -CreateOnly -BatchSize 100

# For custom XML formats, use [xml] or Select-Xml to parse and map elements into objects as above.
```

### XLSX (import)
```powershell
# Use ImportExcel to read .xlsx files
Install-Module -Name ImportExcel -Scope CurrentUser -Force

$rows = Import-Excel -Path .\contacts.xlsx -WorksheetName 'Contacts'

# Map and normalise
$rows | ForEach-Object {
  [PSCustomObject]@{
    TableName = 'contact'
    firstname = $_.FirstName
    lastname  = $_.LastName
    emailaddress1 = $_.Email
    gendercode = $_.Gender  # OptionSet label or numeric value accepted
  }
} | Get-Chunks -ChunkSize 500 | ForEach-Object {
  $_ | Set-DataverseRecord -Connection $c -CreateOnly -BatchSize 100
}
```

Mapping lookups and choices
- Lookups: you can provide the lookup `Id` (GUID), a PSObject with `Id` and `LogicalName`, or a unique name string that the module will resolve. Prefer `Id` for deterministic results. Example:
  @{ parentcustomerid = 'Contoso Ltd' }            # name lookup (must be unique)
  @{ parentcustomerid = '00000000-0000-0000-0000-000000000000' }  # GUID
  @{ parentcustomerid = @{ Id = '00000000-0000-0000-0000-000000000000'; LogicalName = 'account' } }

- Choices / OptionSet fields: provide the label string or numeric value. The module will try to map label → value where possible.

Upsert / MatchOn patterns
- If you want to update existing records or upsert, use `-MatchOn` and/or `-Upsert` to avoid creating duplicates. Example (match on email):

Import-Csv -Path .\contacts.csv -Encoding UTF8 |
  ForEach-Object { [PSCustomObject]@{ TableName='contact'; firstname=$_.FirstName; lastname=$_.LastName; emailaddress1=$_.Email } } |
  Set-DataverseRecord -Connection $c -MatchOn emailaddress1 -PassThru -BatchSize 100

Performance and safety tips
- Use `-WhatIf` for a dry-run before performing destructive or large imports.
- For very large imports pre-query existing records to obtain Ids for updates (set the `Id` on input objects) to avoid per-record existence lookups inside the cmdlet.
- Capture errors with `-ErrorVariable` and `-ErrorAction` to review failed rows and retry selectively.
- To stop on the first error, use `-BatchSize 1` and `-ErrorAction Stop`.

Error handling example
```powershell
$errors = @()
Import-Csv -Path .\contacts.csv -Encoding UTF8 |
  ForEach-Object { [PSCustomObject]@{ TableName='contact'; firstname=$_.FirstName; lastname=$_.LastName; emailaddress1=$_.Email } } |
  Set-DataverseRecord -Connection $c -BatchSize 100 -ErrorVariable +errors -ErrorAction SilentlyContinue

foreach ($err in $errors) {
  Write-Host "Failed: $($err.TargetObject | ConvertTo-Json -Compress)"
  Write-Host "Error: $($err.Exception.Message)"
}
```

These import patterns should cover most day-to-day needs — small ad-hoc re-imports as well as large bulk migrations. When migrating critical or large datasets, practice in a sandbox first and prefer deterministic identifiers (GUIDs) and upsert patterns to avoid duplicates.

### SQL Server (import)
```powershell
# Option A: small-to-medium reads using SqlServer module
Install-Module -Name SqlServer -Scope CurrentUser -Force

# Read rows from SQL Server
$rows = Invoke-Sqlcmd -ServerInstance 'sqlserver\instance' -Database 'MyDb' -Query "SELECT FirstName, LastName, Email, AccountId FROM dbo.Contacts WHERE ModifiedOn > '2025-01-01'"

# Map rows to Dataverse field names and handle lookups (prefer AccountId as GUID if present)
$mapped = $rows | ForEach-Object {
  [PSCustomObject]@{
    TableName = 'contact'
    firstname = $_.FirstName
    lastname  = $_.LastName
    emailaddress1 = $_.Email
    parentcustomerid = if ($_.AccountId) { $_.AccountId } else { $_.AccountName }
  }
}

# Import (batched)
$mapped | Get-Chunks -ChunkSize 500 | ForEach-Object { $_ | Set-DataverseRecord -Connection $c -BatchSize 100 }

# Option B: very large reads - stream using .NET SqlDataReader to avoid loading entire table
function Stream-SqlRows($connectionString, $query, $processRow) {
  $conn = New-Object System.Data.SqlClient.SqlConnection $connectionString
  $cmd = $conn.CreateCommand()
  $cmd.CommandText = $query
  $conn.Open()
  $reader = $cmd.ExecuteReader()
  try {
    while ($reader.Read()) {
      $obj = [PSCustomObject]@{}
      for ($i=0; $i -lt $reader.FieldCount; $i++) { $obj | Add-Member -NotePropertyName $reader.GetName($i) -NotePropertyValue $reader.GetValue($i) }
      & $processRow $obj
    }
  } finally { $reader.Close(); $conn.Close() }
}

# Example usage: stream rows and import in small batches
$buffer = @()
Stream-SqlRows -connectionString 'Server=tcp:sqlserver.database.windows.net,1433;Initial Catalog=MyDb;User ID=sqluser;Password=Secret;' -query 'SELECT FirstName,LastName,Email FROM dbo.Contacts' -processRow {
  param($row)
  $buffer += [PSCustomObject]@{ TableName='contact'; firstname=$row.FirstName; lastname=$row.LastName; emailaddress1=$row.Email }
  if ($buffer.Count -ge 500) { $buffer | Set-DataverseRecord -Connection $c -BatchSize 100; $buffer = @() }
}
if ($buffer.Count) { $buffer | Set-DataverseRecord -Connection $c -BatchSize 100 }
```

Notes and safety:
- Prefer transferring GUID-based lookup IDs where possible to avoid name collisions. If only names are available, ensure uniqueness or use `-MatchOn`/`-Upsert` patterns to avoid duplicates.
- Network and DB credentials: use secure practices (Windows auth, managed identity, or store credentials in a secure store) rather than embedding passwords in scripts.
- For very large imports consider a staged approach: load into a staging table in Dataverse or SQL, validate, then upsert using `-MatchOn` or platform logic.


## Mass updating data

Mass updates are a common operational task — fixing a field across many records, applying a business-rule-driven change, or backfilling values. Choose the approach that balances safety, performance, and the need to execute platform business logic (plugins/workflows):

- For medium-to-large workloads consider using SQL ([`Invoke-DataverseSql`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseSql.md)) for bulk updates when appropriate. SQL can be high-performance, but the SQL engine may retrieve and process data locally and SQL statements bypass the module's conversion logic — be careful with types, lookups and side effects.
- Use [`Set-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md) when you need the module's typed conversions, lookup resolution, and to honour platform business logic by default.
- Use batching (`-BatchSize`) and chunking (`Get-Chunks`) to control memory and throughput.
- Always test changes in a sandbox and perform dry-runs with `-WhatIf` before applying destructive updates.
- Consider `-BypassCustomPluginExecution` if you intentionally want to skip custom plugins/workflows (use with extreme caution).

Common pattern (safe, auditable): query the rows you need, transform locally, then push updates in batches. This lets you preview changes and capture what changed for auditing:

```powershell
# 1) Query candidates
$candidates = Get-DataverseRecord -Connection $c -TableName contact -FilterValues @{ statuscode = 0 } -Columns contactid,description -Top 10000

# 2) Transform locally (preview)
$preview = $candidates | ForEach-Object {
  $_.description = "Updated on $(Get-Date -Format o)"
  $_
}

# Preview the first few changes
$preview | Select-Object -First 10 | Format-Table contactid,description

# 3) Apply in batches
$preview | Get-Chunks -ChunkSize 200 | ForEach-Object {
  $_ | Set-DataverseRecord -Connection $c -BatchSize 100 -Verbose -WhatIf
}
```

### Using SQL

Use [`Invoke-DataverseSql`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseSql.md) when you need high performance for mass updates. The SQL engine may translate and perform some work client-side; SQL updates are fast but bypass the module's conversion logic — be careful with types and lookups.

Example: parameterised update (dry-run using `-WhatIf` / review first):

```powershell
Invoke-DataverseSql -Connection $c -Sql "UPDATE Contact SET description = @desc WHERE createdon < @cutoff" -Parameters @{ desc = 'Archival note'; cutoff = '2024-01-01' } -WhatIf
```

Example: update in two phases (select ids, then update via SQL or Set-DataverseRecord):

```powershell
# Phase 1: select ids (safe to review)
$ids = Invoke-DataverseSql -Connection $c -Sql "SELECT contactid FROM Contact WHERE ..." | Select-Object -ExpandProperty contactid

# Phase 2a: update via SQL in batches
$ids | ForEach-Object -Begin { $batch = @() } -Process {
  $batch += $_
  if ($batch.Count -ge 500) {
    $sql = "UPDATE Contact SET description = 'Batch update' WHERE contactid IN ('" + ($batch -join "','") + "')"
    Invoke-DataverseSql -Connection $c -Sql $sql -Verbose
    $batch = @()
  }
} -End {
  if ($batch.Count) {
    $sql = "UPDATE Contact SET description = 'Batch update' WHERE contactid IN ('" + ($batch -join "','") + "')"
    Invoke-DataverseSql -Connection $c -Sql $sql -Verbose
  }
}
```

Notes on SQL updates:
- Parameterise where possible to avoid SQL injection and improve readability.
- Use `-Timeout`, `-BatchSize` and `-MaxDegreeOfParallelism` on [`Invoke-DataverseSql`](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseSql.md) to control execution for large workloads.
- SQL updates may still trigger platform business logic; test to confirm behaviour or use `-BypassCustomPluginExecution` when supported and appropriate.


## Managing data in source control

- [`Get-DataverseRecordsFolder`](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecordsFolder.md) and [`Set-DataverseRecordsFolder`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecordsFolder.md) are provided to read and write a folder of JSON records which is very suitable for source control — see `Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecordsFolder.md` and `Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecordsFolder.md`.

Example: apply a folder of data files during deployment (dry-run first). This reads files from the folder and pipes them into [`Set-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md) to apply them to Dataverse. Using `account` as an example table name:

```powershell
# Complete example: export (including deletions) and apply to a target environment

# 1) Export from a source environment into ./data/my_table and write a `deletions/` subfolder
## Create connections (example):
$sourceConn = Get-DataverseConnection -url 'https://source.crm11.dynamics.com' -interactive
#
# The `-withdeletions` switch causes `Set-DataverseRecordsFolder` to record any items
# that were present previously but are not present in the new export into
# `./data/my_table/deletions/` so they can be removed from target environments.
Get-DataverseRecord -Connection $sourceConn -TableName my_table -Columns name,accountnumber,telephone1 |
  Set-DataverseRecordsFolder -OutputPath ./data/my_table -withdeletions

Now commit your files to source control etc.

Then bring them back when you are ready to deploy.

# 2) Apply the exported files to the target environment
# Create or obtain a target connection just before applying to the target
$targetConn = Get-DataverseConnection -url 'https://target.crm11.dynamics.com' -interactive

Get-DataverseRecordsFolder -InputPath ./data/my_table | Set-DataverseRecord -Connection $targetConn -BatchSize 100 -Verbose
Get-DataverseRecordsFolder -InputPath ./data/my_table -deletions | Remove-DataverseRecord -Connection $targetConn -BatchSize 100 -Verbose
```

### Copying data between environments

A simple, safe pattern to copy a set of records from a source environment to a target environment. This example uses the folder export helpers so you can review files before applying them to the target.

# 1) Export from the source environment
$src = Get-DataverseConnection -url 'https://source.crm11.dynamics.com' -interactive
Get-DataverseRecord -Connection $src -TableName account -Columns name,accountnumber,parentcustomerid |
  Set-DataverseRecordsFolder -OutputPath ./exports/account -WithDeletions

# 2) Import to the target environment (preview first with -WhatIf)
$dst = Get-DataverseConnection -url 'https://target.crm11.dynamics.com' -interactive
Get-DataverseRecordsFolder -InputPath ./exports/account | Set-DataverseRecord -Connection $dst -BatchSize 100 -Verbose -WhatIf

# 3) Apply deletions recorded during export (preview with -WhatIf first)
Get-DataverseRecordsFolder -InputPath ./exports/account -deletions | Remove-DataverseRecord -Connection $dst -BatchSize 100 -Verbose -WhatIf

Notes:
- Run the import with `-WhatIf` first to preview changes.
- Prefer preserving GUIDs (include `Id` on exported records) or use `-MatchOn`/`-Upsert` to avoid creating duplicates when re-importing.
- Test the process in a sandbox before running against production.



