<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
## Table of Contents

- [Querying Records](#querying-records)
      - [Column output and type conversions](#column-output-and-type-conversions)
      - [Paging and Limiting How Many Records](#paging-and-limiting-how-many-records)
      - [Including and Excluding by Name or Id](#including-and-excluding-by-name-or-id)
      - [Filtering with simple syntax](#filtering-with-simple-syntax)
        - [Excluding records](#excluding-records)
      - [Advanced Filtering with QueryExpression](#advanced-filtering-with-queryexpression)
      - [Advanced Filtering with FetchXML](#advanced-filtering-with-fetchxml)
    - [Querying with SQL](#querying-with-sql)
      - [Using SDK Requests with Invoke-DataverseRequest](#using-sdk-requests-with-invoke-dataverserequest)
        - [Usage Pattern](#usage-pattern)
        - [When to Use SDK Requests](#when-to-use-sdk-requests)
    - [Getting total record count](#getting-total-record-count)
      - [Using -TotalRecordCount switch](#using--totalrecordcount-switch)
      - [Using -VerboseRecordCount switch](#using--verboserecordcount-switch)
    - [Sorting records](#sorting-records)
      - [Linking Related Tables](#linking-related-tables)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

# Querying Records

<!-- TOC -->
    - [Column output and type conversions](#column-output-and-type-conversions)
    - [Paging and Limiting How Many Records](#paging-and-limiting-how-many-records)
    - [Including and Excluding by Name or Id](#including-and-excluding-by-name-or-id)
    - [Filtering with simple syntax](#filtering-with-simple-syntax)
      - [Excluding records](#excluding-records)
    - [Advanced Filtering with QueryExpression](#advanced-filtering-with-queryexpression)
    - [Advanced Filtering with FetchXML](#advanced-filtering-with-fetchxml)
  - [Querying with SQL](#querying-with-sql)
    - [Specialized Invoke-Dataverse* Cmdlets](#specialized-invoke-dataverse-cmdlets)
      - [How to Find and Use Specialized Cmdlets](#how-to-find-and-use-specialized-cmdlets)
      - [Usage Pattern](#usage-pattern)
      - [When to Use Specialized Cmdlets](#when-to-use-specialized-cmdlets)
  - [Getting total record count](#getting-total-record-count)
    - [Using -TotalRecordCount switch](#using--totalrecordcount-switch)
    - [Using -VerboseRecordCount switch](#using--verboserecordcount-switch)
  - [Sorting records](#sorting-records)
    - [Linking Related Tables](#linking-related-tables)
<!-- /TOC -->

Use [`Get-DataverseRecord`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) to retrieve records from Dataverse. By default the cmdlet returns PowerShell objects that include `Id` and `TableName` properties, and can be piped directly to other cmdlets. Each returned object also contains a property for every column returned by the query (the property name equals the column's logical name).
#### Column output and type conversions
The cmdlet converts Dataverse attribute types to PowerShell-friendly values when building the output PSObject. The following table summarises the typical output and how to control it:
| Dataverse attribute | PowerShell output | How to control / notes |
|---|---|---|
| Any requested column | Appears as a property on the returned PSObject (property name = column logical name) | Use [`-Columns`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-columns) to limit returned columns; system columns excluded by default unless [`-IncludeSystemColumns`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-includesystemcolumns) is set. |
| Lookup (EntityReference / Owner / Customer) | Friendly name string by default, or a reference-like object when raw/display behaviour is changed | Use [`-LookupValuesReturnName`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-lookupvaluesreturnname) to prefer names globally; use per-column `ColumnName:Raw` or `ColumnName:Display` in `-Columns` to override. |
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
```

The cmdlet automatically pages results for you; use [`-Top`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-top) to limit the number of records returned when testing or when you only need a subset.

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
The [`Get-DataverseRecord`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) cmdlet supports parameters to control how many records are retrieved and how they are paged:
- [`-Top`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-top): Limits the total number of records returned. Useful for testing, sampling, or when you only need a subset of data.
- [`-PageSize`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-pagesize): Specifies the number of records to retrieve per page. By default, the cmdlet uses automatic paging to retrieve all matching records efficiently.

Examples:

```powershell
# Get the first 10 contacts
Get-DataverseRecord -Connection $c -TableName contact -Top 10

# Select specific columns and limit to 50 records
Get-DataverseRecord -Connection $c -TableName contact -Columns firstname,lastname,emailaddress1 -Top 50
# Retrieve records with a specific page size for controlled batching
Get-DataverseRecord -Connection $c -TableName contact -PageSize 500
```
The cmdlet automatically pages results for you; use [`-Top`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-top) to limit the number of records returned when testing or when you only need a subset.
#### Including and Excluding by Name or Id
You can include or exclude specific records by their primary key (Id) or primary attribute value (Name):
- [`-Id`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-id): List of primary keys (IDs) of records to retrieve.
- [`-Name`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-name): List of names (primary attribute value) of records to retrieve.
- [`-ExcludeId`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-excludeid): List of record ids to exclude.
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
> While building filters (the [`-FilterValues`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-filtervalues) hashtables) you can use completion to help with parameter names and column names — press `Tab` to cycle, `Ctrl+Space` to show the full completion list, or `F1` for contextual help. See [Tab Completion](#tab-completion).
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
Use [`-ExcludeFilterValues`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-excludefiltervalues) to remove matching records from the result set. It accepts the same hashtable-based syntax as [`-FilterValues`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-filtervalues) (including grouping keys and explicit operator hashtables).
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
- When both [`-FilterValues`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-filtervalues) and [`-ExcludeFilterValues`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-excludefiltervalues) are provided the include filters are applied first to select candidate records and the exclude filters are then applied to remove matches from that set.
- [`-ExcludeFilterValues`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-excludefiltervalues) supports grouping and operators just like [`-FilterValues`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-filtervalues) — for example `-ExcludeFilterValues @{'or'=@(@{a=1},@{b=2})}`.

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

When you need programmatic control over complex query logic (dynamic construction, complex joins, or paging control) you can build a QueryExpression (and related SDK types) from PowerShell and execute it directly. For most interactive scenarios prefer the higher-level `-Criteria` and `-Links` parameters on [`Get-DataverseRecord`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) which accept `FilterExpression` and `DataverseLinkEntity` values and return friendly PowerShell objects.

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

Example: build a QueryExpression object when you need programmatic construction of a complex query. QueryExpression objects are useful as inputs to SDK requests that accept a `Query` parameter (for example bulk-detect / bulk-delete style requests). For simple retrievals prefer [`Get-DataverseRecord`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) with `-Criteria`/`-Links` or `-FetchXml` which return friendly PSObjects and handle paging for you.

```powershell
# Build a QueryExpression for advanced programmatic scenarios
$qe = New-Object Microsoft.Xrm.Sdk.Query.QueryExpression('contact')
$qe.ColumnSet = New-Object Microsoft.Xrm.Sdk.Query.ColumnSet('firstname','lastname','emailaddress1')
$qe.Criteria.AddCondition('lastname',[Microsoft.Xrm.Sdk.Query.ConditionOperator]::Equal,'Smith')

# Pass $qe to SDK requests that accept a Query parameter when needed
# Example: BulkDeleteRequest
$request = New-Object Microsoft.Crm.Sdk.Messages.BulkDeleteRequest
$request.QuerySet = @($qe)
# ... set other required properties
$response = Invoke-DataverseRequest -Connection $c -Request $request
```

Notes:
- [`Get-DataverseRecord`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) returns friendly PSObjects and automatically pages results for you when using `-TableName`/`-Criteria`/`-Links` or `-FetchXml`.
- When you use low-level SDK requests that accept a `Query` you may need to manage paging explicitly depending on the request semantics.

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
Tip: if you prefer a QueryExpression object for programmatic manipulation you can convert FetchXML to a QueryExpression using the helper cmdlet and then pass that `QueryExpression` to SDK requests that accept a `Query` parameter. For simple retrievals prefer [`Get-DataverseRecord`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) `-FetchXml` which returns PSObjects and handles paging for you.
### Querying with SQL
As an alternative to hashtable filters, QueryExpression or FetchXML, many Dataverse environments can be queried using a SQL-like syntax via the module's [`Invoke-DataverseSql`](../../Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseSql.md) cmdlet (powered by the MarkMpn.Sql4Cds engine). `Invoke-DataverseSql` executes the SQL you supply and returns PowerShell objects (one per returned row) with properties named after the returned columns.
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
See the `Invoke-DataverseSql` cmdlet documentation for full parameter details and examples: [Invoke-DataverseSql](../../Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseSql.md).
When to prefer which approach:
- Use the module's concise hashtable filters (`-FilterValues` / `-Criteria` / `-Links`) with [`Get-DataverseRecord`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) for most interactive scripts — you get PSObjects, automatic paging and convenient conversion.
- Use `-FetchXml` when you need aggregates, complex grouping, or you want to reuse FetchXML authored in other tools (Power Apps, Advanced Find).
- Use `QueryExpression` when you need to construct queries programmatically with fine-grained SDK control (for example custom paging strategies, programmatic link-entity construction, or integration with other SDK requests). Pass the resulting `QueryExpression` to appropriate SDK requests when needed.

#### Using SDK Requests with Invoke-DataverseRequest

For operations not covered by the core cmdlets, use [`Invoke-DataverseRequest`](../../Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseRequest.md) with SDK request objects. This provides direct access to all Dataverse SDK operations like bulk operations, metadata queries, and administrative tasks.

##### Usage Pattern

1. Create an SDK request object using `New-Object`
2. Set the required properties on the request
3. Pass the request to `Invoke-DataverseRequest`
4. Access the response properties

**Example: Retrieve entity metadata**
```powershell
$request = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityRequest
$request.LogicalName = 'contact'
$request.EntityFilters = [Microsoft.Xrm.Sdk.Metadata.EntityFilters]::Attributes

$response = Invoke-DataverseRequest -Connection $c -Request $request
$response.EntityMetadata.Attributes | Where-Object { $_.IsPrimaryId } | Select-Object LogicalName
```

**Example: Bulk delete records**
```powershell
# Define criteria for bulk delete
$criteria = New-Object Microsoft.Xrm.Sdk.Query.QueryExpression('contact')
$criteria.Criteria.AddCondition('createdon', [Microsoft.Xrm.Sdk.Query.ConditionOperator]::LessThan, [datetime]::Parse('2024-01-01'))

# Create and execute bulk delete request
$request = New-Object Microsoft.Crm.Sdk.Messages.BulkDeleteRequest
$request.QuerySet = @($criteria)
$request.JobName = "OldContactsCleanup"
$request.SendEmailNotification = $false
$request.ToRecipients = @()
$request.CCRecipients = @()
$request.RecurrencePattern = ""
$request.StartDateTime = [DateTime]::Now

$response = Invoke-DataverseRequest -Connection $c -Request $request
Write-Host "Bulk delete job created with ID: $($response.JobId)"
```

##### When to Use SDK Requests

Use `Invoke-DataverseRequest` when:
- You need operations not covered by the core CRUD cmdlets ([`Get-DataverseRecord`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md), [`Set-DataverseRecord`](../../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md), [`Remove-DataverseRecord`](../../Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseRecord.md))
- Performing bulk operations that affect many records
- Working with metadata, security, or administrative tasks
- You have existing SDK code that you want to reuse

For a complete list of available SDK requests, see the [Microsoft Dataverse SDK documentation](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/org-service/overview).
### Getting total record count
If you need to know how many records exist (for reporting or planning queries), use the SDK `RetrieveTotalRecordCountRequest` with `Invoke-DataverseRequest`, which is far more efficient than retrieving all records and counting them locally.

Example: retrieve total record counts for one or more entities (returns a response containing an EntityRecordCountCollection):
```powershell
$request = New-Object Microsoft.Crm.Sdk.Messages.RetrieveTotalRecordCountRequest
$request.EntityNames = [System.Collections.Generic.List[string]]::new(@('contact', 'account'))

$response = Invoke-DataverseRequest -Connection $c -Request $request

# Enumerate results
$response.EntityRecordCountCollection | ForEach-Object { "$($_.Key): $($_.Value)" }
# Get count for a single entity (if present)
$response.EntityRecordCountCollection | Where-Object { $_.Key -eq 'contact' } | ForEach-Object { $($_.Value) }
```
Notes:
- The request uses the platform RetrieveTotalRecordCount API and returns counts for the entities you request (counts are typically for the last 24 hours for partitioned data sources depending on the platform behavior).
- Use this when you only need counts rather than full record data — it avoids the network and memory cost of retrieving every record.
#### Using -TotalRecordCount switch
[`Get-DataverseRecord`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) supports the `-TotalRecordCount` switch parameter, which returns the total number of records matching your query (without retrieving all records). When used, the cmdlet outputs the count as an integer.
**Example:**
```powershell
$count = Get-DataverseRecord -Connection $c -TableName contact -TotalRecordCount
Write-Host "Total contacts: $count"
```
Notes:
- The switch works with all supported query types (`-TableName`, `-FilterValues`, `-FetchXml`, etc.).
- No record data is returned—only the count.
- Unlike using `RetrieveTotalRecordCountRequest` with `Invoke-DataverseRequest`, this method pages through all matching records to get an exact, real-time count. This is less performant for large tables, but provides up-to-date results.
- Use when you need the current count for reporting or planning queries.
- For large tables, prefer `RetrieveTotalRecordCountRequest` for faster, approximate counts unless you require real-time accuracy.

#### Using -VerboseRecordCount switch

[`Get-DataverseRecord`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) supports the `-VerboseRecordCount` switch parameter, which provides detailed information about the number of records processed and returned during queries. This is useful for monitoring query statistics and understanding the performance characteristics of your queries.

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
Use the `-OrderBy` parameter on [`Get-DataverseRecord`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) to request sorting. Provide one or more attribute logical names separated by commas. Append a trailing `-` to an attribute name to request descending order for that attribute.
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
