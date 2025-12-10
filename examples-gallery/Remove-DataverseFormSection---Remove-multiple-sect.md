---
title: "Remove-DataverseFormSection - Remove multiple sections safely"
tags: ['Metadata']
source: "Remove-DataverseFormSection.md"
---
Safely removes multiple sections by first previewing with -WhatIf, then executing the actual removal.

```powershell
$sectionsToRemove = @('TempSection1', 'TempSection2', 'TempSection3')
foreach ($sectionName in $sectionsToRemove) {
    try {
        Remove-DataverseFormSection -FormId $formId -TabName 'Advanced' -SectionName $sectionName -WhatIf
        Write-Host "Would remove section: $sectionName"
    }
    catch {
        Write-Warning "Cannot remove section $sectionName: $($_.Exception.Message)"
    }
}

# # After reviewing, execute without -WhatIf
foreach ($sectionName in $sectionsToRemove) {
    try {
        Remove-DataverseFormSection -FormId $formId -TabName 'Advanced' -SectionName $sectionName
        Write-Host "Removed section: $sectionName"
    }
    catch {
        Write-Warning "Failed to remove section $sectionName: $($_.Exception.Message)"
    }
}

```

