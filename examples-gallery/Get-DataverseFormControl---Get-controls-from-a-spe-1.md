---
title: "Get-DataverseFormControl - Get controls from a specific section (requires TabName)"
tags: ['Metadata']
source: "Get-DataverseFormControl.md"
---
Retrieves all controls from the 'ContactDetails' section in the 'General' tab. Note: TabName is required when SectionName is specified since section names are only unique within a tab.

```powershell
$formId = '12345678-1234-1234-1234-123456789012'
Get-DataverseFormControl -FormId $formId -TabName 'General' -SectionName 'ContactDetails'

```

