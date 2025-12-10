---
title: "Set-DataverseRecordAccess - Add delete access to existing permissions"
tags: ['Data']
source: "Set-DataverseRecordAccess.md"
---
Adds delete access to the user's existing permissions. The user will now have read, write, and delete access.

```powershell
# User already has read/write access, now add delete
Set-DataverseRecordAccess -TableName contact -Id "11111111-1111-1111-1111-111111111111" -Principal "22222222-2222-2222-2222-222222222222" -AccessRights DeleteAccess

```
