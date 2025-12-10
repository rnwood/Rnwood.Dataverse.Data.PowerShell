---
title: "Set-DataverseForm - Update an existing form properties"
tags: ['Metadata']
source: "Set-DataverseForm.md"
---
Updates the name, description, and sets the form as default for its entity.

```powershell
$formId = 'a1234567-89ab-cdef-0123-456789abcdef'
Set-DataverseForm -Id $formId -Name 'Updated Form Name' -Description 'Updated description' -IsDefault

```

