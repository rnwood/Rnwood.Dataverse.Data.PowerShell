<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
## Table of Contents

- [App Module Management](#app-module-management)
  - [Retrieving App Modules](#retrieving-app-modules)
    - [Get a specific app module by ID](#get-a-specific-app-module-by-id)
    - [Get an app module by UniqueName](#get-an-app-module-by-uniquename)
    - [Get all app modules](#get-all-app-modules)
    - [Find app modules by name with wildcards](#find-app-modules-by-name-with-wildcards)
    - [Get raw attribute values](#get-raw-attribute-values)
    - [Get unpublished app modules](#get-unpublished-app-modules)
  - [Creating and Updating App Modules](#creating-and-updating-app-modules)
    - [Create a new app module](#create-a-new-app-module)
    - [Update an existing app module by ID](#update-an-existing-app-module-by-id)
    - [Update an app module by UniqueName](#update-an-app-module-by-uniquename)
    - [Create with icon and form factor](#create-with-icon-and-form-factor)
    - [Validate and publish an app](#validate-and-publish-an-app)
    - [Safe update with NoUpdate](#safe-update-with-noupdate)
    - [Publish an app after creation/update](#publish-an-app-after-creationupdate)
    - [Validate an app before publishing](#validate-an-app-before-publishing)
  - [Deleting App Modules](#deleting-app-modules)
    - [Delete by ID](#delete-by-id)
    - [Delete by UniqueName](#delete-by-uniquename)
    - [Safe deletion with IfExists](#safe-deletion-with-ifexists)
    - [Preview deletion with WhatIf](#preview-deletion-with-whatif)
  - [Common Scenarios](#common-scenarios)
    - [Clone an app module](#clone-an-app-module)
    - [Bulk create apps for multiple purposes](#bulk-create-apps-for-multiple-purposes)
    - [Audit app module configuration](#audit-app-module-configuration)
    - [Clean up development apps](#clean-up-development-apps)
  - [App Module Properties](#app-module-properties)
  - [See Also](#see-also)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

# App Module Management

App modules (model-driven apps) define the structure, navigation, and components of model-driven applications in Dataverse. This guide covers creating, updating, retrieving, and deleting app modules using PowerShell.

## Retrieving App Modules

### Get a specific app module by ID

```powershell
$app = Get-DataverseAppModule -Connection $c -Id $appId
```

Returns parsed properties like Id, UniqueName, Name, Description, PublishedOn, etc.

### Get an app module by UniqueName

```powershell
$app = Get-DataverseAppModule -Connection $c -UniqueName "msdyn_SalesHub"
```

### Get all app modules

```powershell
$apps = Get-DataverseAppModule -Connection $c
```

### Find app modules by name with wildcards

```powershell
$salesApps = Get-DataverseAppModule -Connection $c -Name "Sales*"
```

### Get raw attribute values

```powershell
$app = Get-DataverseAppModule -Connection $c -Id $appId -Raw
```

Returns all raw attributes from the appmodule record instead of parsed properties.

### Get unpublished app modules

```powershell
$unpublishedApps = Get-DataverseAppModule -Connection $c -Unpublished
```

## Creating and Updating App Modules

### Create a new app module

```powershell
$appId = Set-DataverseAppModule -Connection $c -PassThru `
    -UniqueName "my_custom_app" `
    -Name "My Custom Application" `
    -Description "A custom model-driven app"
```

### Update an existing app module by ID

```powershell
Set-DataverseAppModule -Connection $c `
    -Id $appId `
    -Name "Updated App Name" `
    -Description "Updated description"
```

### Update an app module by UniqueName

```powershell
Set-DataverseAppModule -Connection $c `
    -UniqueName "my_custom_app" `
    -Description "New description"
```

### Create with icon and form factor

```powershell
$iconResourceId = (Get-DataverseRecord -Connection $c -TableName webresource -FilterValues @{name="app_icon.svg"}).webresourceid

Set-DataverseAppModule -Connection $c -PassThru `
    -UniqueName "sales_app" `
    -Name "Sales Application" `
    -WebResourceId $iconResourceId `
    -FormFactor 1 `  # Main form factor
    -ClientType 1

### Create with navigation type and featured flag

```powershell
Set-DataverseAppModule -Connection $c -PassThru `
    -UniqueName "featured_app" `
    -Name "Featured App" `
    -NavigationType MultiSession `
    -IsFeatured $true
```

### Validate and publish an app

```powershell
Set-DataverseAppModule -Connection $c -PassThru `
    -UniqueName "ready_app" `
    -Name "Ready App" `
    -Validate `
    -Publish
```
```

### Upsert pattern with NoCreate

```powershell
# Only update if exists, don't create new
Set-DataverseAppModule -Connection $c `
    -UniqueName "existing_app" `
    -Name "Updated Name" `
    -NoCreate
```

### Safe update with NoUpdate

```powershell
# Only create if doesn't exist, don't update existing
Set-DataverseAppModule -Connection $c `
    -UniqueName "new_app" `
    -Name "New App" `
    -NoUpdate
```

### Publish an app after creation/update

```powershell
Set-DataverseAppModule -Connection $c -PassThru `
    -UniqueName "my_app" `
    -Name "My App" `
    -Publish
```

### Validate an app before publishing

```powershell
Set-DataverseAppModule -Connection $c -PassThru `
    -UniqueName "my_app" `
    -Name "My App" `
    -Validate `
    -Publish
```

## Deleting App Modules

### Delete by ID

```powershell
Remove-DataverseAppModule -Connection $c -Id $appId -Confirm:$false
```

### Delete by UniqueName

```powershell
Remove-DataverseAppModule -Connection $c -UniqueName "my_app" -Confirm:$false
```

### Safe deletion with IfExists

```powershell
# Don't error if app doesn't exist
Remove-DataverseAppModule -Connection $c -UniqueName "maybe_exists" -IfExists -Confirm:$false
```

### Preview deletion with WhatIf

```powershell
Remove-DataverseAppModule -Connection $c -UniqueName "my_app" -WhatIf
```

## Common Scenarios

### Clone an app module

```powershell
# Get existing app
$originalApp = Get-DataverseAppModule -Connection $c -Id $originalAppId

# Create new app with same properties
$newAppId = Set-DataverseAppModule -Connection $c -PassThru `
    -UniqueName "$($originalApp.UniqueName)_copy" `
    -Name "$($originalApp.Name) (Copy)" `
    -Description $originalApp.Description `
    -WebResourceId $originalApp.WebResourceId `
    -FormFactor $originalApp.FormFactor `
    -ClientType $originalApp.ClientType
```

### Bulk create apps for multiple purposes

```powershell
$appConfigs = @(
    @{ UniqueName = "sales_app"; Name = "Sales Hub"; Description = "Sales management app" },
    @{ UniqueName = "service_app"; Name = "Customer Service"; Description = "Service management app" },
    @{ UniqueName = "marketing_app"; Name = "Marketing Hub"; Description = "Marketing management app" }
)

foreach ($config in $appConfigs) {
    Set-DataverseAppModule -Connection $c -PassThru `
        -UniqueName $config.UniqueName `
        -Name $config.Name `
        -Description $config.Description
}
```

### Audit app module configuration

```powershell
# Get all apps and export metadata
$apps = Get-DataverseAppModule -Connection $c

$apps | Select-Object UniqueName, Name, Description, PublishedOn, FormFactor, IsFeatured |
    Export-Csv -Path "app-audit.csv" -NoTypeInformation
```

### Clean up development apps

```powershell
# Remove all apps with names starting with "dev_"
Get-DataverseAppModule -Connection $c -Name "dev_*" |
    ForEach-Object {
        Remove-DataverseAppModule -Connection $c -Id $_.Id -Confirm:$false
    }
```

## App Module Properties

When working with app modules, you'll encounter these key properties:

- **Id**: The unique identifier (GUID) of the app module
- **UniqueName**: The schema name used to reference the app (required for creation)
- **Name**: The display name shown in the app launcher
- **Description**: Optional description of the app's purpose
- **WebResourceId**: The web resource ID for the app icon
- **FormFactor**: The form factor (1=Main, 2=Quick, 3=Preview, 4=Dashboard)
- **ClientType**: The client type for the app
- **NavigationType**: The navigation type (Single session or Multi session)
- **IsFeatured**: Whether the app is featured in the app launcher
- **PublishedOn**: When the app was last published

## See Also

- [Connection Management](connections.md) � Managing Dataverse connections
- [Get-DataverseAppModule cmdlet reference](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseAppModule.md)
- [Set-DataverseAppModule cmdlet reference](../../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseAppModule.md)
- [Remove-DataverseAppModule cmdlet reference](../../Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseAppModule.md)