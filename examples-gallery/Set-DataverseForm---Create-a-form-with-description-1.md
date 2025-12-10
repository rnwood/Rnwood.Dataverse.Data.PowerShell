---
title: "Set-DataverseForm - Create a form with description and set as active"
tags: ['Metadata']
source: "Set-DataverseForm.md"
---
Creates a new quick create form with a description and marks it as active.

```powershell
Set-DataverseForm -Entity 'account' -Name 'Account Quick Create' -FormType 'QuickCreate' -Description 'Quick create form for accounts' -IsActive -PassThru

```

