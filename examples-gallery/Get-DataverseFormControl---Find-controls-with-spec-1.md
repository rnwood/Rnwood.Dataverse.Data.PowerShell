---
title: "Get-DataverseFormControl - Find controls with specific properties"
tags: ['Metadata']
source: "Get-DataverseFormControl.md"
---
Filters controls based on specific properties.

```powershell
$controls = Get-DataverseFormControl -FormId $formId
$controls | Where-Object { $_.Hidden -eq $true }                    # Hidden controls
$controls | Where-Object { $_.IsRequired -eq $true }               # Required controls
$controls | Where-Object { $_.Disabled -eq $true }                 # Disabled controls
$controls | Where-Object { $_.ShowLabel -eq $false }               # Controls without labels

```

