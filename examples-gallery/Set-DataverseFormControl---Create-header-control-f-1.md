---
title: "Set-DataverseFormControl - Create header control (form header)"
tags: ['Metadata']
source: "Set-DataverseFormControl.md"
---
Creates a read-only email control in the form header. Header controls are displayed at the top of the form across all tabs. The header section is automatically created if it doesn't exist.

```powershell
Set-DataverseFormControl -FormId $formId `
   -TabName '[Header]' -DataField 'emailaddress1' `
   -ControlType 'Email' -Labels @{1033 = 'Email'} -Disabled -PassThru

```

