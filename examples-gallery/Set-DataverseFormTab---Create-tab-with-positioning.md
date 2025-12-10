---
title: "Set-DataverseFormTab - Create tab with positioning"
tags: ['Metadata']
source: "Set-DataverseFormTab.md"
---
Creates a new tab and positions it before the "general" tab, returning the new tab ID.

```powershell
Set-DataverseFormTab -FormId $formId -Name "Summary" -Label "Summary" `
   -InsertBefore "general" -Layout OneColumn -PassThru

```

