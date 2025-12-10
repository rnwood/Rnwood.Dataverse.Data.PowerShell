---
title: "Set-DataverseFormControl - Create a web resource control without DataField"
tags: ['Metadata']
source: "Set-DataverseFormControl.md"
---
Adds a web resource control displaying a custom HTML chart. Web resources don't require a DataField parameter.

```powershell
$webResourceParams = @{
    'src' = 'WebResources/custom_chart.html'
    'height' = '300'
    'scrolling' = 'no'
}
Set-DataverseFormControl -FormId $formId -SectionName 'Analytics' `
   -TabName 'General' -ControlType 'WebResource' -ControlId 'chart_webresource' `
   -Labels @{1033 = 'Sales Chart'} -Parameters $webResourceParams -ColSpan 2 -PassThru

```

