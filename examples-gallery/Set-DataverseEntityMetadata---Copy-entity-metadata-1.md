---
title: "Set-DataverseEntityMetadata - Copy entity metadata properties to another entity"
tags: ['Metadata']
source: "Set-DataverseEntityMetadata.md"
---
Copies icon properties from one entity to another entity.

```powershell
# Get source entity metadata
$sourceMetadata = Get-DataverseEntityMetadata -EntityName account

# Copy icon properties to target entity
Set-DataverseEntityMetadata -EntityName new_customer `
   -IconVectorName $sourceMetadata.IconVectorName `
   -IconLargeName $sourceMetadata.IconLargeName `
   -IconMediumName $sourceMetadata.IconMediumName `
   -IconSmallName $sourceMetadata.IconSmallName

```
