---
title: "Get-DataverseRecordAccess - Pipeline usage with multiple records"
tags: ['Data']
source: "Get-DataverseRecordAccess.md"
---
Gets shared access information for multiple opportunity records and groups by principal.

```powershell
$opportunities = Get-DataverseRecord -TableName opportunity -FilterValues @{statecode=0} -Columns opportunityid
$opportunities | ForEach-Object {
# >>     Get-DataverseRecordAccess -TableName opportunity -Id $_.opportunityid
# >> } | Group-Object {$_.Principal.Id} | Select-Object Count, Name

# Count Name
# ----- ----
    3 87654321-4321-4321-4321-210987654321
    2 22222222-2222-2222-2222-222222222222

```
