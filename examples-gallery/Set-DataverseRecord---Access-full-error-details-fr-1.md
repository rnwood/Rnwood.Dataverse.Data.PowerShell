---
title: "Set-DataverseRecord - Access full error details from server"
tags: ['Data']
source: "Set-DataverseRecord.md"
---
Demonstrates how to access comprehensive error details from the Dataverse server. The `Exception.Message` property contains the full server response formatted by the cmdlet, including the OrganizationServiceFault error code, message, server-side trace text, and any nested inner fault details. The `TargetObject` identifies which input record caused the error, while the full error message provides all diagnostic information needed for troubleshooting server-side issues.

```powershell
$records = @(
    @{ firstname = "Test"; lastname = "User" }
)

$errors = @()
$records | Set-DataverseRecord -TableName contact -CreateOnly -ErrorVariable +errors -ErrorAction SilentlyContinue

# Access detailed error information from the server
foreach ($err in $errors) {
    # TargetObject contains the input record that failed
    Write-Host "Failed record: $($err.TargetObject.firstname) $($err.TargetObject.lastname)"
    
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

