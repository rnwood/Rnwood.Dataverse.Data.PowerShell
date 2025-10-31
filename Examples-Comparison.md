# Rnwood.Dataverse.Data.PowerShell Examples

This document provides examples of common Dataverse operations using `Rnwood.Dataverse.Data.PowerShell`, showing how to accomplish tasks that were done with `Microsoft.Xrm.Data.PowerShell`.

## Table of Contents

- [Connection](#connection)
- [Basic CRUD Operations](#basic-crud-operations)
- [Querying Records](#querying-records)
- [Batch Operations](#batch-operations)
- [Working with Attachments](#working-with-attachments)
- [Working with Web Resources](#working-with-web-resources)
- [Invoking Custom Requests](#invoking-custom-requests)
- [Metadata Operations](#metadata-operations)
- [Solution Management](#solution-management)
- [User and Team Operations](#user-and-team-operations)
- [Plugin and Workflow Management](#plugin-and-workflow-management)
- [Advanced Query Scenarios](#advanced-query-scenarios)
- [Workflow and Async Job Management](#workflow-and-async-job-management)
- [Marketing Lists](#marketing-lists)
- [Organization Settings](#organization-settings)
- [Multi-Organization Operations](#multi-organization-operations)
- [Duplicate Detection](#duplicate-detection)
- [Business Process Flows](#business-process-flows)
- [Form Management](#form-management)
- [Ribbon Customizations](#ribbon-customizations)
- [Views and Quick Find](#views-and-quick-find)
- [Sitemap Management](#sitemap-management)

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
$contacts = Get-DataverseRecord -Connection $conn -TableName contact -Columns fullname -Filter @{
  lastname = "Smith"
}

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
# Note: New-CRMRecordsBatch is from the Microsoft.Xrm.Data.PowerShell.Samples repository
# (https://github.com/seanmcne/Microsoft.Xrm.Data.PowerShell.Samples), not built into the module
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
# Batching is built-in and automatic
$accounts = @(
  @{
    name = "Company 1"
    telephone1 = "555-0001"
  }
  @{
    name = "Company 2"
    telephone1 = "555-0002"
  }
  @{
    name = "Company 3"
    telephone1 = "555-0003"
  }
)

# Batching is automatic when multiple records are passed
# Uses ExecuteMultipleRequest internally with default batch size of 100
$accounts | Set-DataverseRecord -Connection $conn -TableName account

# Or control batch size
$accounts | Set-DataverseRecord -Connection $conn -TableName account -BatchSize 50

# Or disable batching (send one request at a time)
$accounts | Set-DataverseRecord -Connection $conn -TableName account -BatchSize 1
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
}

$noteId = Set-DataverseRecord -Connection $conn -TableName annotation -Fields $note
```

## Working with Web Resources

### Example: Create a Web Resource from a File

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Complex - requires building Entity objects and manual base64 encoding
$fileContent = [System.IO.File]::ReadAllBytes("C:\script.js")
$base64 = [System.Convert]::ToBase64String($fileContent)

$webResource = New-Object Microsoft.Xrm.Sdk.Entity("webresource")
$webResource.Attributes["name"] = "new_/script.js"
$webResource.Attributes["displayname"] = "My Script"
$webResource.Attributes["webresourcetype"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(3)  # JavaScript
$webResource.Attributes["content"] = $base64

$webResourceId = $conn.Create($webResource)
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Simple - automatically handles file reading and base64 encoding
Set-DataverseWebResource -Connection $conn -Name "new_script" -Path "C:\script.js" `
    -DisplayName "My Script" -PublisherPrefix "new"
```

### Example: Update a Web Resource

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Complex - requires retrieving, modifying and updating
$webResource = $conn.Retrieve("webresource", $webResourceId, (New-Object Microsoft.Xrm.Sdk.Query.ColumnSet($true)))
$fileContent = [System.IO.File]::ReadAllBytes("C:\updated-script.js")
$webResource.Attributes["content"] = [System.Convert]::ToBase64String($fileContent)
$conn.Update($webResource)

# Publish the web resource
$publishRequest = New-Object Microsoft.Crm.Sdk.Messages.PublishXmlRequest
$publishRequest.ParameterXml = "<importexportxml><webresources><webresource>$webResourceId</webresource></webresources></importexportxml>"
$conn.Execute($publishRequest)
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Simple - automatically updates by name and optionally publishes
Set-DataverseWebResource -Connection $conn -Name "new_script" -Path "C:\updated-script.js" -Publish
```

### Example: Download a Web Resource

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Complex - requires manual base64 decoding and file writing
$query = New-Object Microsoft.Xrm.Sdk.Query.QueryExpression("webresource")
$query.ColumnSet = New-Object Microsoft.Xrm.Sdk.Query.ColumnSet("name", "content")
$query.Criteria.AddCondition("name", [Microsoft.Xrm.Sdk.Query.ConditionOperator]::Equal, "new_/script.js")

$result = $conn.RetrieveMultiple($query)
if ($result.Entities.Count -gt 0) {
    $webResource = $result.Entities[0]
    $content = [System.Convert]::FromBase64String($webResource.Attributes["content"])
    [System.IO.File]::WriteAllBytes("C:\downloaded-script.js", $content)
}
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Simple - automatically handles base64 decoding and file writing
Get-DataverseWebResource -Connection $conn -Name "new_script" -Path "C:\downloaded-script.js"
```

### Example: Upload Multiple Web Resources from a Folder

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Complex - requires looping and manual handling
$files = Get-ChildItem -Path "C:\webresources" -File -Filter "*.js"
foreach ($file in $files) {
    $fileContent = [System.IO.File]::ReadAllBytes($file.FullName)
    $base64 = [System.Convert]::ToBase64String($fileContent)
    
    $webResource = New-Object Microsoft.Xrm.Sdk.Entity("webresource")
    $webResource.Attributes["name"] = "new_/$($file.Name)"
    $webResource.Attributes["displayname"] = $file.BaseName
    $webResource.Attributes["webresourcetype"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(3)
    $webResource.Attributes["content"] = $base64
    
    $conn.Create($webResource)
}
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Simple - single command processes entire folder
Set-DataverseWebResource -Connection $conn -Folder "C:\webresources" `
    -PublisherPrefix "new" -FileFilter "*.js" -Publish
```

### Example: Download Multiple Web Resources

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Complex - requires querying, looping, and manual file handling
$query = New-Object Microsoft.Xrm.Sdk.Query.QueryExpression("webresource")
$query.ColumnSet = New-Object Microsoft.Xrm.Sdk.Query.ColumnSet("name", "content")
$query.Criteria.AddCondition("webresourcetype", [Microsoft.Xrm.Sdk.Query.ConditionOperator]::Equal, 3)

$results = $conn.RetrieveMultiple($query)
foreach ($webResource in $results.Entities) {
    $name = $webResource.Attributes["name"].Replace("/", "_")
    $content = [System.Convert]::FromBase64String($webResource.Attributes["content"])
    [System.IO.File]::WriteAllBytes("C:\download\$name", $content)
}
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Simple - automatically saves all matching resources
Get-DataverseWebResource -Connection $conn -FilterValues @{ webresourcetype = 3 } `
    -Folder "C:\download"
```

### Example: Delete a Web Resource

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Must look up by name first, then delete
$query = New-Object Microsoft.Xrm.Sdk.Query.QueryExpression("webresource")
$query.ColumnSet = New-Object Microsoft.Xrm.Sdk.Query.ColumnSet("webresourceid")
$query.Criteria.AddCondition("name", [Microsoft.Xrm.Sdk.Query.ConditionOperator]::Equal, "new_/script.js")

$result = $conn.RetrieveMultiple($query)
if ($result.Entities.Count -gt 0) {
    $conn.Delete("webresource", $result.Entities[0].Id)
}
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Simple - delete by name directly
Remove-DataverseWebResource -Connection $conn -Name "new_script"

# Or pipe from Get
Get-DataverseWebResource -Connection $conn -FilterValues @{ name = "new_script" } |
    Remove-DataverseWebResource -Connection $conn
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

# Using specialized cmdlet (simplest)
$resolution = @{
    subject = "closure subject"
    incidentid = $caseid
}

Invoke-DataverseCloseIncident -Connection $conn `
    -IncidentResolution $resolution `
    -IncidentResolutionTableName incidentresolution `
    -Status (New-Object Microsoft.Xrm.Sdk.OptionSetValue(-1))
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
"@ -Parameters @{
  lastname = "Wood"
}

$results | Format-Table
```

### Example: SQL Pipeline Parameters

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
@(
  @{
    lastname = "Wood"
  },
  @{
    lastname = "Smith"
  }
) | Invoke-DataverseSql -Connection $conn -Sql @"
SELECT TOP 1 lastname, createdon
FROM contact
WHERE lastname = @lastname
"@
```

## Solution Management

### Example: Delete a Solution

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$solutionUniqueName = 'mysolution'
$solutions = Get-CrmRecords -EntityLogicalName solution -FilterAttribute uniquename -FilterOperator eq -FilterValue $solutionUniqueName

if ($solutions.Count -eq 1) {
    $solutionId = $solutions.CrmRecords[0].ReturnProperty_Id
    $conn.Delete("solution", $solutionId)
}
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
$solutionUniqueName = 'mysolution'

# Query for the solution
$solution = Get-DataverseRecord -Connection $conn -TableName solution -Filter @{uniquename = $solutionUniqueName}

if ($solution) {
    Remove-DataverseRecord -Connection $conn -TableName solution -Id $solution.Id
}
```

### Example: Export a Solution

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$solutionName = "MySolution"
Export-CrmSolution -conn $conn -SolutionName $solutionName -Managed $false -ExportPath "C:\Solutions\"
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
$solutionName = "MySolution"

# Using specialized cmdlet (simplest)
$response = Invoke-DataverseExportSolution -Connection $conn `
    -SolutionName $solutionName `
    -Managed $false

# Save the solution file
[System.IO.File]::WriteAllBytes("C:\Solutions\$solutionName.zip", $response.ExportSolutionFile)
```

### Example: Import a Solution

**Microsoft.Xrm.Data.PowerShell:**
```powershell
Import-CrmSolution -conn $conn -SolutionFilePath "C:\Solutions\MySolution.zip"
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
$solutionBytes = [System.IO.File]::ReadAllBytes("C:\Solutions\MySolution.zip")

# Using specialized cmdlet (simplest)
Invoke-DataverseImportSolution -Connection $conn `
    -CustomizationFile $solutionBytes `
    -PublishWorkflows $true `
    -OverwriteUnmanagedCustomizations $false
```

### Example: List All Solutions

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$solutions = Get-CrmRecords -EntityLogicalName solution -Fields uniquename,friendlyname,version
$solutions.CrmRecords | Select-Object uniquename,friendlyname,version
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using Get-DataverseRecord
$solutions = Get-DataverseRecord -Connection $conn -TableName solution -Columns uniquename,friendlyname,version

$solutions | Select-Object uniquename,friendlyname,version

# Or using SQL
Invoke-DataverseSql -Connection $conn -Sql @"
SELECT uniquename, friendlyname, version
FROM solution
WHERE ismanaged = 0
ORDER BY friendlyname
"@
```

## User and Team Operations

### Example: Add User to Team

**Microsoft.Xrm.Data.PowerShell:**
```powershell
Add-CrmUserToTeam -TeamId $teamId -UserId $userId
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using specialized cmdlet (simplest)
Invoke-DataverseAddMembersTeam -Connection $conn `
    -TeamId $teamId `
    -MemberIds @($userId)
```

### Example: Assign Security Role to User

**Microsoft.Xrm.Data.PowerShell:**
```powershell
Add-CrmUserToSecurityRole -UserId $userId -SecurityRoleId $roleId
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Create the role assignment association
$request = New-Object Microsoft.Crm.Sdk.Messages.AssignRequest
$request.Target = New-Object Microsoft.Xrm.Sdk.EntityReference("systemuser", $userId)
$request.Assignee = New-Object Microsoft.Xrm.Sdk.EntityReference("role", $roleId)

Invoke-DataverseRequest -Connection $conn -Request $request

# Or use SQL to add role
Invoke-DataverseSql -Connection $conn -Sql @"
INSERT INTO systemuserroles (systemuserid, roleid)
VALUES ('$userId', '$roleId')
"@
```

### Example: Get All Users in a Team

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$teamId = "guid-here"
$members = Get-CrmRecords -EntityLogicalName systemuser -FetchXml @"
<fetch>
  <entity name='systemuser'>
    <attribute name='fullname' />
    <attribute name='internalemailaddress' />
    <link-entity name='teammembership' from='systemuserid' to='systemuserid'>
      <filter>
        <condition attribute='teamid' operator='eq' value='$teamId' />
      </filter>
    </link-entity>
  </entity>
</fetch>
"@
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
$teamId = "guid-here"

# Using FetchXML
$fetchXml = @"
<fetch>
  <entity name='systemuser'>
    <attribute name='fullname' />
    <attribute name='internalemailaddress' />
    <link-entity name='teammembership' from='systemuserid' to='systemuserid'>
      <filter>
        <condition attribute='teamid' operator='eq' value='$teamId' />
      </filter>
    </link-entity>
  </entity>
</fetch>
"@
$members = Get-DataverseRecord -Connection $conn -FetchXml $fetchXml

# Or using SQL (simpler)
$members = Invoke-DataverseSql -Connection $conn -Sql @"
SELECT u.fullname, u.internalemailaddress
FROM systemuser u
INNER JOIN teammembership tm ON u.systemuserid = tm.systemuserid
WHERE tm.teamid = '$teamId'
"@
```

### Example: Disable All Users Except Administrators

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$users = Get-CrmRecords -EntityLogicalName systemuser -FilterAttribute isdisabled -FilterOperator eq -FilterValue $false

foreach ($user in $users.CrmRecords) {
    # Check if user is admin (has System Administrator role)
    $roles = Get-CrmUserSecurityRoles -UserId $user.systemuserid
    $isAdmin = $roles | Where-Object { $_.Name -eq "System Administrator" }
    
    if (-not $isAdmin) {
        Set-CrmRecordState -EntityLogicalName systemuser -Id $user.systemuserid -StateCode Disabled -StatusCode Disabled
    }
}
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Get all non-admin users using SQL
$nonAdminUsers = Invoke-DataverseSql -Connection $conn -Sql @"
SELECT u.systemuserid
FROM systemuser u
WHERE u.isdisabled = 0
AND u.systemuserid NOT IN (
    SELECT sur.systemuserid
    FROM systemuserroles sur
    INNER JOIN role r ON sur.roleid = r.roleid
    WHERE r.name = 'System Administrator'
)
"@

# Disable each user using SetState request
foreach ($user in $nonAdminUsers) {
    $request = New-Object Microsoft.Crm.Sdk.Messages.SetStateRequest
    $request.EntityMoniker = New-Object Microsoft.Xrm.Sdk.EntityReference("systemuser", $user.systemuserid)
    $request.State = New-Object Microsoft.Xrm.Sdk.OptionSetValue(1) # Disabled
    $request.Status = New-Object Microsoft.Xrm.Sdk.OptionSetValue(2) # Disabled
    
    Invoke-DataverseRequest -Connection $conn -Request $request
}
```

## Plugin and Workflow Management

### Example: Disable All Plugin Steps

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Get all custom assemblies
$assemblies = Get-CrmRecords -EntityLogicalName pluginassembly `
    -FilterAttribute customizationlevel -FilterOperator eq -FilterValue 1

$steps = @()
foreach ($assembly in $assemblies.CrmRecords) {
    $sdkmessages = Get-CrmSdkMessageProcessingStepsForPluginAssembly `
        -PluginAssemblyName $assembly.name
    
    $steps += $sdkmessages | Where-Object { $_.statecode -eq 'Enabled' }
}

# Disable all enabled steps
foreach ($step in $steps) {
    Set-CrmRecordState -EntityLogicalName sdkmessageprocessingstep `
        -Id $step.sdkmessageprocessingstepid `
        -StateCode Disabled -StatusCode Disabled
}
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Get all enabled plugin steps for custom assemblies
$steps = Invoke-DataverseSql -Connection $conn -Sql @"
SELECT s.sdkmessageprocessingstepid, s.name
FROM sdkmessageprocessingstep s
INNER JOIN pluginassembly pa ON s.pluginassemblyid = pa.pluginassemblyid
WHERE pa.customizationlevel = 1
AND s.statecode = 0
"@

# Disable all steps using SetState request
foreach ($step in $steps) {
    $request = New-Object Microsoft.Crm.Sdk.Messages.SetStateRequest
    $request.EntityMoniker = New-Object Microsoft.Xrm.Sdk.EntityReference("sdkmessageprocessingstep", $step.sdkmessageprocessingstepid)
    $request.State = New-Object Microsoft.Xrm.Sdk.OptionSetValue(1) # Disabled
    $request.Status = New-Object Microsoft.Xrm.Sdk.OptionSetValue(2) # Disabled
    
    Invoke-DataverseRequest -Connection $conn -Request $request
}
```

### Example: List All Workflows

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$workflows = Get-CrmRecords -EntityLogicalName workflow `
    -FilterAttribute type -FilterOperator eq -FilterValue 1 `
    -Fields name,statecode,primaryentity
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using Get-DataverseRecord
$workflows = Get-DataverseRecord -Connection $conn -TableName workflow `
    -Filter @{type = 1} `
    -Columns name,statecode,primaryentity

# Or using SQL
$workflows = Invoke-DataverseSql -Connection $conn -Sql @"
SELECT name, statecode, primaryentity
FROM workflow
WHERE type = 1
ORDER BY name
"@
```

## Advanced Query Scenarios

### Example: Query with Multiple Filters (AND/OR Logic)

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$fetch = @"
<fetch>
  <entity name='contact'>
    <attribute name='fullname' />
    <filter type='and'>
      <condition attribute='statecode' operator='eq' value='0' />
      <filter type='or'>
        <condition attribute='address1_city' operator='eq' value='Seattle' />
        <condition attribute='address1_city' operator='eq' value='Redmond' />
      </filter>
    </filter>
  </entity>
</fetch>
"@
$contacts = Get-CrmRecordsByFetch -Fetch $fetch
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using FetchXML
$fetchXml = @"
<fetch>
  <entity name='contact'>
    <attribute name='fullname' />
    <filter type='and'>
      <condition attribute='statecode' operator='eq' value='0' />
      <filter type='or'>
        <condition attribute='address1_city' operator='eq' value='Seattle' />
        <condition attribute='address1_city' operator='eq' value='Redmond' />
      </filter>
    </filter>
  </entity>
</fetch>
"@
$contacts = Get-DataverseRecord -Connection $conn -FetchXml $fetchXml

# Or using SQL (much simpler!)
$contacts = Invoke-DataverseSql -Connection $conn -Sql @"
SELECT fullname
FROM contact
WHERE statecode = 0
AND (address1_city = 'Seattle' OR address1_city = 'Redmond')
"@
```

### Example: Negation (NOT)

Negation can be expressed concisely using `not` in the hashtable-based filters.

**Microsoft.Xrm.Data.PowerShell (FetchXML):**
```powershell
$fetch = @"
<fetch>
  <entity name='contact'>
    <filter>
      <condition attribute='firstname' operator='ne' value='Rob' />
    </filter>
  </entity>
</fetch>
"@

$contacts = Get-CrmRecordsByFetch -conn $conn -Fetch $fetch
```

**Rnwood.Dataverse.Data.PowerShell (hashtable grouping):**
```powershell
Get-DataverseRecord -Connection $conn -TableName contact -FilterValues @{
  'not' = @{
    firstname = 'Rob'
  }
}
```

### Note on XOR

`xor` is supported to express "exactly one of" semantics and is more concise than writing the corresponding FetchXML. However, when used in exclude filters the complement of XOR requires enumerating multiple combinations; to avoid excessive processing the cmdlet limits `xor` groups to 8 items. For large numbers of alternatives prefer SQL or FetchXML.

### Example: Complex Grouped Filters (concise hashtable syntax vs FetchXML)

When you need to express more complex nested logical combinations of filters the
concise hashtable-based grouping syntax in `Rnwood.Dataverse.Data.PowerShell` can
be significantly simpler than writing equivalent FetchXML. Consider the
predicate: ( (firstname = 'Rob' OR firstname = 'Joe') AND (lastname = 'One' OR lastname = 'Smith') )

**Microsoft.Xrm.Data.PowerShell (FetchXML):**
```powershell
$fetch = @"
<fetch>
  <entity name='contact'>
    <attribute name='fullname' />
    <filter type='and'>
      <filter type='or'>
        <condition attribute='firstname' operator='eq' value='Rob' />
        <condition attribute='firstname' operator='eq' value='Joe' />
      </filter>
      <filter type='or'>
        <condition attribute='lastname' operator='eq' value='One' />
        <condition attribute='lastname' operator='eq' value='Smith' />
      </filter>
    </filter>
  </entity>
</fetch>
"@

$contacts = Get-CrmRecordsByFetch -conn $conn -Fetch $fetch
```

**Rnwood.Dataverse.Data.PowerShell (hashtable grouping):**
```powershell
# The same predicate expressed concisely using nested hashtable groups:
$contacts = Get-DataverseRecord -Connection $conn -TableName contact -FilterValues @{
    'and' = @(
    @{ 'or' = @(
      @{
        firstname = 'Rob'
      },
      @{
        firstname = 'Joe'
      }
    ) },
    @{ 'or' = @(
      @{
        lastname = 'One'
      },
      @{
        lastname = 'Smith'
      }
    ) }
    )
}
```

Notes:
- The hashtable syntax maps naturally to PowerShell objects and avoids manual XML construction.
- Grouping keys are case-insensitive (`and` / `or`) and can be nested arbitrarily.
- The same grouped structure can also be expressed as FetchXML or SQL if required, but the hashtable approach keeps complex logical structures readable and easier to build programmatically.

### Example: Excluding with grouped filters

When you want to exclude records that match a grouped condition, `Rnwood.Dataverse.Data.PowerShell` provides `-ExcludeFilterValues` which accepts the same grouping hashtables as `-FilterValues`.

**Scenario:** Exclude records where firstname = 'Rob' OR lastname = 'Smith'.

**Microsoft.Xrm.Data.PowerShell (FetchXML):**
```powershell
$fetch = @"
<fetch>
  <entity name='contact'>
    <filter type='and'>
      <condition attribute='firstname' operator='ne' value='Rob' />
      <condition attribute='lastname' operator='ne' value='Smith' />
    </filter>
  </entity>
</fetch>
"@

$contacts = Get-CrmRecordsByFetch -conn $conn -Fetch $fetch
```

**Rnwood.Dataverse.Data.PowerShell (ExcludeFilterValues):**
```powershell
Get-DataverseRecord -Connection $conn -TableName contact -ExcludeFilterValues @{
  'or' = @(
  @{
    firstname = 'Rob'
  },
  @{
    lastname = 'Smith'
  }
  )
}
```

### Example: Include and Exclude Together

Combine `-FilterValues` (include) and `-ExcludeFilterValues` to compose precise queries.

**Scenario:** Include contacts whose lastname is 'One' or 'Two', but exclude those where exactly one of (emailaddress1 present, mobilephone present) — i.e. exclude contacts where exactly one of these contact methods exists.

**Microsoft.Xrm.Data.PowerShell (FetchXML):**
```powershell
# The server-side equivalent filters for the complement of XOR (both present OR both absent)
$fetch = @"
<fetch>
  <entity name='contact'>
    <filter type='and'>
      <filter type='or'>
        <condition attribute='lastname' operator='eq' value='One' />
        <condition attribute='lastname' operator='eq' value='Two' />
      </filter>
      <filter type='or'>
        <filter type='and'>
          <condition attribute='emailaddress1' operator='null' />
          <condition attribute='mobilephone' operator='null' />
        </filter>
        <filter type='and'>
          <condition attribute='emailaddress1' operator='not-null' />
          <condition attribute='mobilephone' operator='not-null' />
        </filter>
      </filter>
    </filter>
  </entity>
</fetch>
"@

$contacts = Get-CrmRecordsByFetch -conn $conn -Fetch $fetch
```

**Rnwood.Dataverse.Data.PowerShell (hashtable grouping):**
```powershell
Get-DataverseRecord -Connection $conn -TableName contact \
  -FilterValues @{
      'or' = @(
      @{
        lastname = 'One'
      },
      @{
        lastname = 'Two'
      }
      )
  } \
  -ExcludeFilterValues @{
      'xor' = @(
      @{
        emailaddress1 = @{
          operator = 'NotNull'
        }
      },
      @{
        mobilephone = @{
          operator = 'NotNull'
        }
      }
      )
  }
```

This example demonstrates exclusion based on an exclusive-or across two different attributes (presence of email vs presence of mobile). The hashtable `xor` expression keeps the exclusion intent concise; the FetchXML equivalent must express the complement explicitly so it can be executed server-side.

### Example: XOR grouping

`xor` provides a concise way to express "exactly one of" semantics. For example:

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
Get-DataverseRecord -Connection $conn -TableName contact -FilterValues @{
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

Equivalent logic using FetchXML (expanded) requires writing the OR of two terms `(A AND NOT B) OR (B AND NOT A)` and becomes more verbose as the number of elements increases. When using `xor` with many items, be aware of combinatorial explosion (the cmdlet limits `xor` groups to 8 items to avoid excessive processing); for many items consider SQL or other server-side filtering instead.

### Example: Query with Aggregation

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$fetch = @"
<fetch aggregate='true'>
  <entity name='opportunity'>
    <attribute name='estimatedvalue' aggregate='sum' alias='totalvalue' />
    <attribute name='ownerid' groupby='true' alias='owner' />
  </entity>
</fetch>
"@
$results = Get-CrmRecordsByFetch -Fetch $fetch
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using FetchXML
$fetchXml = @"
<fetch aggregate='true'>
  <entity name='opportunity'>
    <attribute name='estimatedvalue' aggregate='sum' alias='totalvalue' />
    <attribute name='ownerid' groupby='true' alias='owner' />
  </entity>
</fetch>
"@
$results = Get-DataverseRecord -Connection $conn -FetchXml $fetchXml

# Or using SQL (much more familiar!)
$results = Invoke-DataverseSql -Connection $conn -Sql @"
SELECT ownerid, SUM(estimatedvalue) as totalvalue
FROM opportunity
GROUP BY ownerid
"@
```

### Example: Query with Linked Entities (Joins)

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$fetch = @"
<fetch>
  <entity name='contact'>
    <attribute name='fullname' />
    <link-entity name='account' from='accountid' to='parentcustomerid' alias='parent'>
      <attribute name='name' />
      <filter>
        <condition attribute='revenue' operator='gt' value='1000000' />
      </filter>
    </link-entity>
  </entity>
</fetch>
"@
$contacts = Get-CrmRecordsByFetch -Fetch $fetch
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using FetchXML
$fetchXml = @"
<fetch>
  <entity name='contact'>
    <attribute name='fullname' />
    <link-entity name='account' from='accountid' to='parentcustomerid' alias='parent'>
      <attribute name='name' />
      <filter>
        <condition attribute='revenue' operator='gt' value='1000000' />
      </filter>
    </link-entity>
  </entity>
</fetch>
"@
$contacts = Get-DataverseRecord -Connection $conn -FetchXml $fetchXml

# Or using SQL (standard SQL JOIN syntax!)
$contacts = Invoke-DataverseSql -Connection $conn -Sql @"
SELECT c.fullname, a.name as accountname
FROM contact c
INNER JOIN account a ON c.parentcustomerid = a.accountid
WHERE a.revenue > 1000000
"@
```

### Example: Bulk Delete Records

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Create bulk delete request
$fetch = @"
<fetch>
  <entity name='contact'>
    <filter>
      <condition attribute='statecode' operator='eq' value='1' />
    </filter>
  </entity>
</fetch>
"@

$request = New-Object Microsoft.Crm.Sdk.Messages.BulkDeleteRequest
$request.QuerySet = @()
$fetchQuery = New-Object Microsoft.Xrm.Sdk.Query.FetchExpression($fetch)
$request.QuerySet += $fetchQuery
$request.JobName = "Delete Inactive Contacts"
$request.StartDateTime = [DateTime]::Now
$request.RecurrencePattern = [string]::Empty
$request.SendEmailNotification = $false

$conn.Execute($request)
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using SQL DELETE (simpler!)
Invoke-DataverseSql -Connection $conn -Sql @"
DELETE FROM contact
WHERE statecode = 1
"@

# Or using traditional bulk delete request
$fetch = @"
<fetch>
  <entity name='contact'>
    <filter>
      <condition attribute='statecode' operator='eq' value='1' />
    </filter>
  </entity>
</fetch>
"@

$request = New-Object Microsoft.Crm.Sdk.Messages.BulkDeleteRequest
$fetchQuery = New-Object Microsoft.Xrm.Sdk.Query.FetchExpression($fetch)
$request.QuerySet = @($fetchQuery)
$request.JobName = "Delete Inactive Contacts"
$request.StartDateTime = [DateTime]::Now
$request.RecurrencePattern = [string]::Empty
$request.SendEmailNotification = $false

Invoke-DataverseRequest -Connection $conn -Request $request
```

## Workflow and Async Job Management

### Example: Cancel Waiting Workflows

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Create fetch query for waiting workflows
$fetch = @"
<fetch>
  <entity name='asyncoperation'>
    <attribute name='asyncoperationid' />
    <filter type='and'>
      <condition attribute='operationtype' operator='eq' value='10' />
      <condition attribute='statuscode' operator='eq' value='10' />
    </filter>
  </entity>
</fetch>
"@

$jobs = Get-CrmRecordsByFetch -conn $conn -Fetch $fetch

foreach($job in $jobs.CrmRecords) {
    $job.statecode = New-CrmOptionSetValue 3
    $job.statuscode = New-CrmOptionSetValue 32
    Set-CrmRecord -conn $conn -CrmRecord $job
    Remove-CrmRecord -conn $conn -EntityLogicalName asyncoperation -Id $job.asyncoperationid
}
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using SQL for simpler querying
$waitingJobs = Invoke-DataverseSql -Connection $conn -Sql @"
SELECT asyncoperationid
FROM asyncoperation
WHERE operationtype = 10 AND statuscode = 10
"@

foreach($job in $waitingJobs) {
    # Cancel the workflow using SetState request
    $request = New-Object Microsoft.Crm.Sdk.Messages.SetStateRequest
    $request.EntityMoniker = New-Object Microsoft.Xrm.Sdk.EntityReference("asyncoperation", $job.asyncoperationid)
    $request.State = New-Object Microsoft.Xrm.Sdk.OptionSetValue(3) # Canceled
    $request.Status = New-Object Microsoft.Xrm.Sdk.OptionSetValue(32) # Canceled
    
    Invoke-DataverseRequest -Connection $conn -Request $request
    
    # Then remove
    Remove-DataverseRecord -Connection $conn -TableName asyncoperation -Id $job.asyncoperationid
}
```

### Example: Monitor Long-Running Workflows

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$fetch = @"
<fetch>
  <entity name='asyncoperation'>
    <attribute name='name' />
    <attribute name='startedon' />
    <filter>
      <condition attribute='operationtype' operator='eq' value='10' />
      <condition attribute='statuscode' operator='eq' value='20' />
    </filter>
  </entity>
</fetch>
"@
$runningJobs = Get-CrmRecordsByFetch -conn $conn -Fetch $fetch
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using SQL for cleaner queries
$runningJobs = Invoke-DataverseSql -Connection $conn -Sql @"
SELECT name, startedon, DATEDIFF(minute, startedon, GETUTCDATE()) as runtime_minutes
FROM asyncoperation
WHERE operationtype = 10 AND statuscode = 20
ORDER BY startedon
"@

# Show long-running workflows
$runningJobs | Where-Object { $_.runtime_minutes -gt 60 } | Format-Table
```

## Marketing Lists

### Example: Add Members to Marketing List

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$marketingListId = "107E563B-7D21-40A5-AF6B-C8975E9C3860"
$contactId = "C69F9B23-F3B2-403F-A1CF-C81FEF71126F"

$addMember = New-Object Microsoft.Crm.Sdk.Messages.AddMemberListRequest
$addMember.EntityId = $contactId
$addMember.ListId = $marketingListId

$conn.ExecuteCrmOrganizationRequest($addMember)
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
$marketingListId = "107E563B-7D21-40A5-AF6B-C8975E9C3860"
$contactId = "C69F9B23-F3B2-403F-A1CF-C81FEF71126F"

# Using specialized cmdlet (simplest)
Invoke-DataverseAddMemberList -Connection $conn `
    -EntityId $contactId `
    -ListId $marketingListId
```

### Example: Remove Members from Marketing List

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$removeMember = New-Object Microsoft.Crm.Sdk.Messages.RemoveMemberListRequest
$removeMember.EntityId = $contactId
$removeMember.ListId = $marketingListId

$conn.ExecuteCrmOrganizationRequest($removeMember)
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using specialized cmdlet (simplest)
Invoke-DataverseRemoveMemberList -Connection $conn `
    -EntityId $contactId `
    -ListId $marketingListId
```

### Example: Get All Members of a Marketing List

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$fetch = @"
<fetch>
  <entity name='contact'>
    <attribute name='fullname' />
    <attribute name='emailaddress1' />
    <link-entity name='listmember' from='entityid' to='contactid'>
      <filter>
        <condition attribute='listid' operator='eq' value='$marketingListId' />
      </filter>
    </link-entity>
  </entity>
</fetch>
"@
$members = Get-CrmRecordsByFetch -conn $conn -Fetch $fetch
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using SQL (much simpler)
$members = Invoke-DataverseSql -Connection $conn -Sql @"
SELECT c.fullname, c.emailaddress1
FROM contact c
INNER JOIN listmember lm ON c.contactid = lm.entityid
WHERE lm.listid = '$marketingListId'
"@
```

## Organization Settings

### Example: Update Organization Settings

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$orgId = (Invoke-CrmWhoAmI).OrganizationId
$updateFields = @{
    "UnResolveEmailAddressIfMultipleMatch" = $false
}
Set-CrmRecord -conn $conn -EntityLogicalName organization -Id $orgId -Fields $updateFields
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
$whoami = Get-DataverseWhoAmI -Connection $conn
$orgId = $whoami.OrganizationId

Set-DataverseRecord -Connection $conn -TableName organization -Id $orgId -Fields @{
    unresolveemailaddressifmultiplematch = $false
}
```

### Example: Get Organization Settings

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$orgId = (Invoke-CrmWhoAmI).OrganizationId
$org = Get-CrmRecord -EntityLogicalName organization -Id $orgId -Fields *
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
$whoami = Get-DataverseWhoAmI -Connection $conn
$org = Get-DataverseRecord -Connection $conn -TableName organization -Id $whoami.OrganizationId

# Or using SQL to get specific settings
$settings = Invoke-DataverseSql -Connection $conn -Sql @"
SELECT name, isemailenabledfordelegation, maxuploadfilesize, allowlegacyclientexperience
FROM organization
"@
```

## Multi-Organization Operations

### Example: Connect and Operate Across Multiple Organizations

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Load connections from CSV
$connectionsSource = Import-Csv -Path ".\connections.csv"
$conns = @()

foreach($connectionSource in $connectionsSource) {
    $password = ConvertTo-SecureString -String $connectionSource.Password -AsPlainText -Force
    $cred = New-Object System.Management.Automation.PSCredential ($connectionSource.User, $password)
    
    if($connectionSource.Type -eq "Online") {
        $conn = Get-CrmConnection -Credential $cred -OnLineType Office365 `
            -DeploymentRegion $connectionSource.DeploymentRegion `
            -OrganizationName $connectionSource.OrganizationName
    }
    $conns += $conn
}

# Create records in all organizations
foreach($conn in $conns) {
  New-CrmRecord -conn $conn -EntityLogicalName account -Fields @{
    name = "Sample Account"
  }
}
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Load connections from CSV
$connectionsSource = Import-Csv -Path ".\connections.csv"
$conns = @()

foreach($connectionSource in $connectionsSource) {
    $conn = Get-DataverseConnection -Url $connectionSource.Url `
        -Username $connectionSource.User `
        -Password (ConvertTo-SecureString -String $connectionSource.Password -AsPlainText -Force)
    $conns += $conn
}

# Create records in all organizations
foreach($conn in $conns) {
  Set-DataverseRecord -Connection $conn -TableName account -Fields @{
    name = "Sample Account"
  }
}

# Or copy data from one org to another
$sourceConn = $conns[0]
$targetConn = $conns[1]

$accounts = Get-DataverseRecord -Connection $sourceConn -TableName account -Top 100
$accounts | Set-DataverseRecord -Connection $targetConn
```

## Duplicate Detection

### Example: Publish Duplicate Detection Rule

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$ruleId = "guid-here"

$request = New-Object Microsoft.Crm.Sdk.Messages.PublishDuplicateRuleRequest
$request.DuplicateRuleId = $ruleId

$conn.Execute($request)
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
$ruleId = "guid-here"

# Using specialized cmdlet (simplest)
Invoke-DataversePublishDuplicateRule -Connection $conn -DuplicateRuleId $ruleId
```

### Example: Unpublish Duplicate Detection Rule

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$request = New-Object Microsoft.Crm.Sdk.Messages.UnpublishDuplicateRuleRequest
$request.DuplicateRuleId = $ruleId

$conn.Execute($request)
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using specialized cmdlet (simplest)
Invoke-DataverseUnpublishDuplicateRule -Connection $conn -DuplicateRuleId $ruleId
```

### Example: Run Duplicate Detection Job

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$query = New-Object Microsoft.Xrm.Sdk.Query.QueryExpression("account")
$query.ColumnSet = New-Object Microsoft.Xrm.Sdk.Query.ColumnSet($true)

$request = New-Object Microsoft.Crm.Sdk.Messages.BulkDetectDuplicatesRequest
$request.Query = $query
$request.JobName = "Detect Duplicate Accounts"
$request.SendEmailNotification = $false
$request.ToRecipients = @()
$request.CCRecipients = @()

$conn.Execute($request)
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
$query = New-Object Microsoft.Xrm.Sdk.Query.QueryExpression("account")
$query.ColumnSet = New-Object Microsoft.Xrm.Sdk.Query.ColumnSet($true)

# Using specialized cmdlet (simplest)
Invoke-DataverseBulkDetectDuplicates -Connection $conn `
    -Query $query `
    -JobName "Detect Duplicate Accounts" `
    -SendEmailNotification $false `
    -ToRecipients @() `
    -CCRecipients @()
```

## Business Process Flows

### Example: Get All Business Process Flow Stages

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Retrieve all stages
$stages = Get-CrmRecords -conn $conn -EntityLogicalName processstage -Fields * -AllRows

# Display with processed information
$stages.CrmRecords | Select-Object primaryentitytypecode, `
    @{name='processname'; expression={$_.processid}}, `
    @{name='processid';expression={$_.processid_Property.Value.Id}}, `
    processstageid, stagecategory, stagename | Sort-Object primaryentitytypecode
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using Get-DataverseRecord (simpler)
$stages = Get-DataverseRecord -Connection $conn -TableName processstage `
    -Columns primaryentitytypecode,processid,processstageid,stagecategory,stagename

# Or using SQL for better querying
$stages = Invoke-DataverseSql -Connection $conn -Sql @"
SELECT 
    ps.primaryentitytypecode,
    w.name as processname,
    ps.processid,
    ps.processstageid,
    ps.stagecategory,
    ps.stagename
FROM processstage ps
INNER JOIN workflow w ON ps.processid = w.workflowid
ORDER BY ps.primaryentitytypecode, ps.stagename
"@

$stages | Format-Table
```

### Example: Get Active Stages for a Specific Entity

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$fetch = @"
<fetch>
  <entity name='processstage'>
    <attribute name='stagename' />
    <attribute name='stagecategory' />
    <filter>
      <condition attribute='primaryentitytypecode' operator='eq' value='lead' />
    </filter>
  </entity>
</fetch>
"@
$leadStages = Get-CrmRecordsByFetch -conn $conn -Fetch $fetch
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using SQL (much simpler)
$leadStages = Invoke-DataverseSql -Connection $conn -Sql @"
SELECT stagename, stagecategory, processid
FROM processstage
WHERE primaryentitytypecode = 'lead'
ORDER BY stagename
"@
```

## Form Management

### Example: Get all forms for an entity

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Not directly supported - required direct SDK queries
$query = New-Object Microsoft.Xrm.Sdk.Query.QueryExpression -ArgumentList "systemform"
$query.ColumnSet = New-Object Microsoft.Xrm.Sdk.Query.ColumnSet($true)
$query.Criteria.AddCondition("objecttypecode", [Microsoft.Xrm.Sdk.Query.ConditionOperator]::Equal, "contact")
$forms = Get-CrmRecords -conn $conn -Query $query
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using dedicated form cmdlet (simplest and recommended)
$forms = Get-DataverseForm -Connection $conn -Entity 'contact'

# Display form information
$forms | Format-Table FormId, Name, Type, IsActive, IsDefault

# Get only Main forms
$mainForms = Get-DataverseForm -Connection $conn -Entity 'contact' -FormType 'Main'
```

### Example: Get a specific form with FormXml

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Complex SDK query required
$formId = 'a1234567-89ab-cdef-0123-456789abcdef'
$form = Get-CrmRecord -conn $conn -EntityLogicalName systemform -Id $formId -Fields formxml,name
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Simple and clean with dedicated cmdlet
$formId = 'a1234567-89ab-cdef-0123-456789abcdef'
$form = Get-DataverseForm -Connection $conn -Id $formId -IncludeFormXml

# Parse FormXml structure
$parsedForm = Get-DataverseForm -Connection $conn -Id $formId -ParseFormXml
$parsedForm.ParsedForm.Tabs | ForEach-Object {
    Write-Host "Tab: $($_.Name) - Expanded: $($_.Expanded)"
    $_.Sections | ForEach-Object {
        Write-Host "  Section: $($_.Name)"
        $_.Controls | ForEach-Object {
            Write-Host "    Control: $($_.DataField)"
        }
    }
}
```

### Example: Create a new form

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Required manual SDK entity creation
$form = New-Object Microsoft.Xrm.Sdk.Entity -ArgumentList "systemform"
$form["name"] = "Custom Contact Form"
$form["objecttypecode"] = "contact"
$form["type"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue -ArgumentList 2  # Main form
$form["formxml"] = $formXmlContent
$formId = Set-CrmRecord -conn $conn -CrmRecord $form
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Clean and simple with dedicated cmdlet
$formId = Set-DataverseForm -Connection $conn `
    -Entity 'contact' `
    -Name 'Custom Contact Form' `
    -FormType 'Main' `
    -Description 'A custom form for contacts' `
    -IsActive `
    -PassThru

# With custom FormXml
$formXml = Get-Content -Path 'CustomForm.xml' -Raw
$formId = Set-DataverseForm -Connection $conn `
    -Entity 'contact' `
    -Name 'Advanced Contact Form' `
    -FormType 'Main' `
    -FormXmlContent $formXml `
    -Publish `
    -PassThru
```

### Example: Update an existing form

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Manual SDK entity updates
$formId = 'a1234567-89ab-cdef-0123-456789abcdef'
$form = New-Object Microsoft.Xrm.Sdk.Entity -ArgumentList "systemform"
$form.Id = $formId
$form["name"] = "Updated Form Name"
$form["description"] = "Updated description"
Set-CrmRecord -conn $conn -CrmRecord $form
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Simple updates with dedicated cmdlet
$formId = 'a1234567-89ab-cdef-0123-456789abcdef'
Set-DataverseForm -Connection $conn `
    -Id $formId `
    -Name 'Updated Form Name' `
    -Description 'Updated description' `
    -IsDefault `
    -Publish
```

### Example: Delete a form

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Manual SDK delete
$formId = 'a1234567-89ab-cdef-0123-456789abcdef'
Remove-CrmRecord -conn $conn -EntityLogicalName systemform -Id $formId
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Clean deletion with dedicated cmdlet
$formId = 'a1234567-89ab-cdef-0123-456789abcdef'
Remove-DataverseForm -Connection $conn -Id $formId -Publish

# Delete by name
Remove-DataverseForm -Connection $conn -Entity 'contact' -Name 'Old Form' -IfExists

# WhatIf support for safety
Remove-DataverseForm -Connection $conn -Id $formId -WhatIf
```

### Example: Export and import forms

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Complex manual process with SDK
$forms = Get-CrmRecords -conn $conn -EntityLogicalName systemform -FilterAttribute objecttypecode -FilterOperator eq -FilterValue contact -Fields formxml,name
foreach ($form in $forms.CrmRecords) {
    $formXml = $form.formxml
    [System.IO.File]::WriteAllText("C:\FormExports\$($form.name).xml", $formXml)
}
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Simple and clean with dedicated cmdlets
# Export forms
$forms = Get-DataverseForm -Connection $conn -Entity 'contact' -IncludeFormXml
$forms | ForEach-Object {
    $safeFileName = $_.Name -replace '[\\/:*?"<>|]', '_'
    $_.FormXml | Out-File -FilePath "C:\FormExports\$safeFileName.xml"
}

# Import forms to another environment
$targetConn = Get-DataverseConnection -Url "https://target.crm.dynamics.com" -Interactive
Get-ChildItem "C:\FormExports\*.xml" | ForEach-Object {
    $formXml = Get-Content $_.FullName -Raw
    $formName = [System.IO.Path]::GetFileNameWithoutExtension($_.Name)
    Set-DataverseForm -Connection $targetConn `
        -Entity 'contact' `
        -Name $formName `
        -FormType 'Main' `
        -FormXmlContent $formXml `
        -Publish
}
```

### Example: Manage form tabs

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Complex manual FormXml manipulation required
$form = Get-CrmRecord -conn $conn -EntityLogicalName systemform -Id $formId -Fields formxml
$xml = [xml]$form.formxml
$tab = $xml.CreateElement("tab")
$tab.SetAttribute("name", "newtab")
# ... extensive XML manipulation ...
Set-CrmRecord -conn $conn -CrmRecord @{systemformid=$formId; formxml=$xml.OuterXml}
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Simple and intuitive with dedicated cmdlet
# Add a new tab
$tabId = Set-DataverseFormTab -Connection $conn `
    -FormId $formId `
    -Name 'CustomTab' `
    -Label 'Custom Information' `
    -Expanded `
    -Publish `
    -PassThru

# Update existing tab
Set-DataverseFormTab -Connection $conn `
    -FormId $formId `
    -TabId $tabId `
    -Label 'Updated Label' `
    -Visible:$false

# Remove tab
Remove-DataverseFormTab -Connection $conn -FormId $formId -TabName 'CustomTab' -Publish

# Get all tabs
$tabs = Get-DataverseFormTab -Connection $conn -FormId $formId
$tabs | Format-Table Name, Expanded, Visible

# Position tabs using Index, InsertBefore, or InsertAfter
Set-DataverseFormTab -Connection $conn `
    -FormId $formId `
    -Name 'FirstTab' `
    -Label 'First Tab' `
    -Index 0  # Insert at beginning

Set-DataverseFormTab -Connection $conn `
    -FormId $formId `
    -Name 'MiddleTab' `
    -Label 'Middle Tab' `
    -InsertBefore 'GeneralTab'  # Insert before specific tab

Set-DataverseFormTab -Connection $conn `
    -FormId $formId `
    -Name 'AfterTab' `
    -Label 'After Tab' `
    -InsertAfter 'GeneralTab'  # Insert after specific tab
```

### Example: Manage form sections

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Requires manual FormXml parsing and manipulation
$form = Get-CrmRecord -conn $conn -EntityLogicalName systemform -Id $formId -Fields formxml
$xml = [xml]$form.formxml
# ... complex XPath and XML manipulation ...
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Simple section management
# Add a new section to a tab
$sectionId = Set-DataverseFormSection -Connection $conn `
    -FormId $formId `
    -TabName 'General' `
    -Name 'custom_section' `
    -Label 'Custom Fields' `
    -Columns 2 `
    -ShowLabel `
    -Publish `
    -PassThru

# Update section properties
Set-DataverseFormSection -Connection $conn `
    -FormId $formId `
    -TabName 'General' `
    -SectionId $sectionId `
    -Label 'Updated Section' `
    -Columns 3

# Remove section
Remove-DataverseFormSection -Connection $conn `
    -FormId $formId `
    -SectionName 'custom_section' `
    -Publish

# Get all sections in a tab
$sections = Get-DataverseFormSection -Connection $conn -FormId $formId -TabName 'General'
$sections | Format-Table Name, ShowLabel, Columns

# Position sections using Index, InsertBefore, or InsertAfter
Set-DataverseFormSection -Connection $conn `
    -FormId $formId `
    -TabName 'General' `
    -Name 'top_section' `
    -Label 'Top Section' `
    -Index 0  # Insert at beginning

Set-DataverseFormSection -Connection $conn `
    -FormId $formId `
    -TabName 'General' `
    -Name 'middle_section' `
    -Label 'Middle Section' `
    -InsertAfter 'general_section'  # Insert after specific section
```

### Example: Manage form controls

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Very complex - requires understanding FormXml schema and manual manipulation
$form = Get-CrmRecord -conn $conn -EntityLogicalName systemform -Id $formId -Fields formxml
$xml = [xml]$form.formxml
# ... extensive XML creation with class IDs, attributes, cells, rows ...
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Add standard controls with simple parameters
# Add a text field
Set-DataverseFormControl -Connection $conn `
    -FormId $formId `
    -SectionName 'general_section' `
    -DataField 'emailaddress1' `
    -Label 'Email Address' `
    -ControlType 'Standard' `
    -IsRequired `
    -Publish

# Add a lookup field
Set-DataverseFormControl -Connection $conn `
    -FormId $formId `
    -SectionName 'general_section' `
    -DataField 'parentcustomerid' `
    -Label 'Parent Account' `
    -ControlType 'Lookup'

# Add a date/time picker
Set-DataverseFormControl -Connection $conn `
    -FormId $formId `
    -SectionName 'general_section' `
    -DataField 'createdon' `
    -Label 'Created On' `
    -ControlType 'DateTime' `
    -Disabled

# Add a subgrid with parameters
Set-DataverseFormControl -Connection $conn `
    -FormId $formId `
    -SectionName 'related_section' `
    -DataField 'contacts_subgrid' `
    -ControlType 'Subgrid' `
    -Parameters @{
        TargetEntityType = 'contact'
        ViewId = '{00000000-0000-0000-0000-000000000000}'
    }

# Update control properties
Set-DataverseFormControl -Connection $conn `
    -FormId $formId `
    -SectionName 'general_section' `
    -ControlId 'emailaddress1' `
    -DataField 'emailaddress1' `
    -Label 'Primary Email' `
    -Visible:$false

# Remove control
Remove-DataverseFormControl -Connection $conn `
    -FormId $formId `
    -DataField 'emailaddress1' `
    -Publish

# Get all controls in a section
$controls = Get-DataverseFormControl -Connection $conn `
    -FormId $formId `
    -SectionName 'general_section'
$controls | Format-Table DataField, Disabled, Visible, IsRequired

# Position controls using Index, InsertBefore, or InsertAfter
Set-DataverseFormControl -Connection $conn `
    -FormId $formId `
    -SectionName 'general_section' `
    -DataField 'firstname' `
    -Label 'First Name' `
    -Index 0  # Insert at beginning

Set-DataverseFormControl -Connection $conn `
    -FormId $formId `
    -SectionName 'general_section' `
    -DataField 'lastname' `
    -Label 'Last Name' `
    -InsertAfter 'firstname'  # Insert after firstname

Set-DataverseFormControl -Connection $conn `
    -FormId $formId `
    -SectionName 'general_section' `
    -DataField 'emailaddress1' `
    -Label 'Email' `
    -InsertBefore 'telephone1'  # Insert before phone

# Use raw XML for advanced control customization
$controlXml = @'
<control id="customfield" datafieldname="new_customfield" classid="{4273EDBD-AC1D-40d3-9FB2-095C621B552D}">
  <labels>
    <label description="Custom Field" languagecode="1033" />
  </labels>
  <parameters>
    <MaxLength>100</MaxLength>
    <Format>Text</Format>
  </parameters>
</control>
'@

Set-DataverseFormControl -Connection $conn `
    -FormId $formId `
    -SectionName 'general_section' `
    -ControlXml $controlXml `
    -InsertAfter 'emailaddress1' `
    -Publish

# Supported control types: Standard, Lookup, OptionSet, DateTime, Boolean, 
# Subgrid, WebResource, QuickForm, Spacer, IFrame, Timer, KBSearch, Notes
```

## Ribbon Customizations

### Example: Export Application Ribbon

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$exportPath = "C:\RibbonExports"
Export-CrmApplicationRibbonXml -conn $conn -RibbonFilePath $exportPath
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
$exportPath = "C:\RibbonExports"

# Using specialized cmdlet (simplest)
$response = Invoke-DataverseRetrieveApplicationRibbon -Connection $conn

# Save the ribbon XML
$ribbonXml = $response.CompressedApplicationRibbonXml
[System.IO.File]::WriteAllBytes("$exportPath\ApplicationRibbon.xml", $ribbonXml)
```

### Example: Export Entity Ribbons

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$entities = @("account", "contact", "opportunity", "lead")
$exportPath = "C:\RibbonExports"

foreach($entity in $entities) {
    Export-CrmEntityRibbonXml -conn $conn -EntityLogicalName $entity -RibbonFilePath $exportPath
}
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
$entities = @("account", "contact", "opportunity", "lead")
$exportPath = "C:\RibbonExports"

foreach($entity in $entities) {
    # Using specialized cmdlet (simplest)
    $response = Invoke-DataverseRetrieveEntityRibbon -Connection $conn `
        -EntityName $entity `
        -RibbonLocationFilter ([Microsoft.Crm.Sdk.Messages.RibbonLocationFilters]::All)
    
    # Save the ribbon XML
    $ribbonXml = $response.CompressedEntityXml
    [System.IO.File]::WriteAllBytes("$exportPath\$entity-Ribbon.xml", $ribbonXml)
}
```

### Example: List All Ribbon-Enabled Entities

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$entities = Get-CrmEntityAllMetadata -conn $conn -EntityFilters Entity
$ribbonEnabled = $entities | Where-Object { $_.IsCustomizable.Value -and $_.IsValidForAdvancedFind.Value }
$ribbonEnabled.LogicalName
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using SQL to query metadata
$ribbonEnabled = Invoke-DataverseSql -Connection $conn -Sql @"
SELECT name, displayname
FROM metadata.entity
WHERE iscustomizable = 1 
AND isvalidforadvancedfind = 1
AND ismanaged = 0
ORDER BY name
"@

$ribbonEnabled | Format-Table
```

## Views and Quick Find

### Example: Get Quick Find Search Fields

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Get all QuickFind views (querytype of 4)
$views = Get-CrmRecords -EntityLogicalName savedquery `
    -FilterAttribute querytype -FilterOperator eq -FilterValue 4 `
    -Fields name,fetchxml,returnedtypecode

$results = @()
foreach($view in $views.CrmRecords) {
    $entityname = $view.returnedtypecode
    $xml = [xml]$view.fetchxml
    $filters = $xml.fetch.entity.filter.condition | Where-Object { $_.value -eq "{0}" }
    
    $results += [PSCustomObject]@{
        Entity = $entityname
        SearchFieldCount = $filters.Count
        SearchFields = ($filters.attribute -join ", ")
    }
}

$results | Sort-Object Entity | Format-Table
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Get all QuickFind views using SQL
$views = Invoke-DataverseSql -Connection $conn -Sql @"
SELECT savedqueryid, name, fetchxml, returnedtypecode
FROM savedquery
WHERE querytype = 4
ORDER BY returnedtypecode
"@

$results = @()
foreach($view in $views) {
    $xml = [xml]$view.fetchxml
    $filters = $xml.fetch.entity.filter.condition | Where-Object { $_.value -eq "{0}" }
    
    $results += [PSCustomObject]@{
        Entity = $view.returnedtypecode
        ViewName = $view.name
        SearchFieldCount = $filters.Count
        SearchFields = ($filters.attribute -join ", ")
    }
}

$results | Format-Table
```

### Example: Get All Views for an Entity

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$views = Get-CrmRecords -EntityLogicalName savedquery `
    -FilterAttribute returnedtypecode -FilterOperator eq -FilterValue "account" `
    -Fields name,querytype,isdefault
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using Get-DataverseRecord
$views = Get-DataverseRecord -Connection $conn -TableName savedquery `
    -Filter @{returnedtypecode = "account"} `
    -Columns name,querytype,isdefault

# Or using SQL for richer queries
$views = Invoke-DataverseSql -Connection $conn -Sql @"
SELECT 
    name,
    CASE querytype
        WHEN 0 THEN 'Saved View'
        WHEN 1 THEN 'Quick Find'
        WHEN 2 THEN 'Advanced Find'
        WHEN 4 THEN 'Quick Find'
        WHEN 8 THEN 'Lookup'
        ELSE 'Other'
    END as viewtype,
    isdefault,
    isquickfindquery
FROM savedquery
WHERE returnedtypecode = 'account'
ORDER BY name
"@

$views | Format-Table
```

### Example: Get Personal Views for Current User

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$userId = (Invoke-CrmWhoAmI).UserId
$personalViews = Get-CrmRecords -EntityLogicalName userquery `
    -FilterAttribute ownerid -FilterOperator eq -FilterValue $userId `
    -Fields name,returnedtypecode
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
$whoami = Get-DataverseWhoAmI -Connection $conn
$userId = $whoami.UserId

# Using SQL
$personalViews = Invoke-DataverseSql -Connection $conn -Sql @"
SELECT name, returnedtypecode, fetchxml
FROM userquery
WHERE ownerid = '$userId'
ORDER BY returnedtypecode, name
"@

$personalViews | Format-Table
```

### Example: Create a Custom View

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Requires complex SDK object creation
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using specific cmdlet (recommended)
Set-DataverseView -Connection $conn -PassThru `
    -TableName contact `
    -Name "Active Contacts" `
    -Columns @("fullname", "emailaddress1", "telephone1", "createdon") `
    -FetchXml @"
<fetch>
  <entity name='contact'>
    <attribute name='fullname' />
    <attribute name='emailaddress1' />
    <attribute name='telephone1' />
    <attribute name='createdon' />
    <filter>
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>
"@
```

### Example: Update an Existing View

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Requires complex SDK object creation and updates
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using specific cmdlet (recommended)
# Get existing view
$view = Get-DataverseView -Connection $conn -TableName contact -Name "Active Contacts"

# Update the view
Set-DataverseView -Connection $conn -Id $view.Id `
    -Name "Active Contacts (Updated)" `
    -Columns @("fullname", "emailaddress1", "telephone1", "createdon", "modifiedon")
```

### Example: Delete a View

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Requires finding the view and deleting the record
$view = Get-CrmRecords -EntityLogicalName savedquery `
    -FilterAttribute name -FilterOperator eq -FilterValue "My Custom View"
if ($view.Count -eq 1) {
    Remove-CrmRecord -EntityLogicalName savedquery -Id $view.CrmRecords[0].ReturnProperty_Id
}
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using specific cmdlet (recommended)
Remove-DataverseView -Connection $conn -Name "My Custom View" -Confirm:$false
```

## Sitemap Management

Sitemaps define the navigation structure for model-driven apps in Dataverse. They contain Area, Group, and SubArea elements that define the app's navigation hierarchy.

### Example: Retrieve All Sitemaps

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# No specialized cmdlet - must use generic record query
$sitemaps = Get-CrmRecords -EntityLogicalName sitemap -Fields sitemapname,sitemapxml,ismanaged
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using specialized cmdlet (simpler)
$sitemaps = Get-DataverseSitemap -Connection $conn

# Or using generic record cmdlet
$sitemaps = Get-DataverseRecord -Connection $conn -TableName sitemap -Columns sitemapname,sitemapxml,ismanaged

# Or using SQL
$sitemaps = Invoke-DataverseSql -Connection $conn -Sql @"
SELECT sitemapid, sitemapname, ismanaged, createdon, modifiedon
FROM sitemap
ORDER BY sitemapname
"@
```

### Example: Get Sitemap for a Specific App

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Must use FetchXML with link to appmodule
$fetchXml = @"
<fetch>
  <entity name='sitemap'>
    <attribute name='sitemapname' />
    <attribute name='sitemapxml' />
    <link-entity name='appmodule' from='appmoduleid' to='sitemapid'>
      <filter>
        <condition attribute='uniquename' operator='eq' value='myapp' />
      </filter>
    </link-entity>
  </entity>
</fetch>
"@
$sitemap = Get-CrmRecordsByFetch -conn $conn -Fetch $fetchXml
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using specialized cmdlet with filtering (simplest)
$sitemap = Get-DataverseSitemap -Connection $conn -AppUniqueName "myapp"

# Or using SQL
$sitemap = Invoke-DataverseSql -Connection $conn -Sql @"
SELECT s.sitemapid, s.sitemapname, s.sitemapxml, a.uniquename as appuniquename
FROM sitemap s
INNER JOIN appmodule a ON s.sitemapid = a.appmoduleid
WHERE a.uniquename = 'myapp'
"@
```

### Example: Export Sitemap to File

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$sitemap = Get-CrmRecords -EntityLogicalName sitemap `
    -FilterAttribute sitemapname -FilterOperator eq -FilterValue "MySitemap" `
    -Fields sitemapxml

$sitemap.CrmRecords[0].sitemapxml | Out-File -FilePath "MySitemap.xml"
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using specialized cmdlet
$sitemap = Get-DataverseSitemap -Connection $conn -Name "MySitemap"
$sitemap.SitemapXml | Out-File -FilePath "MySitemap.xml"

# Or using generic cmdlet
$sitemap = Get-DataverseRecord -Connection $conn -TableName sitemap `
    -Filter @{sitemapname = "MySitemap"} -Columns sitemapxml

$sitemap.sitemapxml | Out-File -FilePath "MySitemap.xml"
```

### Example: Create a New Sitemap

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$sitemapXml = @"
<SiteMap>
  <Area Id="Area1" ResourceId="Area1.Title">
    <Group Id="Group1" ResourceId="Group1.Title">
      <SubArea Id="SubArea1" ResourceId="SubArea1.Title" Entity="account" />
    </Group>
  </Area>
</SiteMap>
"@

$entity = New-Object Microsoft.Xrm.Sdk.Entity("sitemap")
$entity.Attributes["sitemapname"] = "MySitemap"
$entity.Attributes["sitemapxml"] = $sitemapXml
$sitemapId = $conn.Create($entity)
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
$sitemapXml = @"
<SiteMap>
  <Area Id="Area1" ResourceId="Area1.Title">
    <Group Id="Group1" ResourceId="Group1.Title">
      <SubArea Id="SubArea1" ResourceId="SubArea1.Title" Entity="account" />
    </Group>
  </Area>
</SiteMap>
"@

# Using specialized cmdlet (simplest)
$sitemapId = Set-DataverseSitemap -Connection $conn -Name "MySitemap" -SitemapXml $sitemapXml -PassThru

# Or using generic cmdlet
$sitemapId = Set-DataverseRecord -Connection $conn -TableName sitemap -Fields @{
    sitemapname = "MySitemap"
    sitemapxml = $sitemapXml
}
```

### Example: Update an Existing Sitemap

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Query for the sitemap
$sitemap = Get-CrmRecords -EntityLogicalName sitemap `
    -FilterAttribute sitemapname -FilterOperator eq -FilterValue "MySitemap" `
    -Fields sitemapid,sitemapxml

$sitemapId = $sitemap.CrmRecords[0].sitemapid

# Modify the XML
$xml = [xml]$sitemap.CrmRecords[0].sitemapxml
$newArea = $xml.CreateElement("Area")
$newArea.SetAttribute("Id", "Area2")
$xml.SiteMap.AppendChild($newArea)

# Update
$entity = New-Object Microsoft.Xrm.Sdk.Entity("sitemap", $sitemapId)
$entity.Attributes["sitemapxml"] = $xml.OuterXml
$conn.Update($entity)
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using specialized cmdlet (simplest)
$sitemap = Get-DataverseSitemap -Connection $conn -Name "MySitemap"

# Modify the XML
$xml = [xml]$sitemap.SitemapXml
$newArea = $xml.CreateElement("Area")
$newArea.SetAttribute("Id", "Area2")
$xml.SiteMap.AppendChild($newArea)

# Update using specialized cmdlet
Set-DataverseSitemap -Connection $conn -Name "MySitemap" -Id $sitemap.Id -SitemapXml $xml.OuterXml

# Or using generic cmdlet
Set-DataverseRecord -Connection $conn -TableName sitemap -Id $sitemap.Id -Fields @{
    sitemapxml = $xml.OuterXml
}
```

### Example: Import Sitemap from File

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$sitemapXml = Get-Content -Path "MySitemap.xml" -Raw

$entity = New-Object Microsoft.Xrm.Sdk.Entity("sitemap")
$entity.Attributes["sitemapname"] = "ImportedSitemap"
$entity.Attributes["sitemapxml"] = $sitemapXml
$sitemapId = $conn.Create($entity)
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
$sitemapXml = Get-Content -Path "MySitemap.xml" -Raw

# Using specialized cmdlet (simplest)
$sitemapId = Set-DataverseSitemap -Connection $conn `
    -Name "ImportedSitemap" `
    -SitemapXml $sitemapXml `
    -PassThru

# Or using generic cmdlet
$sitemapId = Set-DataverseRecord -Connection $conn -TableName sitemap -Fields @{
    sitemapname = "ImportedSitemap"
    sitemapxml = $sitemapXml
}
```

### Example: Delete a Sitemap

**Microsoft.Xrm.Data.PowerShell:**
```powershell
# Query for the sitemap
$sitemap = Get-CrmRecords -EntityLogicalName sitemap `
    -FilterAttribute sitemapname -FilterOperator eq -FilterValue "MySitemap"

if ($sitemap.CrmRecords.Count -gt 0) {
    $sitemapId = $sitemap.CrmRecords[0].sitemapid
    $conn.Delete("sitemap", $sitemapId)
}
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using specialized cmdlet (simplest)
Remove-DataverseSitemap -Connection $conn -Name "MySitemap"

# Or by ID
Remove-DataverseSitemap -Connection $conn -Id $sitemapId

# Or using generic cmdlet
Remove-DataverseRecord -Connection $conn -TableName sitemap -MatchOn @(@("sitemapname")) -InputObject @{
    sitemapname = "MySitemap"
}
```

### Example: Clone a Sitemap

**Microsoft.Xrm.Data.PowerShell:**
```powershell
$source = Get-CrmRecords -EntityLogicalName sitemap `
    -FilterAttribute sitemapname -FilterOperator eq -FilterValue "OriginalSitemap" `
    -Fields sitemapxml

$entity = New-Object Microsoft.Xrm.Sdk.Entity("sitemap")
$entity.Attributes["sitemapname"] = "ClonedSitemap"
$entity.Attributes["sitemapxml"] = $source.CrmRecords[0].sitemapxml
$newId = $conn.Create($entity)
```

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Using specialized cmdlets (simplest)
$source = Get-DataverseSitemap -Connection $conn -Name "OriginalSitemap"
$newId = Set-DataverseSitemap -Connection $conn `
    -Name "ClonedSitemap" `
    -SitemapXml $source.SitemapXml `
    -PassThru

# Or using generic cmdlets
$source = Get-DataverseRecord -Connection $conn -TableName sitemap `
    -Filter @{sitemapname = "OriginalSitemap"} -Columns sitemapxml

$newId = Set-DataverseRecord -Connection $conn -TableName sitemap -Fields @{
    sitemapname = "ClonedSitemap"
    sitemapxml = $source.sitemapxml
}
```

### Example: Add Navigation Entry to Sitemap (Using XML)

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Retrieve sitemap
$sitemap = Get-DataverseSitemap -Connection $conn -Name "MySitemap"

# Parse and modify XML
$xml = [xml]$sitemap.SitemapXml

# Find or create area
$area = $xml.SelectSingleNode("//Area[@Id='SalesArea']")
if ($null -eq $area) {
    $area = $xml.CreateElement("Area")
    $area.SetAttribute("Id", "SalesArea")
    $area.SetAttribute("ResourceId", "SalesArea.Title")
    $xml.SiteMap.AppendChild($area)
}

# Add group
$group = $xml.CreateElement("Group")
$group.SetAttribute("Id", "CustomersGroup")
$group.SetAttribute("ResourceId", "CustomersGroup.Title")
$area.AppendChild($group)

# Add subarea (navigation entry)
$subarea = $xml.CreateElement("SubArea")
$subarea.SetAttribute("Id", "AccountsSubarea")
$subarea.SetAttribute("ResourceId", "AccountsSubarea.Title")
$subarea.SetAttribute("Entity", "account")
$subarea.SetAttribute("Icon", "/_imgs/area/sales_24x24.gif")
$group.AppendChild($subarea)

# Update sitemap
Set-DataverseSitemap -Connection $conn -Name "MySitemap" -Id $sitemap.Id -SitemapXml $xml.OuterXml
```

### Example: Work with Sitemap Entries (Using Entry Cmdlets)

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Get all entries from a sitemap
$entries = Get-DataverseSitemapEntry -Connection $conn -SitemapName "MySitemap"

# Get only Areas
$areas = Get-DataverseSitemapEntry -Connection $conn -SitemapName "MySitemap" -EntryType Area

# Get a specific entry
$salesArea = Get-DataverseSitemapEntry -Connection $conn -SitemapName "MySitemap" -EntryId "SalesArea"

# Get Groups within an Area
$groups = Get-DataverseSitemapEntry -Connection $conn -SitemapName "MySitemap" -EntryType Group -ParentAreaId "SalesArea"

# Get SubAreas within a Group
$subareas = Get-DataverseSitemapEntry -Connection $conn -SitemapName "MySitemap" -EntryType SubArea -ParentAreaId "SalesArea" -ParentGroupId "CustomersGroup"
```

### Example: Add New Navigation Entry (Using Entry Cmdlet)

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Add a new Area
Set-DataverseSitemapEntry -Connection $conn -SitemapName "MySitemap" `
    -EntryType Area -EntryId "ServiceArea" `
    -Title "Service" -ResourceId "ServiceArea.Title" `
    -Icon "/_imgs/area/service_24x24.gif"

# Add a Group to the Area
Set-DataverseSitemapEntry -Connection $conn -SitemapName "MySitemap" `
    -EntryType Group -EntryId "CasesGroup" `
    -ParentAreaId "ServiceArea" `
    -Title "Cases" -ResourceId "CasesGroup.Title"

# Add a SubArea to the Group
Set-DataverseSitemapEntry -Connection $conn -SitemapName "MySitemap" `
    -EntryType SubArea -EntryId "CasesSubarea" `
    -ParentAreaId "ServiceArea" -ParentGroupId "CasesGroup" `
    -Entity "incident" `
    -Title "Cases" -ResourceId "CasesSubarea.Title"

# Use pipeline to add entry
Get-DataverseSitemap -Connection $conn -Name "MySitemap" | 
    Set-DataverseSitemapEntry -EntryType Area -EntryId "MarketingArea" -Title "Marketing"
```

### Example: Update Navigation Entry

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Update entry properties
Set-DataverseSitemapEntry -Connection $conn -SitemapName "MySitemap" `
    -EntryType Area -EntryId "SalesArea" `
    -Title "Sales & Marketing" -Icon "/_imgs/area/sales_new.gif"

# Update SubArea entity
Set-DataverseSitemapEntry -Connection $conn -SitemapName "MySitemap" `
    -EntryType SubArea -EntryId "AccountsSubarea" `
    -ParentAreaId "SalesArea" -ParentGroupId "CustomersGroup" `
    -Entity "account" -Title "Customer Accounts"

# Use pipeline to update
Get-DataverseSitemapEntry -Connection $conn -SitemapName "MySitemap" -EntryId "SalesArea" |
    Set-DataverseSitemapEntry -Title "Sales Department"
```

### Example: Remove Navigation Entry

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Remove a SubArea
Remove-DataverseSitemapEntry -Connection $conn -SitemapName "MySitemap" `
    -EntryType SubArea -EntryId "OldSubarea" `
    -ParentAreaId "SalesArea" -ParentGroupId "CustomersGroup"

# Remove an Area (and all its children)
Remove-DataverseSitemapEntry -Connection $conn -SitemapName "MySitemap" `
    -EntryType Area -EntryId "ObsoleteArea"

# Use pipeline to remove
Get-DataverseSitemapEntry -Connection $conn -SitemapName "MySitemap" -EntryId "TempArea" |
    Remove-DataverseSitemapEntry -Confirm:$false
```

### Example: Complex Sitemap Reorganization

**Rnwood.Dataverse.Data.PowerShell:**
```powershell
# Get all SubAreas from one Group
$oldSubareas = Get-DataverseSitemapEntry -Connection $conn -SitemapName "MySitemap" `
    -EntryType SubArea -ParentAreaId "SalesArea" -ParentGroupId "OldGroup"

# Process each and recreate in new group
foreach ($subarea in $oldSubareas) {
    # Add to new group
    Set-DataverseSitemapEntry -Connection $conn -SitemapName "MySitemap" `
        -EntryType SubArea -EntryId "$($subarea.Id)_new" `
        -ParentAreaId "SalesArea" -ParentGroupId "NewGroup" `
        -Entity $subarea.Entity -Title $subarea.Title `
        -Icon $subarea.Icon
    
    # Remove from old group
    Remove-DataverseSitemapEntry -Connection $conn -SitemapName "MySitemap" `
        -EntryType SubArea -EntryId $subarea.Id `
        -ParentAreaId "SalesArea" -ParentGroupId "OldGroup" -Confirm:$false
}
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

### Specialized Cmdlets
**Microsoft.Xrm.Data.PowerShell** has limited specialized cmdlets.  
**Rnwood.Dataverse.Data.PowerShell** provides specialized cmdlets for common operations like sitemap management, solution management, and more, making these tasks simpler and more intuitive.

## See Also

- [Get-DataverseConnection](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseConnection.md)
- [Get-DataverseRecord](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md)
- [Set-DataverseRecord](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md)
- [Remove-DataverseRecord](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseRecord.md)
- [Invoke-DataverseRequest](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseRequest.md)
- [Invoke-DataverseSql](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseSql.md)
- [Get-DataverseSitemap](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseSitemap.md)
- [Set-DataverseSitemap](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseSitemap.md)
- [Remove-DataverseSitemap](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseSitemap.md)
- [Get-DataverseSitemapEntry](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseSitemapEntry.md)
- [Set-DataverseSitemapEntry](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseSitemapEntry.md)
- [Remove-DataverseSitemapEntry](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseSitemapEntry.md)
