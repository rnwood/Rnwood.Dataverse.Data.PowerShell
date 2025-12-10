---
title: "Test-DataverseRecordAccess - Check user access to a specific account record"
tags: ['Data']
source: "Test-DataverseRecordAccess.md"
---
Tests what access rights the specified user has for the account record.

```powershell
$userId = "87654321-4321-4321-4321-210987654321"
$access = Test-DataverseRecordAccess -TableName account -Id "12345678-1234-1234-1234-123456789012" -Principal $userId
$access
# ReadAccess, WriteAccess, DeleteAccess, ShareAccess, AssignAccess

```
