---
title: "Get-DataverseFormSection - Explore section properties"
tags: ['Metadata']
source: "Get-DataverseFormSection.md"
---
Explores the structure and properties of a specific section.

```powershell
$section = Get-DataverseFormSection -FormId $formId -TabName 'General' -SectionName 'Summary'
$section.ColumnIndex    # Which column in the tab this section belongs to
$section.Columns        # Number of columns in the section
$section.Controls       # Controls contained in this section
$section.Labels         # Localized labels for the section

```

