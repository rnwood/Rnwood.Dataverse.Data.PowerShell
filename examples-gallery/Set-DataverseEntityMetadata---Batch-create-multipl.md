---
title: "Set-DataverseEntityMetadata - Batch create multiple entities"
tags: ['Metadata']
source: "Set-DataverseEntityMetadata.md"
---
Creates multiple entities by iterating through a collection.

```powershell
$entities = @(
    @{ SchemaName = "new_Category"; DisplayName = "Category"; PrimaryAttribute = "new_name" }
    @{ SchemaName = "new_Tag"; DisplayName = "Tag"; PrimaryAttribute = "new_name" }
    @{ SchemaName = "new_Comment"; DisplayName = "Comment"; PrimaryAttribute = "new_text" }
)

foreach ($entity in $entities) {
    $name = $entity.SchemaName.ToLower()
    Set-DataverseEntityMetadata -EntityName $name `
       -SchemaName $entity.SchemaName `
       -DisplayName $entity.DisplayName `
       -DisplayCollectionName "$($entity.DisplayName)s" `
       -OwnershipType UserOwned `
       -PrimaryAttributeSchemaName $entity.PrimaryAttribute `
       -PrimaryAttributeDisplayName "Name"
}

```
