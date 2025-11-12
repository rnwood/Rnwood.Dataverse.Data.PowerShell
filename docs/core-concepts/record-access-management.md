<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
## Table of Contents

- [Record Access Management](#record-access-management)
  - [Overview](#overview)
  - [Test-DataverseRecordAccess](#test-dataverserecordaccess)
    - [Basic Usage](#basic-usage)
    - [Check Specific Permissions](#check-specific-permissions)
    - [Access Rights Values](#access-rights-values)
  - [Set-DataverseRecordAccess](#set-dataverserecordaccess)
    - [Additive Mode (Default)](#additive-mode-default)
    - [Replace Mode](#replace-mode)
    - [Combining Multiple Rights](#combining-multiple-rights)
    - [Working with Teams](#working-with-teams)
    - [WhatIf and Confirm Support](#whatif-and-confirm-support)
  - [Get-DataverseRecordAccess](#get-dataverserecordaccess)
    - [Basic Usage](#basic-usage-1)
    - [Filter by Access Rights](#filter-by-access-rights)
    - [Pipeline Usage](#pipeline-usage)
  - [Remove-DataverseRecordAccess](#remove-dataverserecordaccess)
    - [Basic Usage](#basic-usage-2)
    - [Skip Confirmation](#skip-confirmation)
    - [Bulk Operations](#bulk-operations)
  - [Complete Workflow Example](#complete-workflow-example)
  - [Common Use Cases](#common-use-cases)
    - [Audit Who Has Access to Sensitive Records](#audit-who-has-access-to-sensitive-records)
    - [Bulk Share Records with a Team](#bulk-share-records-with-a-team)
    - [Implement Custom Security Logic](#implement-custom-security-logic)
  - [Best Practices](#best-practices)
  - [Related Documentation](#related-documentation)
  - [API Reference](#api-reference)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

# Record Access Management

Dataverse provides row-level security through record sharing, allowing you to grant specific access rights to individual users or teams on specific records. This module provides four cmdlets to manage record access programmatically.

## Overview

Record access management allows you to:
- **Test** what access rights a user or team has for a specific record
- **Grant** or **modify** access rights for a user or team on a record
- **List** all users and teams who have shared access to a record
- **Revoke** shared access from a user or team

**Important Notes:**
- These cmdlets manage **explicit shared access** only
- They do not affect access granted through:
  - Record ownership
  - Security roles
  - Team membership
  - Hierarchy security
- All cmdlets use simple `TableName` and `Id` parameters instead of complex SDK EntityReference objects

## Test-DataverseRecordAccess

Tests what access rights a security principal (user or team) has for a specific record.

### Basic Usage

```powershell
# Get current user's access to a record
$whoAmI = Get-DataverseWhoAmI -Connection $connection
$access = Test-DataverseRecordAccess -Connection $connection -TableName account -Id $accountId -Principal $whoAmI.UserId

# Display access rights
Write-Host "User has these rights: $access"
```

### Check Specific Permissions

```powershell
# Check if user has write access using bitwise operations
$access = Test-DataverseRecordAccess -Connection $connection -TableName contact -Id $contactId -Principal $userId

$hasWrite = ($access -band [Microsoft.Crm.Sdk.Messages.AccessRights]::WriteAccess) -ne 0
if ($hasWrite) {
    Write-Host "User has write access"
}
```

### Access Rights Values

The cmdlet returns an `AccessRights` enum which can include:
- `None` - No access
- `ReadAccess` - Read access
- `WriteAccess` - Write access
- `AppendAccess` - Append access
- `AppendToAccess` - Append to access
- `CreateAccess` - Create access
- `DeleteAccess` - Delete access
- `ShareAccess` - Share access
- `AssignAccess` - Assign access

## Set-DataverseRecordAccess

Grants or modifies access rights for a security principal on a record. The cmdlet has two modes:

### Additive Mode (Default)

By default, the cmdlet **adds** the specified rights to any existing rights the principal has.

```powershell
# Grant read access
Set-DataverseRecordAccess -Connection $connection -TableName account -Id $accountId -Principal $userId -AccessRights ReadAccess

# Add write access (user now has both Read and Write)
Set-DataverseRecordAccess -Connection $connection -TableName account -Id $accountId -Principal $userId -AccessRights WriteAccess
```

### Replace Mode

Use the `-Replace` switch to **replace** all existing rights with only the specified rights.

```powershell
# Replace all access with only DeleteAccess (removes Read and Write if they existed)
Set-DataverseRecordAccess -Connection $connection -TableName account -Id $accountId -Principal $userId -AccessRights DeleteAccess -Replace
```

### Combining Multiple Rights

```powershell
# Grant multiple rights at once using bitwise OR
$accessRights = [Microsoft.Crm.Sdk.Messages.AccessRights]::ReadAccess -bor [Microsoft.Crm.Sdk.Messages.AccessRights]::WriteAccess
Set-DataverseRecordAccess -Connection $connection -TableName opportunity -Id $oppId -Principal $userId -AccessRights $accessRights
```

### Working with Teams

```powershell
# Grant access to a team instead of a user
Set-DataverseRecordAccess -Connection $connection -TableName contact -Id $contactId -Principal $teamId -AccessRights ReadAccess -IsTeam
```

### WhatIf and Confirm Support

```powershell
# Preview changes without executing
Set-DataverseRecordAccess -Connection $connection -TableName account -Id $accountId -Principal $userId -AccessRights WriteAccess -WhatIf

# Skip confirmation prompt (default confirms for Medium impact operations)
Set-DataverseRecordAccess -Connection $connection -TableName account -Id $accountId -Principal $userId -AccessRights WriteAccess -Confirm:$false
```

## Get-DataverseRecordAccess

Lists all security principals (users or teams) who have explicit shared access to a record.

### Basic Usage

```powershell
# Get all principals with access
$accessList = Get-DataverseRecordAccess -Connection $connection -TableName account -Id $accountId

# Display each principal and their access
foreach ($access in $accessList) {
    Write-Host "Principal: $($access.Principal.Id)"
    Write-Host "Type: $($access.Principal.LogicalName)"
    Write-Host "Access: $($access.AccessMask)"
    Write-Host ""
}
```

### Filter by Access Rights

```powershell
# Find all principals with write access
$accessList = Get-DataverseRecordAccess -Connection $connection -TableName contact -Id $contactId
$writeAccess = $accessList | Where-Object {
    ($_.AccessMask -band [Microsoft.Crm.Sdk.Messages.AccessRights]::WriteAccess) -ne 0
}
```

### Pipeline Usage

```powershell
# Check access across multiple records
$opportunities = Get-DataverseRecord -Connection $connection -TableName opportunity -FilterValues @{statecode=0}
$opportunities | ForEach-Object {
    Get-DataverseRecordAccess -Connection $connection -TableName opportunity -Id $_.opportunityid
}
```

## Remove-DataverseRecordAccess

Revokes all shared access rights from a security principal on a record.

### Basic Usage

```powershell
# Revoke access from a user
Remove-DataverseRecordAccess -Connection $connection -TableName account -Id $accountId -Principal $userId

# Revoke access from a team
Remove-DataverseRecordAccess -Connection $connection -TableName contact -Id $contactId -Principal $teamId -IsTeam
```

### Skip Confirmation

```powershell
# Skip confirmation prompt (default confirms for Medium impact operations)
Remove-DataverseRecordAccess -Connection $connection -TableName account -Id $accountId -Principal $userId -Confirm:$false
```

### Bulk Operations

```powershell
# Remove shared access from all principals except current user
$accessList = Get-DataverseRecordAccess -Connection $connection -TableName opportunity -Id $oppId
$whoAmI = Get-DataverseWhoAmI -Connection $connection

foreach ($access in $accessList) {
    if ($access.Principal.Id -ne $whoAmI.UserId) {
        Remove-DataverseRecordAccess -Connection $connection -TableName opportunity -Id $oppId -Principal $access.Principal.Id -Confirm:$false
    }
}
```

## Complete Workflow Example

Here's a complete example demonstrating all four cmdlets:

```powershell
# Connect to Dataverse
$connection = Get-DataverseConnection -url https://myorg.crm.dynamics.com -ClientId $clientId -ClientSecret $clientSecret

# Get current user and find another user
$whoAmI = Get-DataverseWhoAmI -Connection $connection
$otherUser = Get-DataverseRecord -Connection $connection -TableName systemuser -FilterValues @{isdisabled=$false} | 
    Where-Object { $_.systemuserid -ne $whoAmI.UserId } | 
    Select-Object -First 1

# Create a test record
$account = @{
    name = "Test Account"
    description = "For access management demo"
} | Set-DataverseRecord -Connection $connection -TableName account -CreateOnly -PassThru

$accountId = $account.Id

# Grant read and write access to the other user
$accessRights = [Microsoft.Crm.Sdk.Messages.AccessRights]::ReadAccess -bor [Microsoft.Crm.Sdk.Messages.AccessRights]::WriteAccess
Set-DataverseRecordAccess -Connection $connection -TableName account -Id $accountId -Principal $otherUser.systemuserid -AccessRights $accessRights -Confirm:$false

# Test the access that was granted
$access = Test-DataverseRecordAccess -Connection $connection -TableName account -Id $accountId -Principal $otherUser.systemuserid
Write-Host "Access granted: $access"

# List all principals with access
$allAccess = Get-DataverseRecordAccess -Connection $connection -TableName account -Id $accountId
Write-Host "Total principals with access: $($allAccess.Count)"

# Replace access with only delete permission
Set-DataverseRecordAccess -Connection $connection -TableName account -Id $accountId -Principal $otherUser.systemuserid -AccessRights DeleteAccess -Replace -Confirm:$false

# Verify the change
$newAccess = Test-DataverseRecordAccess -Connection $connection -TableName account -Id $accountId -Principal $otherUser.systemuserid
Write-Host "Access after replace: $newAccess"

# Revoke all access
Remove-DataverseRecordAccess -Connection $connection -TableName account -Id $accountId -Principal $otherUser.systemuserid -Confirm:$false

# Verify access was removed
$finalAccess = Test-DataverseRecordAccess -Connection $connection -TableName account -Id $accountId -Principal $otherUser.systemuserid
Write-Host "Access after removal: $finalAccess"

# Clean up
Remove-DataverseRecord -Connection $connection -TableName account -Id $accountId -Confirm:$false
```

## Common Use Cases

### Audit Who Has Access to Sensitive Records

```powershell
# Find all users with access to high-value opportunities
$highValueOpps = Get-DataverseRecord -Connection $connection -TableName opportunity -FilterValues @{estimatedvalue_gt=1000000}

foreach ($opp in $highValueOpps) {
    Write-Host "`nOpportunity: $($opp.name) ($($opp.estimatedvalue))"
    $accessList = Get-DataverseRecordAccess -Connection $connection -TableName opportunity -Id $opp.opportunityid
    
    foreach ($access in $accessList) {
        Write-Host "  $($access.Principal.LogicalName): $($access.Principal.Id) - $($access.AccessMask)"
    }
}
```

### Bulk Share Records with a Team

```powershell
# Share all active accounts in a region with a sales team
$accounts = Get-DataverseRecord -Connection $connection -TableName account -FilterValues @{statecode=0; address1_stateorprovince="California"}
$salesTeamId = "12345678-1234-1234-1234-123456789012"

foreach ($account in $accounts) {
    Set-DataverseRecordAccess -Connection $connection -TableName account -Id $account.accountid -Principal $salesTeamId -AccessRights ReadAccess -IsTeam -Confirm:$false
}
```

### Implement Custom Security Logic

```powershell
# Grant write access only if user has read access and record is in draft state
function Grant-ConditionalWriteAccess {
    param($Connection, $TableName, $RecordId, $UserId)
    
    # Check current access
    $currentAccess = Test-DataverseRecordAccess -Connection $Connection -TableName $TableName -Id $RecordId -Principal $UserId
    $hasRead = ($currentAccess -band [Microsoft.Crm.Sdk.Messages.AccessRights]::ReadAccess) -ne 0
    
    if ($hasRead) {
        # User has read access, add write
        Set-DataverseRecordAccess -Connection $Connection -TableName $TableName -Id $RecordId -Principal $UserId -AccessRights WriteAccess -Confirm:$false
        Write-Host "Granted write access to user $UserId"
    } else {
        Write-Host "User $UserId does not have read access - cannot grant write access"
    }
}
```

## Best Practices

1. **Always check existing access before modifying**: Use `Test-DataverseRecordAccess` to understand current permissions before making changes.

2. **Use Replace mode carefully**: The `-Replace` switch removes all existing permissions and sets only the specified ones. This can accidentally remove important access rights.

3. **Consider using teams**: Instead of granting access to individual users, consider creating teams and granting access to teams for easier management.

4. **Implement proper error handling**: Access operations can fail due to security restrictions, record ownership, or invalid principal IDs.

5. **Audit access changes**: Keep logs of access changes for security and compliance purposes.

6. **Use WhatIf for testing**: Before bulk operations, use `-WhatIf` to preview changes without executing them.

7. **Be aware of inherited access**: These cmdlets only manage explicit shared access. Users may still have access through roles, ownership, or team membership.

## Related Documentation

- [Get-DataverseRecord](../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) - Query records
- [Get-DataverseWhoAmI](../getting-started/authentication.md) - Get current user information
- [Creating and Updating Records](creating-updating.md) - Create records to share
- [Microsoft Learn: Security concepts in Dataverse](https://learn.microsoft.com/power-platform/admin/wp-security-cds)

## API Reference

For complete parameter documentation and examples:
- [Test-DataverseRecordAccess](../../Rnwood.Dataverse.Data.PowerShell/docs/Test-DataverseRecordAccess.md)
- [Set-DataverseRecordAccess](../../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecordAccess.md)
- [Get-DataverseRecordAccess](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecordAccess.md)
- [Remove-DataverseRecordAccess](../../Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseRecordAccess.md)
