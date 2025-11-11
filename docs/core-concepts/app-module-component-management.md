<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
## Table of Contents

- [App Module Component Management](#app-module-component-management)
  - [Retrieving Components](#retrieving-components)
    - [Get all components for an app by unique name](#get-all-components-for-an-app-by-unique-name)
    - [Get components for an app by Id](#get-components-for-an-app-by-id)
    - [Get components of a specific type (e.g. Entities)](#get-components-of-a-specific-type-eg-entities)
    - [Get apps which include a given entity](#get-apps-which-include-a-given-entity)
    - [Get raw component records](#get-raw-component-records)
  - [Creating Components](#creating-components)
    - [Add an entity to an app (by unique name)](#add-an-entity-to-an-app-by-unique-name)
    - [Add a form as default](#add-a-form-as-default)
    - [Add multiple entities in bulk](#add-multiple-entities-in-bulk)
  - [Updating Components](#updating-components)
    - [Set a component as default and change behavior](#set-a-component-as-default-and-change-behavior)
    - [Safely skip updates](#safely-skip-updates)
    - [Prevent creation (update-only mode)](#prevent-creation-update-only-mode)
  - [Removing Components](#removing-components)
    - [Remove by component Id](#remove-by-component-id)
    - [Remove by app unique name & object Id](#remove-by-app-unique-name--object-id)
    - [Safe removal](#safe-removal)
    - [Preview removal](#preview-removal)
  - [Component Types](#component-types)
  - [Root Component Behavior](#root-component-behavior)
  - [Tips & Best Practices](#tips--best-practices)
  - [See Also](#see-also)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

# App Module Component Management

App module components define which entities, forms, views, charts, business process flows, ribbon commands and site maps are included inside a model-driven app module.

This guide covers creating, updating, retrieving and removing app module components with the Dataverse PowerShell module.

## Retrieving Components

### Get all components for an app by unique name
```powershell
Get-DataverseAppModuleComponent -Connection $c -AppModuleUniqueName "my_app"
```

### Get components for an app by Id
```powershell
$app = Get-DataverseAppModule -Connection $c -UniqueName "my_app"
Get-DataverseAppModuleComponent -Connection $c -AppModuleId $app.Id
```

### Get components of a specific type (e.g. Entities)
```powershell
Get-DataverseAppModuleComponent -Connection $c -AppModuleUniqueName "my_app" -ComponentType Entity
```

### Get apps which include a given entity
```powershell
$entity = Get-DataverseEntityMetadata -Connection $c -EntityName "contact"
Get-DataverseAppModuleComponent -Connection $c -ObjectId $entity.MetadataId
```

### Get raw component records
```powershell
Get-DataverseAppModuleComponent -Connection $c -AppModuleUniqueName "my_app" -Raw
```
Returns raw `appmodulecomponent` entities instead of simplified PSObjects.

## Creating Components

Components are added with `Set-DataverseAppModuleComponent`. Creation requires:
- AppModuleId or AppModuleUniqueName
- ObjectId (the underlying component record Id / metadata Id)
- ComponentType

### Add an entity to an app (by unique name)
```powershell
$entity = Get-DataverseEntityMetadata -Connection $c -EntityName "account"
$componentId = Set-DataverseAppModuleComponent -Connection $c -PassThru `
  -AppModuleUniqueName "my_app" `
  -ObjectId $entity.MetadataId `
  -ComponentType Entity
```

### Add a form as default
```powershell
$form = Get-DataverseRecord -Connection $c -TableName systemform -FilterValues @{ name = "Contact Main Form" }
$componentId = Set-DataverseAppModuleComponent -Connection $c -PassThru `
  -AppModuleUniqueName "my_app" `
  -ObjectId $form.systemformid `
  -ComponentType Form `
  -IsDefault $true
```

### Add multiple entities in bulk
```powershell
$entities = @("contact","lead","opportunity") | ForEach-Object {
  Get-DataverseEntityMetadata -Connection $c -EntityName $_
}
$entities | ForEach-Object {
  Set-DataverseAppModuleComponent -Connection $c -AppModuleUniqueName "sales_app" -ObjectId $_.MetadataId -ComponentType Entity
}
```

## Updating Components

### Set a component as default and change behavior
```powershell
Set-DataverseAppModuleComponent -Connection $c -Id $componentId -IsDefault $true -RootComponentBehavior DoNotIncludeSubcomponents
```

Only specified parameters are updated; unspecified attributes remain unchanged.

### Safely skip updates
```powershell
Set-DataverseAppModuleComponent -Connection $c -Id $componentId -IsDefault $true -NoUpdate
```

### Prevent creation (update-only mode)
```powershell
Set-DataverseAppModuleComponent -Connection $c -AppModuleUniqueName "my_app" -ObjectId $entity.MetadataId -ComponentType Entity -NoCreate
```

## Removing Components

### Remove by component Id
```powershell
Remove-DataverseAppModuleComponent -Connection $c -Id $componentId -Confirm:$false
```

### Remove by app unique name & object Id
```powershell
Remove-DataverseAppModuleComponent -Connection $c -AppModuleUniqueName "my_app" -ObjectId $entity.MetadataId -Confirm:$false
```

### Safe removal
```powershell
Remove-DataverseAppModuleComponent -Connection $c -Id $maybeId -IfExists -Confirm:$false
```

### Preview removal
```powershell
Remove-DataverseAppModuleComponent -Connection $c -AppModuleUniqueName "my_app" -ObjectId $entity.MetadataId -WhatIf
```

## Component Types
| Enum | Value | Description |
|------|-------|-------------|
| Entity | 1 | Table/entity |
| View | 26 | Saved query view |
| BusinessProcessFlow | 29 | BPF definition |
| RibbonCommand | 48 | Ribbon command |
| Chart | 59 | Saved query visualization |
| Form | 60 | Form definition |
| SiteMap | 62 | Site map |

## Root Component Behavior
| Enum | Value | Description |
|------|-------|-------------|
| IncludeSubcomponents | 0 | Include all subcomponents |
| DoNotIncludeSubcomponents | 1 | Only main component |
| IncludeAsShell | 2 | Shell only |

## See Also
- Get-DataverseAppModuleComponent
- Set-DataverseAppModuleComponent
- Remove-DataverseAppModuleComponent
- Get-DataverseAppModule
- Set-DataverseAppModule
- Remove-DataverseAppModule
