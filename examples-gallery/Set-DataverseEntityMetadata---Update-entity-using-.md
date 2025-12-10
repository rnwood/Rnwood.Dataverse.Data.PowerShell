---
title: "Set-DataverseEntityMetadata - Update entity using EntityMetadata object"
tags: ['Metadata']
source: "Set-DataverseEntityMetadata.md"
---
Retrieves an EntityMetadata object, modifies properties, and updates the entity using the modified object. This is useful for complex updates or bulk modifications.

```powershell
# Get existing metadata
$metadata = Get-DataverseEntityMetadata -EntityName account

# Modify properties
$metadata.IconVectorName = "svg_updated_account"
$metadata.ChangeTrackingEnabled = $true

# Update the entity with modified metadata
Set-DataverseEntityMetadata -EntityMetadata $metadata

```
