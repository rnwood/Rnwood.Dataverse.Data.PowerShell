---
title: "Get-DataverseAppModuleComponent - Get components for a specific entity"
tags: ['Metadata']
source: "Get-DataverseAppModuleComponent.md"
---
Finds which app modules include a specific entity.

---

```powershell
$entityMetadata = Get-DataverseEntityMetadata -EntityName "contact"
Get-DataverseAppModuleComponent -ObjectId $entityMetadata.MetadataId

```
