---
title: "Set-DataverseOptionSetMetadata - Create a new global option set"
tags: ['Metadata']
source: "Set-DataverseOptionSetMetadata.md"
---
Creates a new global option set with three options.

```powershell
$options = @(
    @{ Value = 1; Label = "Option 1" }
    @{ Value = 2; Label = "Option 2"; Color = "#FF0000" }
    @{ Value = 3; Label = "Option 3"; Description = "Third option" }
)

Set-DataverseOptionSetMetadata -Name "new_optionset" -DisplayName "New Option Set" -Options $options

```

