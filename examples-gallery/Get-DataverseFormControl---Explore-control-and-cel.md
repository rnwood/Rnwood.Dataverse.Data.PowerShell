---
title: "Get-DataverseFormControl - Explore control and cell properties"
tags: ['Metadata']
source: "Get-DataverseFormControl.md"
---
Explores the structure and properties of a specific control including cell-level attributes.

```powershell
$control = Get-DataverseFormControl -FormId $formId -DataField 'firstname'
$control.Id              # Control ID
$control.DataField       # Bound data field name
$control.ClassId         # Control class identifier
$control.Labels          # Localized labels
$control.Events          # Event handlers
$control.Parameters      # Custom parameters
$control.ColSpan         # Cell column span
$control.RowSpan         # Cell row span
$control.CellId          # Cell identifier
$control.Auto            # Cell auto-sizing
$control.LockLevel       # Cell lock level
$control.CellLabels      # Cell-level labels (if different from control)
$control.CellEvents      # Cell-level events

```

