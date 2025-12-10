---
title: "Set-DataverseEntityMetadata - Pipeline EntityMetadata for batch updates"
tags: ['Metadata']
source: "Set-DataverseEntityMetadata.md"
---
Retrieves all custom entities, updates their icon properties, and publishes the changes using the pipeline.

```powershell
# Update icons for multiple custom entities
Get-DataverseEntityMetadata | 
    Where-Object { $_.IsCustomEntity -eq $true } |
    ForEach-Object {
        $_.IconVectorName = "svg_custom_icon"
        $_
    } |
    Set-DataverseEntityMetadata -Publish

```
