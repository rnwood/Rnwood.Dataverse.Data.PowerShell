---
title: "Remove-DataverseFormTab - Remove multiple tabs safely"
tags: ['Metadata']
source: "Remove-DataverseFormTab.md"
---
Safely removes multiple tabs by first previewing with -WhatIf, then executing the actual removal.

```powershell
$tabsToRemove = @('TempTab1', 'TempTab2', 'TempTab3')
foreach ($tabName in $tabsToRemove) {
    try {
        Remove-DataverseFormTab -FormId $formId -TabName $tabName -WhatIf
        Write-Host "Would remove tab: $tabName"
    }
    catch {
        Write-Warning "Cannot remove tab $tabName: $($_.Exception.Message)"
    }
}

# # After reviewing, execute without -WhatIf
foreach ($tabName in $tabsToRemove) {
    try {
        Remove-DataverseFormTab -FormId $formId -TabName $tabName
        Write-Host "Removed tab: $tabName"
    }
    catch {
        Write-Warning "Failed to remove tab $tabName: $($_.Exception.Message)"
    }
}

```

