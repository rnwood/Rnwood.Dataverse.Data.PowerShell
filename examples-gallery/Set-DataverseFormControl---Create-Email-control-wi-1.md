---
title: "Set-DataverseFormControl - Create Email control with validation"
tags: ['Metadata']
source: "Set-DataverseFormControl.md"
---
Creates an email control with built-in email validation.

```powershell
Set-DataverseFormControl -FormId $formId `
   -TabName 'General' -SectionName 'ContactInfo' `
   -DataField 'emailaddress1' -ControlType 'Email' `
   -Labels @{1033 = 'Primary Email'} -IsRequired -ColSpan 2 -PassThru

```

