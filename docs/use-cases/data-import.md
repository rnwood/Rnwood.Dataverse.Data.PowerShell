# Importing data from a file (JSON, CSV, XML, XLSX)

Importing data is equally common. This section shows safe, practical patterns to read files, map columns to Dataverse fields, handle lookups and choices, and import efficiently in bulk.

## General Guidance

- Validate and preview data before writing to Dataverse (use `-WhatIf`, `-Top`, or import a small sample file first).
- Map column names from files to Dataverse logical names and normalise data types (dates, guids, numbers) before calling [`Set-DataverseRecord`](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md).
- For lookups prefer providing the target record Id where possible. If using names, ensure uniqueness or narrow the import with `-MatchOn`/`-Upsert` patterns.
- Use `-BatchSize` to control batching for memory-friendly large imports.
- **For large-scale imports**, consider using `-MaxDegreeOfParallelism` to process records in parallel for improved performance. See [Parallelization](../advanced/parallelization.md) for detailed guidance.

## JSON (import)
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

## Parallelization for Large Imports

For importing large datasets (thousands to millions of records), parallelization can significantly reduce import time by processing multiple batches concurrently. The module supports built-in parallelization through the `-MaxDegreeOfParallelism` parameter.

*Example: Import large dataset with parallel processing:*
```powershell
# Import 100,000 contacts in parallel using 4 workers
$contacts = Get-Content -Path .\large-contacts.json -Raw | ConvertFrom-Json

$contacts | ForEach-Object {
  [PSCustomObject]@{
    firstname = $_.firstName
    lastname = $_.lastName
    emailaddress1 = $_.email
  }
} | Set-DataverseRecord -Connection $c -TableName contact -CreateOnly `
    -BatchSize 100 `
    -MaxDegreeOfParallelism 4 `
    -Verbose
```

**Key considerations for parallel imports:**
- Start with `-MaxDegreeOfParallelism 4` and adjust based on performance and throttling limits
- Combine with appropriate `-BatchSize` (typically 50-100) for optimal throughput
- Monitor for throttling errors and reduce parallelism if needed
- Use `-Retries` parameter to handle transient failures automatically

For complex multi-step import workflows, consider using `Invoke-DataverseParallel` for more control. See [Parallelization](../advanced/parallelization.md) for comprehensive guidance on parallel processing strategies, performance tuning, and best practices.

## See Also

- [Data Export](data-export.md) - Export data to various formats
- [Creating and Updating Records](../core-concepts/creating-updating.md) - Details on Set-DataverseRecord
- [Parallelization](../advanced/parallelization.md) - Parallel processing for best performance
- [Source Control](source-control.md) - Manage data in source control
