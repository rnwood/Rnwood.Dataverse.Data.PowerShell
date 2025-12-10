---
title: "Test-DataverseRecordAccess - Pipeline usage"
tags: ['Data']
source: "Test-DataverseRecordAccess.md"
---
Checks write access for a user across multiple contact records.

```powershell
$contacts = Get-DataverseRecord -TableName contact -FilterValues @{statecode=0} -Columns contactid
$userId = "44444444-4444-4444-4444-444444444444"
$contacts | ForEach-Object {
# >>     $access = Test-DataverseRecordAccess -TableName contact -Id $_.contactid -Principal $userId
# >>     [PSCustomObject]@{
# >>         ContactId = $_.contactid
# >>         HasWriteAccess = ($access -band [Microsoft.Crm.Sdk.Messages.AccessRights]::WriteAccess) -ne 0
# >>     }
# >> }

```
