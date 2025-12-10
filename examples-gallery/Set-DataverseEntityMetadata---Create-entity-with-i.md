---
title: "Set-DataverseEntityMetadata - Create entity with icon properties"
tags: ['Metadata']
source: "Set-DataverseEntityMetadata.md"
---
Creates an entity with custom icon properties. Icons control the visual appearance of the entity in the Dataverse UI.

```powershell
Set-DataverseEntityMetadata -EntityName new_product `
   -SchemaName new_Product `
   -DisplayName "Product" `
   -DisplayCollectionName "Products" `
   -OwnershipType UserOwned `
   -PrimaryAttributeSchemaName new_name `
   -PrimaryAttributeDisplayName "Product Name" `
   -IconVectorName "svg_product" `
   -IconLargeName "Entity/product_large.png" `
   -IconMediumName "Entity/product_medium.png" `
   -IconSmallName "Entity/product_small.png"

```
