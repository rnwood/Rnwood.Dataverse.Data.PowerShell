---
title: "Set-DataverseEntityMetadata - Use -WhatIf to preview changes"
tags: ['Metadata']
source: "Set-DataverseEntityMetadata.md"
---
Uses `-WhatIf` to see what would happen without actually creating the entity.

```powershell
Set-DataverseEntityMetadata -EntityName new_test `
   -SchemaName new_Test `
   -DisplayName "Test Entity" `
   -DisplayCollectionName "Test Entities" `
   -OwnershipType UserOwned `
   -PrimaryAttributeSchemaName new_name `
   -PrimaryAttributeDisplayName "Name" `
   -WhatIf

# What if: Performing the operation "Create entity 'new_Test'" on target "Dataverse organization".

```
