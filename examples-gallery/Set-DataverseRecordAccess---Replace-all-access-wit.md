---
title: "Set-DataverseRecordAccess - Replace all access with only read access"
tags: ['Data']
source: "Set-DataverseRecordAccess.md"
---
Replaces all existing access rights with only read access. Any write or delete permissions are removed.

```powershell
# User has read, write, delete access - replace with only read
Set-DataverseRecordAccess -TableName opportunity -Id "33333333-3333-3333-3333-333333333333" -Principal "44444444-4444-4444-4444-444444444444" -AccessRights ReadAccess -Replace

```
