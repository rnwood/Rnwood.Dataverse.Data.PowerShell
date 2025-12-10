---
title: "Set-DataverseRecordAccess - Grant read and write access to a user (additive)"
tags: ['Data']
source: "Set-DataverseRecordAccess.md"
---
Grants read and write access to the specified user for the account record. If the user already has other rights, they will be retained.

```powershell
$accessRights = [Microsoft.Crm.Sdk.Messages.AccessRights]::ReadAccess -bor [Microsoft.Crm.Sdk.Messages.AccessRights]::WriteAccess
Set-DataverseRecordAccess -TableName account -Id "12345678-1234-1234-1234-123456789012" -Principal "87654321-4321-4321-4321-210987654321" -AccessRights $accessRights

```
