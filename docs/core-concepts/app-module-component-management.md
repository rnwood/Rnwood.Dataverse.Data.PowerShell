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

## Tips & Best Practices
- Prefer AppModuleUniqueName for environment-independent scripts.
- Use PassThru to capture created component Ids.
- Combine ComponentType + ObjectId filters for precise queries.
- Use -NoCreate / -NoUpdate to enforce idempotent deployment semantics.
- Pipeline scenarios: supply PSCustomObject with matching property names (AppModuleId / AppModuleUniqueName, ObjectId, ComponentType, etc.).
- Retrieval uses published app modules first; creation prefers unique name resolution (unpublished first).

## See Also
- Get-DataverseAppModuleComponent
- Set-DataverseAppModuleComponent
- Remove-DataverseAppModuleComponent
- Get-DataverseAppModule
- Set-DataverseAppModule
- Remove-DataverseAppModule
