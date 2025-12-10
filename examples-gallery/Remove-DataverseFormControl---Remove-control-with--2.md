---
title: "Remove-DataverseFormControl - Remove control with immediate publishing"
tags: ['Metadata']
source: "Remove-DataverseFormControl.md"
---
Removes the custom field control and publishes the form to make changes visible immediately.

```powershell
Remove-DataverseFormControl -FormId $formId -TabName 'Details' -SectionName 'CustomSection' -DataField 'custom_field' -Publish

```

