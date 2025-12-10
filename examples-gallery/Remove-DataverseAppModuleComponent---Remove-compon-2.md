---
title: "Remove-DataverseAppModuleComponent - Remove component by app module unique name"
tags: ['Metadata']
source: "Remove-DataverseAppModuleComponent.md"
---
Removes the contact entity from an app module by specifying the app's unique name and entity metadata ID.

```powershell
$entityMetadata = Get-DataverseEntityMetadata -EntityName "contact"
Remove-DataverseAppModuleComponent -AppModuleUniqueName "myapp" -ObjectId $entityMetadata.MetadataId -Confirm:$false

```

