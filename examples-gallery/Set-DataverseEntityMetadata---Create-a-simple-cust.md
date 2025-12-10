---
title: "Set-DataverseEntityMetadata - Create a simple custom entity"
tags: ['Metadata']
source: "Set-DataverseEntityMetadata.md"
---
Creates a new user-owned entity named `new_product` with a primary name attribute.

```powershell
Set-DataverseEntityMetadata -EntityName new_product `
   -SchemaName new_Product `
   -DisplayName "Product" `
   -DisplayCollectionName "Products" `
   -Description "Custom product catalog" `
   -OwnershipType UserOwned `
   -PrimaryAttributeSchemaName new_name `
   -PrimaryAttributeDisplayName "Product Name"

```
