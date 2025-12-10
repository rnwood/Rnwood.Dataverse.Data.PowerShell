---
title: "Remove-DataverseView - Remove views in a safe workflow"
tags: ['Metadata']
source: "Remove-DataverseView.md"
---
Demonstrates a safe workflow: first preview with WhatIf, then delete with confirmation prompts.

```powershell
# Get test views
$testViews = Get-DataverseView -Name "DEV_*"

# Preview what will be deleted
$testViews | Remove-DataverseView -WhatIf

# Confirm and delete
$testViews | Remove-DataverseView 

```

