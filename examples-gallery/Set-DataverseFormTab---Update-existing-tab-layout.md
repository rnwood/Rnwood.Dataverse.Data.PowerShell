---
title: "Set-DataverseFormTab - Update existing tab layout"
tags: ['Metadata']
source: "Set-DataverseFormTab.md"
---
Updates an existing tab to use a two-column layout with equal widths.

```powershell
$tab = Get-DataverseFormTab -FormId $formId -TabName "general"
Set-DataverseFormTab -FormId $formId -TabId $tab.Id -Name "general" `
   -Layout TwoColumns -Column1Width 50 -Column2Width 50

```

