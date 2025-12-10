---
title: "Remove-DataverseSolutionComponent - Remove an attribute component"
tags: ['Solutions']
source: "Remove-DataverseSolutionComponent.md"
---
Removes an attribute component from the solution without confirmation prompt.

```powershell
$attributeMetadata = Get-DataverseAttributeMetadata `
   -EntityName "account" -AttributeName "new_customfield"
Remove-DataverseSolutionComponent -SolutionName "MySolution" `
   -ComponentId $attributeMetadata.MetadataId -ComponentType 2 -Confirm:$false

```

