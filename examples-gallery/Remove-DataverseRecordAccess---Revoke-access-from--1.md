---
title: "Remove-DataverseRecordAccess - Revoke access from a team without confirmation"
tags: ['Data']
source: "Remove-DataverseRecordAccess.md"
---
Revokes all access from the specified team for the contact record without prompting for confirmation.

```powershell
Remove-DataverseRecordAccess -TableName contact -Id "11111111-1111-1111-1111-111111111111" -Principal "22222222-2222-2222-2222-222222222222" -IsTeam -Confirm:$false

```
