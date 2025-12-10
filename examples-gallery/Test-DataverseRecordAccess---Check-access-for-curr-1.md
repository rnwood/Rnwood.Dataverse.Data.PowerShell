---
title: "Test-DataverseRecordAccess - Check access for current user"
tags: ['Data']
source: "Test-DataverseRecordAccess.md"
---
Checks what access the current authenticated user has to an opportunity record.

```powershell
$whoAmI = Get-DataverseWhoAmI
$access = Test-DataverseRecordAccess -TableName opportunity -Id "33333333-3333-3333-3333-333333333333" -Principal $whoAmI.UserId
$access
# ReadAccess, WriteAccess, AppendAccess, AppendToAccess, DeleteAccess, ShareAccess, AssignAccess

```
