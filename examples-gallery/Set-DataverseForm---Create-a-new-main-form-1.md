---
title: "Set-DataverseForm - Create a new main form"
tags: ['Metadata']
source: "Set-DataverseForm.md"
---
Creates a new main form for the contact entity with minimal configuration.

```powershell
$formId = Set-DataverseForm -Entity 'contact' -Name 'Custom Contact Form' -FormType 'Main' -PassThru
Write-Host "Created form with ID: $formId"

```

