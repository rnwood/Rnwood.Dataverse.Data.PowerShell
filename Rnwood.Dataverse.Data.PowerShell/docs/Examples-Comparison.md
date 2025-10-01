# Rnwood.Dataverse.Data.PowerShell Examples

This document provides examples of common Dataverse operations using `Rnwood.Dataverse.Data.PowerShell`, showing how to accomplish tasks that were done with `Microsoft.Xrm.Data.PowerShell`.

## Table of Contents

- [Connection](#connection)
- [Basic CRUD Operations](#basic-crud-operations)
- [Querying Records](#querying-records)
- [Batch Operations](#batch-operations)
- [Working with Attachments](#working-with-attachments)
- [Invoking Custom Requests](#invoking-custom-requests)
- [Metadata Operations](#metadata-operations)

## Connection

### Example: Interactive Connection

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$conn = Connect-CrmOnlineDiscovery -InteractiveMode
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
$conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
```

### Example: Client Secret Connection

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Not directly supported - required manual SDK work
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
$conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -ClientId "your-client-id" -ClientSecret "your-client-secret"
```

### Example: Username/Password Connection

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$cred = Get-Credential
$conn = Connect-CrmOnline -ServerUrl "https://yourorg.crm.dynamics.com" -Credential $cred
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
$conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Username "user@domain.com" -Password "password"
```

## Basic CRUD Operations

### Example: Create a Record

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$account = New-CrmRecord -EntityLogicalName account -Fields @{
    "name" = "Contoso Ltd"
    "telephone1" = "555-1234"
}
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
$account = @{
    name = "Contoso Ltd"
    telephone1 = "555-1234"
}
$accountId = Set-DataverseRecord -Connection $conn -TableName account -Fields $account
```

### Example: Update a Record

**Microsoft.Xrm.Data.PowerShell:**
```powershell
Set-CrmRecord -EntityLogicalName account -Id $accountId -Fields @{
    "telephone1" = "555-5678"
}
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
Set-DataverseRecord -Connection $conn -TableName account -Id $accountId -Fields @{
    telephone1 = "555-5678"
}
```

### Example: Delete a Record

**Microsoft.Xrm.Data.PowerShell:**
```powershell
Remove-CrmRecord -EntityLogicalName account -Id $accountId
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
Remove-DataverseRecord -Connection $conn -TableName account -Id $accountId
```

### Example: Retrieve a Single Record

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$account = Get-CrmRecord -EntityLogicalName account -Id $accountId -Fields name,telephone1
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
$account = Get-DataverseRecord -Connection $conn -TableName account -Id $accountId -Columns name,telephone1
```

## Querying Records

### Example: Retrieve All Records of a Type

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Requires manual paging
$fetch = @"
<fetch>
  <entity name='account'>
    <attribute name='name' />
    <attribute name='telephone1' />
  </entity>
</fetch>
"@
$accounts = Get-CrmRecordsByFetch -conn $conn -Fetch $fetch
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Automatic paging
$accounts = Get-DataverseRecord -Connection $conn -TableName account -Columns name,telephone1
```

### Example: Query with Filter

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$fetch = @"
<fetch>
  <entity name='contact'>
    <attribute name='fullname' />
    <filter>
      <condition attribute='lastname' operator='eq' value='Smith' />
    </filter>
  </entity>
</fetch>
"@
$contacts = Get-CrmRecordsByFetch -conn $conn -Fetch $fetch
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Simple filter
$contacts = Get-DataverseRecord -Connection $conn -TableName contact -Columns fullname -Filter @{lastname = "Smith"}

# Or with FetchXML for complex queries
$fetchXml = @"
<fetch>
  <entity name='contact'>
    <attribute name='fullname' />
    <filter>
      <condition attribute='lastname' operator='eq' value='Smith' />
    </filter>
  </entity>
</fetch>
"@
$contacts = Get-DataverseRecord -Connection $conn -FetchXml $fetchXml
```

### Example: Count Records

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$count = Get-CrmRecordsCount -conn $conn -EntityLogicalName account
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
$count = Get-DataverseRecord -Connection $conn -TableName account -RecordCount
# Or using SQL
$result = Invoke-DataverseSql -Connection $conn -Sql "SELECT COUNT(*) as cnt FROM account"
$count = $result.cnt
```

### Example: Count Records for All Entities

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$conn = Connect-CrmOnlineDiscovery -InteractiveMode
$results = New-Object System.Collections.Generic.List[PSObject]
$entities = Get-CrmEntityAllMetadata -conn $conn -EntityFilters Entity

$entities | ForEach-Object {
    $logicalName = $_.LogicalName
    $count = Get-CrmRecordsCount -conn $conn -EntityLogicalName $logicalName
    $result = [PSCustomObject]@{
        LogicalName = $logicalName
        RecordsCount = $count
    }
    $results.Add($result)
}

$results | Sort-Object RecordsCount -Descending | Select-Object -First 10
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
$conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive

# Get all entities using SQL query
$tables = Invoke-DataverseSql -Connection $conn -Sql @"
SELECT name 
FROM metadata.entity 
WHERE isprivate = 0
"@

$results = @()
foreach ($table in $tables) {
    $tableName = $table.name
    try {
        $count = Get-DataverseRecord -Connection $conn -TableName $tableName -RecordCount
        $results += [PSCustomObject]@{
            LogicalName = $tableName
            RecordsCount = $count
        }
    }
    catch {
        Write-Warning "Could not query $tableName : $_"
    }
}

# Top 10 by count
$results | Sort-Object RecordsCount -Descending | Select-Object -First 10
```

## Batch Operations

### Example: Create Multiple Records in Batch

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Complex - requires building Entity objects manually
[Microsoft.Xrm.Sdk.Entity[]]$entities = @()
foreach($item in $data) {
    $entity = [Microsoft.Xrm.Sdk.Entity]::new('account')
    $entity.Attributes['name'] = $item.Name
    $entity.Attributes['telephone1'] = $item.Phone
    $entities += $entity
}

$response = New-CRMRecordsBatch -Entities $entities -conn $conn
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Simple - just pass PowerShell objects
$accounts = @(
    @{ name = "Company 1"; telephone1 = "555-0001" }
    @{ name = "Company 2"; telephone1 = "555-0002" }
    @{ name = "Company 3"; telephone1 = "555-0003" }
)

# Batching is automatic when multiple records are passed
$accounts | Set-DataverseRecord -Connection $conn -TableName account
```

## Working with Attachments

### Example: Create a Note with Attachment

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$fileContent = [System.IO.File]::ReadAllBytes("C:\file.pdf")
$base64 = [System.Convert]::ToBase64String($fileContent)

$note = New-Object Microsoft.Xrm.Sdk.Entity("annotation")
$note.Attributes["subject"] = "My Document"
$note.Attributes["documentbody"] = $base64
$note.Attributes["filename"] = "file.pdf"
$note.Attributes["objectid"] = New-CrmEntityReference -EntityLogicalName account -Id $accountId

$noteId = $conn.Create($note)
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
$fileContent = [System.IO.File]::ReadAllBytes("C:\file.pdf")
$base64 = [System.Convert]::ToBase64String($fileContent)

$note = @{
    subject = "My Document"
    documentbody = $base64
    filename = "file.pdf"
    mimetype = "application/pdf"
    objectid = $accountId  # Can use ID directly, lookup is automatic
    "objectid@logicalname" = "account"
}

$noteId = Set-DataverseRecord -Connection $conn -TableName annotation -Fields $note
```

## Invoking Custom Requests

### Example: Close an Incident (Case)

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$caseid = "9EFFD829-8D95-E611-80F3-5065F38B3191"

$closure = New-Object Microsoft.Xrm.Sdk.Entity
$closure.LogicalName = "incidentresolution"
$closure.Attributes.Add("subject", "closure subject")
$closure.Attributes.Add("incidentid", (New-CrmEntityReference -EntityLogicalName incident -Id $caseid))
$closure.Id = [guid]::NewGuid()

$caseClose = New-Object Microsoft.Crm.Sdk.Messages.CloseIncidentRequest
$caseClose.Status = New-CrmOptionSetValue -Value -1
$caseClose.IncidentResolution = $closure

$conn.ExecuteCrmOrganizationRequest($caseClose)
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
$caseid = "9EFFD829-8D95-E611-80F3-5065F38B3191"

# Create the incident resolution entity
$resolution = @{
    subject = "closure subject"
    incidentid = $caseid
    "incidentid@logicalname" = "incident"
}
$resolutionId = Set-DataverseRecord -Connection $conn -TableName incidentresolution -Fields $resolution

# Create and execute the close request
$request = New-Object Microsoft.Crm.Sdk.Messages.CloseIncidentRequest
$request.Status = New-Object Microsoft.Xrm.Sdk.OptionSetValue(-1)

$resolutionEntity = New-Object Microsoft.Xrm.Sdk.Entity("incidentresolution", $resolutionId)
$resolutionEntity.Attributes["subject"] = "closure subject"
$resolutionEntity.Attributes["incidentid"] = New-Object Microsoft.Xrm.Sdk.EntityReference("incident", $caseid)
$request.IncidentResolution = $resolutionEntity

Invoke-DataverseRequest -Connection $conn -Request $request
```

### Example: Execute WhoAmI Request

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$whoamiRequest = New-Object Microsoft.Crm.Sdk.Messages.WhoAmIRequest
$response = $conn.Execute($whoamiRequest)
$userId = $response.UserId
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
$whoami = Get-DataverseWhoAmI -Connection $conn
$userId = $whoami.UserId
# Also provides: BusinessUnitId, OrganizationId
```

## Metadata Operations

### Example: Get Entity Metadata

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$metadata = Get-CrmEntityAllMetadata -conn $conn -EntityFilters Entity
$accountMetadata = $metadata | Where-Object {$_.LogicalName -eq "account"}
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using SQL queries against metadata
$entityInfo = Invoke-DataverseSql -Connection $conn -Sql @"
SELECT name, displayname, primaryidattribute, primarynameattribute
FROM metadata.entity
WHERE name = 'account'
"@
```

## SQL Queries

The `Rnwood.Dataverse.Data.PowerShell` module includes support for SQL queries via Sql4Cds, which is not available in `Microsoft.Xrm.Data.PowerShell`.

### Example: SQL SELECT Query

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
$results = Invoke-DataverseSql -Connection $conn -Sql @"
SELECT TOP 10 fullname, emailaddress1, telephone1
FROM contact
WHERE lastname = 'Smith'
ORDER BY createdon DESC
"@

$results | Format-Table
```

### Example: SQL INSERT

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
Invoke-DataverseSql -Connection $conn -Sql @"
INSERT INTO account (name, telephone1)
VALUES ('New Company', '555-9999')
"@
```

### Example: SQL UPDATE

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
Invoke-DataverseSql -Connection $conn -Sql @"
UPDATE contact
SET telephone1 = '555-0000'
WHERE lastname = 'Smith'
"@
```

### Example: SQL with Parameters

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
$results = Invoke-DataverseSql -Connection $conn -Sql @"
SELECT fullname, createdon
FROM contact
WHERE lastname = @lastname
"@ -Parameters @{ lastname = "Wood" }

$results | Format-Table
```

### Example: SQL Pipeline Parameters

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
@{lastname="Wood"}, @{lastname="Smith"} | Invoke-DataverseSql -Connection $conn -Sql @"
SELECT TOP 1 lastname, createdon
FROM contact
WHERE lastname = @lastname
"@
```

## Key Differences and Advantages

### Automatic Paging
**Microsoft.Xrm.Data.PowerShell** requires manual paging implementation.  
**Rnwood.Dataverse.Data.PowerShell** handles paging automatically.

### Type Conversion
**Microsoft.Xrm.Data.PowerShell** requires manual type conversion (EntityReference, OptionSetValue, etc.).  
**Rnwood.Dataverse.Data.PowerShell** automatically converts types based on metadata.

### Batching
**Microsoft.Xrm.Data.PowerShell** requires manual batch setup with SDK objects.  
**Rnwood.Dataverse.Data.PowerShell** automatically batches multiple records.

### Cross-Platform
**Microsoft.Xrm.Data.PowerShell** works only on Windows with PowerShell Desktop.  
**Rnwood.Dataverse.Data.PowerShell** works on Windows, Linux, and macOS with PowerShell 7+.

### SQL Support
**Microsoft.Xrm.Data.PowerShell** does not have SQL query support.  
**Rnwood.Dataverse.Data.PowerShell** includes full SQL support via Sql4Cds.

## See Also

- [Get-DataverseConnection](Get-DataverseConnection.md)
- [Get-DataverseRecord](Get-DataverseRecord.md)
- [Set-DataverseRecord](Set-DataverseRecord.md)
- [Remove-DataverseRecord](Remove-DataverseRecord.md)
- [Invoke-DataverseRequest](Invoke-DataverseRequest.md)
- [Invoke-DataverseSql](Invoke-DataverseSql.md)
