---
title: "Set-DataverseOptionSetMetadata - Update an existing option set"
tags: ['Metadata']
source: "Set-DataverseOptionSetMetadata.md"
---
Updates an existing option set by changing the label of option 1, keeping option 2 unchanged, and adding a new option 4. Option 3 (if it existed) would be removed unless -NoRemoveMissingOptions is specified.

```powershell
$options = @(
    @{ Value = 1; Label = "Updated Option 1" }
    @{ Value = 2; Label = "Option 2" }
    @{ Value = 4; Label = "New Option 4" }
)

Set-DataverseOptionSetMetadata -Name "existing_optionset" -Options $options

```

