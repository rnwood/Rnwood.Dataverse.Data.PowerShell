---
title: "Set-DataverseEntityMetadata - Create entity and return metadata"
tags: ['Metadata']
source: "Set-DataverseEntityMetadata.md"
---
Creates an entity and returns its metadata using `-PassThru`.

```powershell
$result = Set-DataverseEntityMetadata -EntityName new_item `
   -SchemaName new_Item `
   -DisplayName "Item" `
   -DisplayCollectionName "Items" `
   -OwnershipType UserOwned `
   -PrimaryAttributeSchemaName new_name `
   -PrimaryAttributeDisplayName "Item Name" `
   -PassThru

$result

# LogicalName      : new_item
# SchemaName       : new_Item
# DisplayName      : Item
# EntitySetName    : new_items
# MetadataId       : a1234567-89ab-cdef-0123-456789abcdef
# OwnershipType    : UserOwned
# IsCustomEntity   : True
# IsAuditEnabled   : False

```
