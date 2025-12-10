---
title: "Set-DataverseFormControl - Insert control at specific position"
tags: ['Metadata']
source: "Set-DataverseFormControl.md"
---
Inserts an email control after the 'lastname' control.

```powershell
Set-DataverseFormControl -FormId $formId -SectionName 'GeneralSection' `
   -DataField 'emailaddress1' -Labels @{1033 = 'Primary Email'} `
   -InsertAfter 'lastname' -ColSpan 2 -PassThru

```

