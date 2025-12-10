---
title: "Get-DataverseFormControl - Get controls and their locations"
tags: ['Metadata']
source: "Get-DataverseFormControl.md"
---
Gets controls with their location information and sorts by hierarchy.

```powershell
Get-DataverseFormControl -FormId $formId | 
    Select-Object TabName, SectionName, Id, DataField | 
    Sort-Object TabName, SectionName, Id

```

