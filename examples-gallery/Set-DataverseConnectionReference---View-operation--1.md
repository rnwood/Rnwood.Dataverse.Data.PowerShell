---
title: "Set-DataverseConnectionReference - View operation results"
tags: ['Solutions']
source: "Set-DataverseConnectionReference.md"
---
Sets a connection reference and displays the operation type and connection ID changes.

```powershell
$result = Set-DataverseConnectionReference -ConnectionReferenceLogicalName "new_sharedconnectionref" -ConnectionId "12345678-1234-1234-1234-123456789abc"
Write-Host "Operation: $($result.Operation)"
if ($result.PreviousConnectionId) {
    Write-Host "Changed from $($result.PreviousConnectionId) to $($result.ConnectionId)"
}

```
