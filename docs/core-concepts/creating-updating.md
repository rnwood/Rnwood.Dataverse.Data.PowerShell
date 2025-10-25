<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
## Table of Contents

- [Creating and Updating Records](#creating-and-updating-records)
      - [Input object shape and type conversion (create/update)](#input-object-shape-and-type-conversion-createupdate)
      - [Advanced Set-DataverseRecord Parameters](#advanced-set-dataverserecord-parameters)
    - [Assigning records](#assigning-records)
    - [Setting state and status](#setting-state-and-status)
      - [Alternate Keys Explanation](#alternate-keys-explanation)
    - [SQL alternative — Create / Update](#sql-alternative--create--update)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

# Creating and Updating Records

<!-- TOC -->
    - [Input object shape and type conversion (create/update)](#input-object-shape-and-type-conversion-createupdate)
    - [Advanced Set-DataverseRecord Parameters](#advanced-set-dataverserecord-parameters)
  - [Assigning records](#assigning-records)
  - [Setting state and status](#setting-state-and-status)
    - [Alternate Keys Explanation](#alternate-keys-explanation)
  - [SQL alternative — Create / Update](#sql-alternative-create-update)
<!-- /TOC -->


Use [`Set-DataverseRecord`](../../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md) to create new records or update existing ones. You can pass a single hashtable, a list of hashtables, or pipeline objects. Use `-PassThru` to return the created/updated records (including their Ids). The cmdlet expects each input object to expose properties whose names match the Dataverse logical names for the target table's columns — those properties are mapped to Dataverse attributes during conversion.

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

The [`Set-DataverseRecord`](../../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md) cmdlet supports several advanced parameters to fine-tune create/update/upsert behavior:

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
When using `-Upsert` with [`Set-DataverseRecord`](../../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md), the cmdlet leverages alternate keys to let Dataverse handle the create-or-update decision. You must specify a `-MatchOn` parameter that exactly matches one of the alternate keys defined on the table.
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

For bulk or ad-hoc creations and updates you can use [`Invoke-DataverseSql`](../../Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseSql.md) with `INSERT` and `UPDATE` statements. This is useful when you need mutations expressed in SQL (for example bulk field fixes, mass updates derived from joins, or scripted migrations).

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
- Prefer the module's typed [`Set-DataverseRecord`](../../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md) for typical create/update flows — it performs per-record conversion, lookup resolution and minimizes accidental data issues. Use SQL for controlled, bulk, or complex mutations where SQL expresses the operation more clearly or efficiently.

