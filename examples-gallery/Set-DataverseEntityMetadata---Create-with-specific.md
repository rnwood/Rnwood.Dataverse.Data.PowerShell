---
title: "Set-DataverseEntityMetadata - Create with specific connection"
tags: ['Metadata']
source: "Set-DataverseEntityMetadata.md"
---
Creates an entity using a specific connection instead of the default connection.

```powershell
$conn = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive

Set-DataverseEntityMetadata onn `
   -EntityName new_inventory `
   -SchemaName new_Inventory `
   -DisplayName "Inventory Item" `
   -DisplayCollectionName "Inventory Items" `
   -OwnershipType UserOwned `
   -PrimaryAttributeSchemaName new_name `
   -PrimaryAttributeDisplayName "Item Name"

```

