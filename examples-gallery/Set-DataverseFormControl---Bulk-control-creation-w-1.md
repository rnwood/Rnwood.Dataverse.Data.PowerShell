---
title: "Set-DataverseFormControl - Bulk control creation with error handling"
tags: ['Metadata']
source: "Set-DataverseFormControl.md"
---
Creates multiple controls with error handling and conditional parameters.

```powershell
$controls = @(
    @{ DataField = 'firstname'; Label = 'First Name'; ColSpan = 1 }
    @{ DataField = 'lastname'; Label = 'Last Name'; ColSpan = 1; IsRequired = $true }
    @{ DataField = 'emailaddress1'; Label = 'Email'; ColSpan = 2; ControlType = 'Standard' }
)

foreach ($ctrl in $controls) {
    try {
        $params = @{
            FormId = $formId
            SectionName = 'ContactDetails'
            DataField = $ctrl.DataField
            Label = $ctrl.Label
            ColSpan = $ctrl.ColSpan
            ShowLabel = $true
            Visible = $true
            PassThru = $true
        }
        if ($ctrl.IsRequired) { $params.IsRequired = $true }
        if ($ctrl.ControlType) { $params.ControlType = $ctrl.ControlType }
        
        $controlId = Set-DataverseFormControl @params
        Write-Host "Created control: $($ctrl.DataField) with ID: $controlId"
    }
    catch {
        Write-Warning "Failed to create control $($ctrl.DataField): $($_.Exception.Message)"
    }
}

```
