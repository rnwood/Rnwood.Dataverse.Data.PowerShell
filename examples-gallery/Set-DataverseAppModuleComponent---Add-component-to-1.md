---
title: "Set-DataverseAppModuleComponent - Add component to app by unique name and then update"
tags: ['Metadata']
source: "Set-DataverseAppModuleComponent.md"
---
Creates a component referencing the app by unique name, then sets it as default.

```powershell
$entity = Get-DataverseEntityMetadata -EntityName "contact"
$compId = Set-DataverseAppModuleComponent -PassThru -AppModuleUniqueName "myapp" -ObjectId $entity.MetadataId -ComponentType Entity
Set-DataverseAppModuleComponent -Id $compId -IsDefault $true

```

