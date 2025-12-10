---
title: "Remove-DataverseView - Remove multiple views via pipeline"
tags: ['Metadata']
source: "Remove-DataverseView.md"
---
Finds all views whose names start with "Test" and removes them without confirmation prompts. The ViewType is automatically inferred from the pipeline input.

```powershell
Get-DataverseView -Name "Test*" |
    Remove-DataverseView -Confirm:$false

```

