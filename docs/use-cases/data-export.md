# Exporting Data to Files

Common scenarios require exporting Dataverse data for reporting, backups, or offline analysis. Below are practical examples and tips for exporting to JSON, CSV, XML, Excel (XLSX), and SQL Server.

## General Guidance

- Prefer selecting only the columns you need with `-Columns` or `Select-Object` to reduce payload size and make output files easier to consume.
- Before exporting, narrow the result set using the module's filtering options — see [Querying Records](../core-concepts/querying.md) for filtering guidance.
- For lookup or complex fields (EntityReference, OptionSet, Money, PartyList), select or project the sub-properties you want (for example `parentcustomerid.Name` or `statuscode` label) so files contain simple values.
- For very large datasets, export in chunks to avoid high memory use.

## Mapping and Transforming Columns

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

## JSON (Small Exports)

```powershell
# Small dataset: simple and human-readable
$contacts = Get-DataverseRecord -Connection $c -TableName contact -Top 1000 -Columns firstname,lastname,emailaddress1
$contacts | ConvertTo-Json -Depth 5 | Set-Content -Path .\contacts.json -Encoding UTF8
```

## JSON (Large Streaming)

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

## CSV

```powershell
# Basic CSV export (Excel-friendly):
Get-DataverseRecord -Connection $c -TableName contact -Columns firstname,lastname,emailaddress1 |
  Select-Object firstname,lastname,emailaddress1 |
  Export-Csv -Path .\contacts.csv -NoTypeInformation -Encoding UTF8

# Large CSV export (write in chunks to avoid high memory use):
$path = '.\contacts_big.csv'
if (Test-Path $path) { Remove-Item $path }
$first = $true
Get-DataverseRecord -Connection $c -TableName contact -Columns firstname,lastname,emailaddress1 |
  ForEach-Object -Begin { $count = 0; $chunk = @() } -Process {
    $chunk += $_
    $count++
    if ($count -ge 1000) {
      if ($first) {
        $chunk | Select-Object firstname,lastname,emailaddress1 | Export-Csv -Path $path -NoTypeInformation -Encoding UTF8
        $first = $false
      } else {
        $chunk | Select-Object firstname,lastname,emailaddress1 | Export-Csv -Path $path -NoTypeInformation -Encoding UTF8 -Append
      }
      $chunk = @()
      $count = 0
    }
  } -End {
    if ($chunk.Count -gt 0) {
      if ($first) {
        $chunk | Select-Object firstname,lastname,emailaddress1 | Export-Csv -Path $path -NoTypeInformation -Encoding UTF8
      } else {
        $chunk | Select-Object firstname,lastname,emailaddress1 | Export-Csv -Path $path -NoTypeInformation -Encoding UTF8 -Append
      }
    }
  }
```

## XML

```powershell
# PowerShell-typed XML (round-trippable) - good for PowerShell consumers
Get-DataverseRecord -Connection $c -TableName contact -Top 500 |
  Export-Clixml -Path .\contacts.xml

# If you need a custom XML format, project the properties you want and use ConvertTo-Xml or a custom serializer.
```

## XLSX (Excel)

```powershell
# Use the popular ImportExcel module to write .xlsx files without needing Excel on the host
Install-Module -Name ImportExcel -Scope CurrentUser -Force

Get-DataverseRecord -Connection $c -TableName contact -Columns firstname,lastname,emailaddress1 |
  Select-Object firstname,lastname,emailaddress1 |
  Export-Excel -Path .\contacts.xlsx -AutoSize -WorksheetName 'Contacts'

# For very large exports, write in chunks and append rows (ImportExcel supports -Append):
$path = '.\contacts_large.xlsx'
Get-DataverseRecord -Connection $c -TableName contact -Columns firstname,lastname,emailaddress1 |
  ForEach-Object -Begin { $count = 0; $chunk = @(); $first = $true } -Process {
    $chunk += $_
    $count++
    if ($count -ge 500) {
      if ($first) {
        $chunk | Export-Excel -Path $path -WorksheetName 'Contacts' -AutoSize
        $first = $false
      } else {
        $chunk | Export-Excel -Path $path -WorksheetName 'Contacts' -Append
      }
      $chunk = @()
      $count = 0
    }
  } -End {
    if ($chunk.Count -gt 0) {
      if ($first) {
        $chunk | Export-Excel -Path $path -WorksheetName 'Contacts' -AutoSize
      } else {
        $chunk | Export-Excel -Path $path -WorksheetName 'Contacts' -Append
      }
    }
  }
```

## SQL Server

```powershell
# Option A: small-to-medium volume using SqlServer module
Install-Module -Name SqlServer -Scope CurrentUser -Force

# Query Dataverse and write rows into a SQL Server table
$rows = Get-DataverseRecord -Connection $c -TableName contact -Columns firstname,lastname,emailaddress1

# Ensure table exists and has a matching schema
$rows | Select-Object firstname,lastname,emailaddress1 |
  ForEach-Object {
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

**Notes:**
- Ensure columns and data types in SQL Server match or are compatible with values exported from Dataverse.
- For deterministic mapping include primary keys (GUIDs) when possible so you can correlate Dataverse records to SQL rows.
- For complex transforms, perform them in PowerShell before writing to SQL.

## Additional Tips

- **Encoding**: Prefer UTF8. If Excel on Windows shows garbled characters, try UTF8 with BOM or use ImportExcel which handles encoding well.
- **Date/time**: Export date/times as ISO strings (for CSV/JSON) or keep as native DateTime (for Clixml/Excel) so consuming tools parse them reliably.
- **Lookups and choices**: Project friendly columns, for example:
  ```powershell
  Get-DataverseRecord -Connection $c -TableName contact -Columns firstname,lastname,parentcustomerid,gendercode |
    Select-Object firstname,lastname,@{Name='AccountName';Expression={$_.parentcustomerid.Name}},@{Name='Gender';Expression={$_.gendercode}} |
    Export-Csv -Path .\contacts_mapped.csv -NoTypeInformation -Encoding UTF8
  ```

## See Also

- [Data Import](data-import.md) - Import data from files
- [Querying Records](../core-concepts/querying.md) - Filtering and querying data
- [Source Control](source-control.md) - Manage data in source control
