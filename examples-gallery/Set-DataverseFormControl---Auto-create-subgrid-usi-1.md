---
title: "Set-DataverseFormControl - Auto-create subgrid using relationship name"
tags: ['Metadata']
source: "Set-DataverseFormControl.md"
---
Creates a subgrid control by specifying a relationship name in DataField. When DataField contains a one-to-many or many-to-many relationship name, the cmdlet automatically determines the control type as Subgrid. No need to explicitly specify ControlType.

```powershell
Set-DataverseFormControl -FormId $formId -SectionName 'RelatedData' `
   -TabName 'General' -DataField 'contact_customer_accounts' `
   -Labels @{1033 = 'Related Accounts'} -ColSpan 2 -PassThru

```

