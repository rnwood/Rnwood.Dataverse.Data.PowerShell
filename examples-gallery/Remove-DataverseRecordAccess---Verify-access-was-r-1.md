---
title: "Remove-DataverseRecordAccess - Verify access was removed"
tags: ['Data']
source: "Remove-DataverseRecordAccess.md"
---
Removes access and verifies it was removed successfully.

```powershell
Remove-DataverseRecordAccess -TableName contact -Id "11111111-1111-1111-1111-111111111111" -Principal "44444444-4444-4444-4444-444444444444" -Confirm:$false
$access = Test-DataverseRecordAccess -TableName contact -Id "11111111-1111-1111-1111-111111111111" -Principal "44444444-4444-4444-4444-444444444444"
Write-Host "Access after removal: $access"
# Access after removal: None

```
