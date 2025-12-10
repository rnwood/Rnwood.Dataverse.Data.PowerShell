---
title: "Set-DataverseEntityMetadata - Create entity with validated icon webresources"
tags: ['Metadata']
source: "Set-DataverseEntityMetadata.md"
---
Creates an entity with icon validation enabled (default). The cmdlet validates that:
- "new_product_svg" references an SVG webresource (type 11)
- "new_product_large.png" references a PNG, JPG, or GIF webresource (type 5, 6, or 7)
- "new_product_medium.png" references a PNG, JPG, or GIF webresource (type 5, 6, or 7)
- "new_product_small.png" references a PNG, JPG, or GIF webresource (type 5, 6, or 7)

If any webresource doesn't exist or has the wrong type, an error is thrown.

```powershell
# Create an entity with all icon properties that reference valid webresources
Set-DataverseEntityMetadata -EntityName new_product `
   -SchemaName new_Product `
   -DisplayName "Product" `
   -DisplayCollectionName "Products" `
   -OwnershipType UserOwned `
   -PrimaryAttributeSchemaName new_name `
   -PrimaryAttributeDisplayName "Product Name" `
   -IconVectorName "new_product_svg" `
   -IconLargeName "new_product_large.png" `
   -IconMediumName "new_product_medium.png" `
   -IconSmallName "new_product_small.png"

```
