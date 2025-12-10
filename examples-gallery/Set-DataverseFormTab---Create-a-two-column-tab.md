---
title: "Set-DataverseFormTab - Create a two-column tab"
tags: ['Metadata']
source: "Set-DataverseFormTab.md"
---
Creates a tab with two columns: 60% and 40% width.

```powershell
Set-DataverseFormTab -FormId $formId -Name "ContactInfo" -Label "Contact Information" `
   -Layout TwoColumns -Column1Width 60 -Column2Width 40

```

