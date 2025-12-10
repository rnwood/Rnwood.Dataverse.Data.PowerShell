---
title: "Remove-DataverseFormControl - Remove control and handle dependencies"
tags: ['Metadata']
source: "Remove-DataverseFormControl.md"
---
Checks for control existence and handles dependencies before removal.

```powershell
$controlToRemove = 'creditlimit'

# Check if control exists and get details
$control = Get-DataverseFormControl -FormId $formId -TabName 'Details' -SectionName 'Financial' -DataField $controlToRemove

if ($control) {
    Write-Host "Found control: $($control.DataField) - $($control.Label)"
    Write-Warning "Please verify no business rules or scripts reference this control before removal"
    
    $userChoice = Read-Host "Continue with removal? (y/N)"
    if ($userChoice -eq 'y' -or $userChoice -eq 'Y') {
        Remove-DataverseFormControl -FormId $formId -TabName 'Details' -SectionName 'Financial' -DataField $controlToRemove
        Write-Host "Control removed successfully"
    } else {
        Write-Host "Removal cancelled"
    }
} else {
    Write-Warning "Control not found: $controlToRemove"
}

```

