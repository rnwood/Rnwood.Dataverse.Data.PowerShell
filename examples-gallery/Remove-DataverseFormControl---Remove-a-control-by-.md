---
title: "Remove-DataverseFormControl - Remove a control by ID"
tags: ['Metadata']
source: "Remove-DataverseFormControl.md"
---
Removes the control with the specified ID from the form.

```powershell
$form = Get-DataverseForm -Entity 'contact' -Name 'Information'
Remove-DataverseFormControl -FormId $form.Id -ControlId 'firstname_control_id'

```

