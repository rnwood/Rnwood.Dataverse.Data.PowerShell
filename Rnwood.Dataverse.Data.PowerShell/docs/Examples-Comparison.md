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
- [Solution Management](#solution-management)
- [User and Team Operations](#user-and-team-operations)
- [Plugin and Workflow Management](#plugin-and-workflow-management)
- [Advanced Query Scenarios](#advanced-query-scenarios)

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

# Create export request
$request = New-Object Microsoft.Crm.Sdk.Messages.ExportSolutionRequest
$request.SolutionName = $solutionName
$request.Managed = $false

$response = Invoke-DataverseRequest -Connection $conn -Request $request

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

$request = New-Object Microsoft.Crm.Sdk.Messages.ImportSolutionRequest
$request.CustomizationFile = $solutionBytes
$request.PublishWorkflows = $true
$request.OverwriteUnmanagedCustomizations = $false

Invoke-DataverseRequest -Connection $conn -Request $request
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
# Create the team membership association
$request = New-Object Microsoft.Xrm.Sdk.Messages.AddMembersTeamRequest
$request.TeamId = $teamId
$request.MemberIds = @($userId)

Invoke-DataverseRequest -Connection $conn -Request $request
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

# Disable each user
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

# Disable all steps
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
