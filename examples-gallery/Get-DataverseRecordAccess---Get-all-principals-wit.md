---
title: "Get-DataverseRecordAccess - Get all principals with access to a record"
tags: ['Data']
source: "Get-DataverseRecordAccess.md"
---
Retrieves all principals who have shared access to the account record.

```powershell
$accessList = Get-DataverseRecordAccess -TableName account -Id "12345678-1234-1234-1234-123456789012"
$accessList | Format-Table @{Label="Principal ID"; Expression={$_.Principal.Id}}, @{Label="Principal Type"; Expression={$_.Principal.LogicalName}}, AccessMask

# Principal ID                         Principal Type AccessMask
# ------------                        -------------- ----------
# 87654321-4321-4321-4321-210987654321 systemuser     ReadAccess, WriteAccess
# 22222222-2222-2222-2222-222222222222 team           ReadAccess, WriteAccess, DeleteAccess

```
