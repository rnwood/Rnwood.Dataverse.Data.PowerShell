---
title: "Get-DataverseForm - Work with form structure using specialized cmdlets"
tags: ['Metadata']
source: "Get-DataverseForm.md"
---
Demonstrates how to explore form structure using the specialized form cmdlets.

```powershell
$form = Get-DataverseForm -Entity 'contact' -Name 'Information'
# Get all tabs from the form
$tabs = Get-DataverseFormTab -FormId $form.FormId
# Get all sections from a specific tab
$sections = Get-DataverseFormSection -FormId $form.FormId -TabName 'General'
# Get all controls from a specific section
$controls = Get-DataverseFormControl -FormId $form.FormId -TabName 'General' -SectionName 'Details'

```

