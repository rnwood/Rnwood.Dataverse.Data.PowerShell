---
title: "Set-DataverseFormTab - Update tab visibility and layout"
tags: ['Metadata']
source: "Set-DataverseFormTab.md"
---
Updates an existing tab to make it visible and change to a three-column layout.

```powershell
$tab = Get-DataverseFormTab -FormId $formId -TabName "details"
Set-DataverseFormTab -FormId $formId -TabId $tab.Id -Name "details" `
   -Hidden:$false -Layout ThreeColumns -Column1Width 33 -Column2Width 33 -Column3Width 34

```

