---
title: "Remove-DataverseRecordAccess - Remove access from multiple principals"
tags: ['Data']
source: "Remove-DataverseRecordAccess.md"
---
Removes shared access from all principals except the current user.

```powershell
$accessList = Get-DataverseRecordAccess -TableName opportunity -Id "33333333-3333-3333-3333-333333333333"
$whoAmI = Get-DataverseWhoAmI
$accessList | Where-Object { $_.Principal.Id -ne $whoAmI.UserId } | ForEach-Object {
# >>     Remove-DataverseRecordAccess -TableName opportunity -Id "33333333-3333-3333-3333-333333333333" -Principal $_.Principal.Id -Confirm:$false
# >> }

```
