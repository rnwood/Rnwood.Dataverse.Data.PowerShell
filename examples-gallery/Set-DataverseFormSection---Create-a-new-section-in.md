---
title: "Set-DataverseFormSection - Create a new section in a tab"
tags: ['Metadata']
source: "Set-DataverseFormSection.md"
---
Creates a new section named 'CustomSection' with 2 columns in the General tab.

```powershell
$form = Get-DataverseForm -Entity 'contact' -Name 'Information'
Set-DataverseFormSection -FormId $form.FormId -TabName 'General' `
   -Name 'CustomSection' -Label 'Custom Information' -Columns 2 -PassThru

```

