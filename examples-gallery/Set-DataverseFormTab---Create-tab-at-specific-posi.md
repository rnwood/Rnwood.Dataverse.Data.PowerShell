---
title: "Set-DataverseFormTab - Create tab at specific position"
tags: ['Metadata']
source: "Set-DataverseFormTab.md"
---
Creates a new tab at the beginning (first position) of the tab list.

```powershell
Set-DataverseFormTab -FormId $formId -Name "FirstTab" -Label "First Tab" `
   -Index 0 -Layout OneColumn

```

