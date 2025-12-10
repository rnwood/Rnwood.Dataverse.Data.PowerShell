---
title: "Get-DataverseFormSection - Find sections with specific properties"
tags: ['Metadata']
source: "Get-DataverseFormSection.md"
---
Filters sections based on specific properties.

```powershell
$sections = Get-DataverseFormSection -FormId $formId
$sections | Where-Object { $_.Hidden -eq $true }                    # Hidden sections
$sections | Where-Object { $_.Columns -gt 1 }                      # Multi-column sections
$sections | Where-Object { $_.ShowLabel -eq $false }               # Sections without labels

```

