---
title: "Set-DataverseFormTab - Create a three-column tab"
tags: ['Metadata']
source: "Set-DataverseFormTab.md"
---
Creates a tab with three columns with custom widths.

```powershell
Set-DataverseFormTab -FormId $formId -Name "Overview" -Label "Overview" `
   -Layout ThreeColumns -Column1Width 40 -Column2Width 30 -Column3Width 30

```

