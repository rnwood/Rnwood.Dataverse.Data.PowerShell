---
title: "Get-DataverseFormTab - Explore tab structure"
tags: ['Metadata']
source: "Get-DataverseFormTab.md"
---
Explores the structure and properties of a specific tab.

```powershell
$tab = Get-DataverseFormTab -FormId $formId -TabName 'Details'
$tab.Layout          # Shows OneColumn, TwoColumns, ThreeColumns, or Custom
$tab.Sections        # Array of sections in the tab
$tab.Labels          # Localized labels for the tab

```

