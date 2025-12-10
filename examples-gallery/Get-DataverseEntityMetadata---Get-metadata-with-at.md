---
title: "Get-DataverseEntityMetadata - Get metadata with attributes excluded"
tags: ['Metadata']
source: "Get-DataverseEntityMetadata.md"
---
Gets entity metadata without attributes, then shows the default behavior includes them.

```powershell
$metadata = Get-DataverseEntityMetadata -EntityName account -ExcludeAttributes
$metadata.Attributes
# # Attributes will be null or minimal when excluded

# By default, attributes are included
$metadata = Get-DataverseEntityMetadata -EntityName account
$metadata.Attributes.Count
# 150

```
