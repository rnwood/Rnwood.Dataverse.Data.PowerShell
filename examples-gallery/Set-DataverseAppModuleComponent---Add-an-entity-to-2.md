---
title: "Set-DataverseAppModuleComponent - Add an entity to an app module by ID"
tags: ['Metadata']
source: "Set-DataverseAppModuleComponent.md"
---
Adds the contact entity to an app module using the app module ID. (Parameter corrected: uses -AppModuleId)

```powershell
$app = Get-DataverseAppModule -UniqueName "myapp"
$entityMetadata = Get-DataverseEntityMetadata -EntityName "contact"
Set-DataverseAppModuleComponent -PassThru `
   -AppModuleId $app.Id `
   -ObjectId $entityMetadata.MetadataId `
   -ComponentType Entity

```

