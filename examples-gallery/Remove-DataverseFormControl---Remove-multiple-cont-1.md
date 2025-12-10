---
title: "Remove-DataverseFormControl - Remove multiple controls safely"
tags: ['Metadata']
source: "Remove-DataverseFormControl.md"
---
Safely removes multiple controls by first previewing with -WhatIf, then executing the actual removal.

```powershell
$controlsToRemove = @('fax', 'pager', 'telex')
foreach ($fieldName in $controlsToRemove) {
    try {
        Remove-DataverseFormControl -FormId $formId -TabName 'General' -SectionName 'ContactMethods' -DataField $fieldName -WhatIf
        Write-Host "Would remove control: $fieldName"
    }
    catch {
        Write-Warning "Cannot remove control $fieldName: $($_.Exception.Message)"
    }
}

# # After reviewing, execute without -WhatIf
foreach ($fieldName in $controlsToRemove) {
    try {
        Remove-DataverseFormControl -FormId $formId -TabName 'General' -SectionName 'ContactMethods' -DataField $fieldName
        Write-Host "Removed control: $fieldName"
    }
    catch {
        Write-Warning "Failed to remove control $fieldName: $($_.Exception.Message)"
    }
}

```

