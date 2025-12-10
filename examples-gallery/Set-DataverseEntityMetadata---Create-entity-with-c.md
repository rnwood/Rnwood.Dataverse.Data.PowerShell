---
title: "Set-DataverseEntityMetadata - Create entity with custom primary name length"
tags: ['Metadata']
source: "Set-DataverseEntityMetadata.md"
---
Creates an entity with a primary name attribute limited to 50 characters.

```powershell
Set-DataverseEntityMetadata -EntityName new_code `
   -SchemaName new_Code `
   -DisplayName "Code" `
   -DisplayCollectionName "Codes" `
   -OwnershipType UserOwned `
   -PrimaryAttributeSchemaName new_code `
   -PrimaryAttributeDisplayName "Code" `
   -PrimaryAttributeMaxLength 50

```
