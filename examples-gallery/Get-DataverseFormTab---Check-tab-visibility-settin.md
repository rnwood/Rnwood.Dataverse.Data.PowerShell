---
title: "Get-DataverseFormTab - Check tab visibility settings"
tags: ['Metadata']
source: "Get-DataverseFormTab.md"
---
Finds all hidden tabs in the form.

```powershell
$tabs = Get-DataverseFormTab -FormId $formId
$tabs | Where-Object { $_.Hidden -eq $true }

```

