---
title: "Test-DataverseRecordAccess - Check if a user has write access"
tags: ['Data']
source: "Test-DataverseRecordAccess.md"
---
Checks if a user has write access to a contact record.

```powershell
$userId = "22222222-2222-2222-2222-222222222222"
$access = Test-DataverseRecordAccess -TableName contact -Id "11111111-1111-1111-1111-111111111111" -Principal $userId
if ($access -band [Microsoft.Crm.Sdk.Messages.AccessRights]::WriteAccess) {
# >>     Write-Host "User has write access"
# >> } else {
# >>     Write-Host "User does not have write access"
# >> }

```
