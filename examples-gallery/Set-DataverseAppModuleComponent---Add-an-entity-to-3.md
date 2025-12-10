---
title: "Set-DataverseAppModuleComponent - Add an entity to an app module by unique name"
tags: ['Metadata']
source: "Set-DataverseAppModuleComponent.md"
---
Adds the account entity to an app module using the app module's unique name.

```powershell
$entityMetadata = Get-DataverseEntityMetadata -EntityName "account"
Set-DataverseAppModuleComponent -PassThru `
   -AppModuleUniqueName "myapp" `
   -ObjectId $entityMetadata.MetadataId `
   -ComponentType Entity

```

