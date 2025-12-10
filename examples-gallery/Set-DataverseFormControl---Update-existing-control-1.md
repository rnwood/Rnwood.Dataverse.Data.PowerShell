---
title: "Set-DataverseFormControl - Update existing control to make it required"
tags: ['Metadata']
source: "Set-DataverseFormControl.md"
---
Updates an existing control to make it required. The cmdlet now supports updating existing controls without recreating them.

```powershell
Set-DataverseFormControl -FormId $formId `
   -TabName 'General' -SectionName 'ContactInfo' `
   -DataField 'telephone1' -IsRequired -ShowLabel

```

