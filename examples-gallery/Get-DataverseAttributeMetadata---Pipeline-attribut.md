---
title: "Get-DataverseAttributeMetadata - Pipeline attribute name"
tags: ['Metadata']
source: "Get-DataverseAttributeMetadata.md"
---
Retrieves metadata for multiple attributes using pipeline.

```powershell
@("firstname", "lastname", "emailaddress1") | ForEach-Object {
    Get-DataverseAttributeMetadata -EntityName contact -AttributeName $_
} | Select-Object LogicalName, AttributeType, MaxLength

# LogicalName     AttributeType MaxLength
# -----------    ------------- ---------
# firstname       String        50
# lastname        String        50
# emailaddress1   String        100

```
