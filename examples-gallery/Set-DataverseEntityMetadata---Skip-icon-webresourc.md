---
title: "Set-DataverseEntityMetadata - Skip icon webresource validation"
tags: ['Metadata']
source: "Set-DataverseEntityMetadata.md"
---
Creates an entity with icon validation disabled. The cmdlet will not check if "new_product_icon" webresource exists or has the correct type. Use this when you plan to create the webresource later or want to bypass validation for development/testing.

```powershell
# Create an entity without validating icon webresource existence
Set-DataverseEntityMetadata -EntityName new_product `
   -SchemaName new_Product `
   -DisplayName "Product" `
   -DisplayCollectionName "Products" `
   -OwnershipType UserOwned `
   -PrimaryAttributeSchemaName new_name `
   -PrimaryAttributeDisplayName "Product Name" `
   -IconVectorName "new_product_icon" `
   -SkipIconValidation

```
