---
title: "Set-DataverseRecordAccess - Use with pipeline"
tags: ['Data']
source: "Set-DataverseRecordAccess.md"
---
Grants read access to a user for all active contact records.

```powershell
$records = Get-DataverseRecord -TableName contact -FilterValues @{statecode=0} -Columns contactid
$userId = "77777777-7777-7777-7777-777777777777"
$records | ForEach-Object {
# >>     Set-DataverseRecordAccess -TableName contact -Id $_.contactid -Principal $userId -AccessRights ReadAccess
# >> }

```
