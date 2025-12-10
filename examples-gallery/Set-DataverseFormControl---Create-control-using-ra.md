---
title: "Set-DataverseFormControl - Create control using raw XML for advanced configuration"
tags: ['Metadata']
source: "Set-DataverseFormControl.md"
---
Creates a datetime control using raw XML for full control over configuration.

```powershell
$controlXml = @"
# <control id="custom_datetime" classid="{5B773807-9FB2-42DB-97C3-7A91EFF8ADFF}" 
         datafieldname="custom_datetime" disabled="false" visible="true">
  <labels>
    <label description="Custom Date/Time" languagecode="1033" />
  </labels>
  <parameters>
    <DateAndTimeStyle>DateAndTime</DateAndTimeStyle>
    <HideTimeForDate>false</HideTimeForDate>
  </parameters>
# </control>
# "@
Set-DataverseFormControl -FormId $formId -SectionName 'CustomSection' `
   -ControlXml $controlXml -PassThru

```

