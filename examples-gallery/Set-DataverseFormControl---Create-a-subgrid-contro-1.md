---
title: "Set-DataverseFormControl - Create a subgrid control without DataField"
tags: ['Metadata']
source: "Set-DataverseFormControl.md"
---
Creates a subgrid control showing related opportunities. Note: Subgrids don't require a DataField parameter since they're not bound to a single attribute.

```powershell
$parameters = @{
    'targetEntityType' = 'opportunity'
    'viewId' = '12345678-1234-1234-1234-123456789012'
    'RelationshipName' = 'contact_customer_opportunity'
}
Set-DataverseFormControl -FormId $formId -SectionName 'RelatedData' `
   -TabName 'General' -ControlType 'Subgrid' -ControlId 'opportunities_subgrid' `
   -Labels @{1033 = 'Related Opportunities'} -Parameters $parameters -ColSpan 2 -PassThru

```

