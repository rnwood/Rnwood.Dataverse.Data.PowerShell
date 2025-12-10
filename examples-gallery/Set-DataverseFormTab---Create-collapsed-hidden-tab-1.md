---
title: "Set-DataverseFormTab - Create collapsed hidden tab"
tags: ['Metadata']
source: "Set-DataverseFormTab.md"
---
Creates a new tab that is hidden and collapsed by default but shows the label.

```powershell
Set-DataverseFormTab -FormId $formId -Name "Advanced" -Label "Advanced Options" `
   -Hidden -Expanded:$false -ShowLabel

```

