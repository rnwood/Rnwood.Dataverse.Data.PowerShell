---
title: "Get-DataverseRecordAccess - Find users with write access"
tags: ['Data']
source: "Get-DataverseRecordAccess.md"
---
Finds all principals who have write access to the contact record.

```powershell
$accessList = Get-DataverseRecordAccess -TableName contact -Id "11111111-1111-1111-1111-111111111111"
$writeAccess = $accessList | Where-Object { 
# >>     ($_.AccessMask -band [Microsoft.Crm.Sdk.Messages.AccessRights]::WriteAccess) -ne 0 
# >> }
$writeAccess | ForEach-Object { 
# >>     Write-Host "Principal $($_.Principal.Id) has write access"
# >> }

```
