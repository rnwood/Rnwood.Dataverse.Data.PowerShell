---
title: "Set-DataverseFormControl - Add a simple text field control"
tags: ['Metadata']
source: "Set-DataverseFormControl.md"
---
Adds a text field control for the firstname attribute with a custom label.

```powershell
$form = Get-DataverseForm -Entity 'contact' -Name 'Information'
Set-DataverseFormControl -FormId $form.Id -SectionName 'GeneralSection' `
   -DataField 'firstname' -Labels @{1033 = 'First Name'} -ShowLabel -Visible -PassThru

```

