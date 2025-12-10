---
title: "Remove-DataverseRecord - Access full error details from server"
tags: ['Data']
source: "Remove-DataverseRecord.md"
---
Demonstrates how to access comprehensive error details from the Dataverse server when deleting records. The `Exception.Message` property contains the full server response formatted by the cmdlet, including the OrganizationServiceFault error code, message, server-side trace text, and any nested inner fault details. The `TargetObject` identifies which input record caused the deletion error, while the full error message provides all diagnostic information needed for troubleshooting server-side issues.

```powershell
$recordsToDelete = Get-DataverseRecord -TableName contact -Top 10

$errors = @()
$recordsToDelete | Remove-DataverseRecord -ErrorVariable +errors -ErrorAction SilentlyContinue

# Access detailed error information from the server
foreach ($err in $errors) {
    # TargetObject contains the input record that failed to delete
    Write-Host "Failed to delete record: $($err.TargetObject.Id)"
    
    # Exception.Message contains full server response including:
    # - OrganizationServiceFault ErrorCode and Message
    # - TraceText with server-side trace details
    # - InnerFault details (if any)
    Write-Host "Full error details from server:"
    Write-Host $err.Exception.Message
    
    # You can also access individual components:
    Write-Host "Error category: $($err.CategoryInfo.Category)"
    Write-Host "Exception type: $($err.Exception.GetType().Name)"
}

```

