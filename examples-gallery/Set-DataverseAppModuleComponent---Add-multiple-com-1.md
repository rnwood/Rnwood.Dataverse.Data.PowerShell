---
title: "Set-DataverseAppModuleComponent - Add multiple components via pipeline"
tags: ['Metadata']
source: "Set-DataverseAppModuleComponent.md"
---
Adds multiple entities to an app module via pipeline input.

```powershell
$entities = @("contact", "account", "lead") | ForEach-Object {
    $metadata = Get-DataverseEntityMetadata -EntityName $_
    [PSCustomObject]@{
        AppModuleUniqueName = "salesapp"
        ObjectId = $metadata.MetadataId
        ComponentType = [Rnwood.Dataverse.Data.PowerShell.Commands.Model.AppModuleComponentType]::Entity
    }
}
$entities | Set-DataverseAppModuleComponent 

```

