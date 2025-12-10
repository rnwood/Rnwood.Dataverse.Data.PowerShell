---
title: "Remove-DataverseFormControl - Remove a control by data field name"
tags: ['Metadata']
source: "Remove-DataverseFormControl.md"
---
Removes the control bound to the 'middlename' field from the ContactInfo section in the General tab.

```powershell
Remove-DataverseFormControl -FormId $formId -TabName 'General' -SectionName 'ContactInfo' -DataField 'middlename'

```

