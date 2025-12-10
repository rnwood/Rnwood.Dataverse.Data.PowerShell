---
title: "Get-DataverseOptionSetMetadata - Find multi-select choice options"
tags: ['Metadata']
source: "Get-DataverseOptionSetMetadata.md"
---
Retrieves options for a multi-select choice field.

```powershell
$attr = Get-DataverseAttributeMetadata -EntityName contact -AttributeName new_interests
if ($attr.AttributeType -eq 'MultiSelectPicklist') {
    $optionSet = Get-DataverseOptionSetMetadata -EntityName contact -AttributeName new_interests
    $optionSet.Options | Select-Object Value, Label
}

# Value Label
# ----- -----
# 1     Technology
# 2     Sports
# 3     Music
# 4     Travel

```
