---
title: "Get-DataverseAttributeMetadata - Get choice field options"
tags: ['Metadata']
source: "Get-DataverseAttributeMetadata.md"
---
Retrieves the options for a choice (picklist) field.

```powershell
$attr = Get-DataverseAttributeMetadata -EntityName contact -AttributeName gendercode
$attr.OptionSet.Options | Select-Object Value, Label

# Value Label
# ----- -----
# 1     Male
# 2     Female

```
