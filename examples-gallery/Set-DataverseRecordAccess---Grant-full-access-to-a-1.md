---
title: "Set-DataverseRecordAccess - Grant full access to a team"
tags: ['Data']
source: "Set-DataverseRecordAccess.md"
---
Grants full access to a team for the account record.

```powershell
$fullAccess = [Microsoft.Crm.Sdk.Messages.AccessRights]::ReadAccess -bor [Microsoft.Crm.Sdk.Messages.AccessRights]::WriteAccess -bor [Microsoft.Crm.Sdk.Messages.AccessRights]::DeleteAccess -bor [Microsoft.Crm.Sdk.Messages.AccessRights]::ShareAccess -bor [Microsoft.Crm.Sdk.Messages.AccessRights]::AssignAccess
Set-DataverseRecordAccess -TableName account -Id "55555555-5555-5555-5555-555555555555" -Principal "66666666-6666-6666-6666-666666666666" -AccessRights $fullAccess -IsTeam

```
