
# Plugin Management Cmdlets - Usage Examples

This document provides practical examples for managing Dataverse plugins using the new cmdlets.

## Prerequisites

```powershell
# Connect to your Dataverse environment
$connection = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
```

## Complete Plugin Registration Example

This example shows how to register a complete plugin from start to finish.

### 1. Upload Plugin Assembly

```powershell
# Upload a plugin assembly from a DLL file
$assembly = Set-DataversePluginAssembly `
    -Connection $connection `
    -Name "MyCompany.Plugins" `
    -FilePath "C:\Development\MyPlugins\bin\Debug\MyCompany.Plugins.dll" `
    -IsolationMode 2 `
    -Description "My company's custom plugins" `
    -PassThru

Write-Host "Assembly uploaded with ID: $($assembly.Id)"
```

### 2. Register Plugin Type

```powershell
# Register a plugin type (class) from the assembly
$pluginType = Set-DataversePluginType `
    -Connection $connection `
    -PluginAssemblyId $assembly.Id `
    -TypeName "MyCompany.Plugins.AccountValidationPlugin" `
    -FriendlyName "Account Validation Plugin" `
    -Description "Validates account records before creation" `
    -PassThru

Write-Host "Plugin type registered with ID: $($pluginType.Id)"
```

### 3. Get SDK Message and Filter IDs

```powershell
# Get the SDK message for 'Create'
$createMessage = Get-DataverseRecord -Connection $connection `
    -TableName sdkmessage `
    -FilterValues @{ name = "Create" } `
    -Columns sdkmessageid, name

# Get the filter for 'account' entity
$accountFilter = Get-DataverseRecord -Connection $connection `
    -TableName sdkmessagefilter `
    -FilterValues @{ 
        sdkmessageid = $createMessage.sdkmessageid
        primaryobjecttypecode = "account"
    } `
    -Columns sdkmessagefilterid

Write-Host "Message ID: $($createMessage.sdkmessageid)"
Write-Host "Filter ID: $($accountFilter.sdkmessagefilterid)"
```

### 4. Register Plugin Step

```powershell
# Register a plugin step for PreValidation on Account Create
$step = Set-DataversePluginStep `
    -Connection $connection `
    -Name "Validate Account on Create" `
    -PluginTypeId $pluginType.Id `
    -SdkMessageId $createMessage.sdkmessageid `
    -SdkMessageFilterId $accountFilter.sdkmessagefilterid `
    -Stage 10 `
    -Mode 0 `
    -Rank 1 `
    -FilteringAttributes "name","revenue","primarycontactid" `
    -Description "Validates account data before creation" `
    -PassThru

Write-Host "Plugin step registered with ID: $($step.Id)"
```

> [!TIP]
> The `-FilteringAttributes` parameter accepts an array of attribute names. Tab completion is available for each attribute name, making it easier to specify the correct attributes.

### 5. Register Step Image (Optional)

```powershell
# Register a pre-image to access existing data (for Update operations)
$preImage = Set-DataversePluginStepImage `
    -Connection $connection `
    -SdkMessageProcessingStepId $step.Id `
    -EntityAlias "PreImage" `
    -ImageType 0 `
    -Attributes "name","revenue","primarycontactid" `
    -MessagePropertyName "Target" `
    -PassThru

Write-Host "Pre-image registered with ID: $($preImage.Id)"

# Register a post-image
$postImage = Set-DataversePluginStepImage `
    -Connection $connection `
    -SdkMessageProcessingStepId $step.Id `
    -EntityAlias "PostImage" `
    -ImageType 1 `
    -Attributes "name","revenue","primarycontactid" `
    -MessagePropertyName "Target" `
    -PassThru

Write-Host "Post-image registered with ID: $($postImage.Id)"
```

## Query and Management Examples

### List All Plugin Assemblies

```powershell
Get-DataversePluginAssembly -Connection $connection -All | 
    Select-Object name, version, isolationmode, @{N='Id';E={$_.Id}} |
    Format-Table
```

### Find Plugin Types for an Assembly

```powershell
$assembly = Get-DataversePluginAssembly -Connection $connection -Name "MyCompany.Plugins"

Get-DataversePluginType -Connection $connection -PluginAssemblyId $assembly.Id |
    Select-Object typename, friendlyname, @{N='Id';E={$_.Id}} |
    Format-Table
```

### List Steps for a Plugin Type

```powershell
$pluginType = Get-DataversePluginType -Connection $connection -TypeName "MyCompany.Plugins.AccountPlugin"

Get-DataversePluginStep -Connection $connection -PluginTypeId $pluginType.Id |
    Select-Object name, stage, mode, rank, @{N='Id';E={$_.Id}} |
    Format-Table
```

### Update Assembly with New Version

```powershell
# Get existing assembly
$assembly = Get-DataversePluginAssembly -Connection $connection -Name "MyCompany.Plugins"

# Update with new DLL
Set-DataversePluginAssembly `
    -Connection $connection `
    -Id $assembly.Id `
    -Name "MyCompany.Plugins" `
    -FilePath "C:\Development\MyPlugins\bin\Release\MyCompany.Plugins.dll" `
    -Version "2.0.0"

Write-Host "Assembly updated successfully"
```

## Cleanup Examples

### Remove a Plugin Step

```powershell
# Find and remove by name
$step = Get-DataversePluginStep -Connection $connection -Name "Validate Account on Create"
Remove-DataversePluginStep -Connection $connection -Id $step.Id -Confirm:$false
```

### Remove All Steps for a Plugin Type

```powershell
$pluginType = Get-DataversePluginType -Connection $connection -TypeName "MyCompany.Plugins.AccountPlugin"

Get-DataversePluginStep -Connection $connection -PluginTypeId $pluginType.Id |
    ForEach-Object {
        Write-Host "Removing step: $($_.name)"
        Remove-DataversePluginStep -Connection $connection -Id $_.Id -Confirm:$false
    }
```

### Unregister Complete Plugin

```powershell
$assembly = Get-DataversePluginAssembly -Connection $connection -Name "MyCompany.Plugins"

# 1. Remove all step images for all steps in all types
Get-DataversePluginType -Connection $connection -PluginAssemblyId $assembly.Id | ForEach-Object {
    $pluginType = $_
    Get-DataversePluginStep -Connection $connection -PluginTypeId $pluginType.Id | ForEach-Object {
        $step = $_
        Get-DataversePluginStepImage -Connection $connection -SdkMessageProcessingStepId $step.Id | ForEach-Object {
            Write-Host "Removing image: $($_.entityalias) for step: $($step.name)"
            Remove-DataversePluginStepImage -Connection $connection -Id $_.Id -Confirm:$false
        }
        Write-Host "Removing step: $($step.name)"
        Remove-DataversePluginStep -Connection $connection -Id $step.Id -Confirm:$false
    }
    Write-Host "Removing type: $($pluginType.typename)"
    Remove-DataversePluginType -Connection $connection -Id $pluginType.Id -Confirm:$false
}

# 2. Remove the assembly
Write-Host "Removing assembly: $($assembly.name)"
Remove-DataversePluginAssembly -Connection $connection -Id $assembly.Id -Confirm:$false

Write-Host "Plugin completely unregistered"
```

## Plugin Package Examples (for modern plugins)

Plugin packages are the modern approach to deploying plugins to Dataverse. They use NuGet packages (.nupkg files) instead of DLL assemblies. This approach provides better versioning, dependency management, and deployment options.

For more information on plugin packages, see [Microsoft's Plugin Package documentation](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/reference/entities/pluginpackage).

### Upload Plugin Package

```powershell
# Upload a NuGet package from file
$package = Set-DataversePluginPackage `
    -Connection $connection `
    -UniqueName "MyCompanyPlugins" `
    -FilePath "C:\Development\MyPlugins\bin\Release\MyCompany.Plugins.1.0.0.nupkg" `
    -Version "1.0.0" `
    -Description "My company plugins package" `
    -PassThru

Write-Host "Package uploaded with ID: $($package.Id)"
```

### Upload from Byte Array

```powershell
# Read package file into byte array
$packageBytes = [System.IO.File]::ReadAllBytes("C:\Plugins\MyPlugin.nupkg")

# Upload using byte array
$package = Set-DataversePluginPackage `
    -Connection $connection `
    -UniqueName "MyPluginPackage" `
    -Content $packageBytes `
    -Version "2.0.0" `
    -Description "Updated plugin package" `
    -PassThru

Write-Host "Package uploaded: $($package.uniquename) v$($package.version)"
```

### List All Plugin Packages

```powershell
# Get all plugin packages
Get-DataversePluginPackage -Connection $connection |
    Select-Object uniquename, version, description, @{N='Id';E={$_.Id}} |
    Format-Table

# Or with explicit -All parameter
Get-DataversePluginPackage -Connection $connection -All |
    Format-Table uniquename, version, description
```

### Get Package by Unique Name

```powershell
# Query a specific package by unique name
$package = Get-DataversePluginPackage -Connection $connection -UniqueName "MyCompanyPlugins"

if ($package) {
    Write-Host "Found package: $($package.uniquename) v$($package.version)"
    Write-Host "Package ID: $($package.Id)"
}
```

### Get Package by ID

```powershell
# Query a specific package by ID
$packageId = [Guid]"12345678-1234-1234-1234-123456789012"
$package = Get-DataversePluginPackage -Connection $connection -Id $packageId

Write-Host "Package: $($package.uniquename)"
Write-Host "Version: $($package.version)"
Write-Host "Description: $($package.description)"
```

### Update Existing Plugin Package

```powershell
# Get existing package
$package = Get-DataversePluginPackage -Connection $connection -UniqueName "MyCompanyPlugins"

# Update with new version
Set-DataversePluginPackage `
    -Connection $connection `
    -Id $package.Id `
    -UniqueName "MyCompanyPlugins" `
    -FilePath "C:\Development\MyPlugins\bin\Release\MyCompany.Plugins.2.0.0.nupkg" `
    -Version "2.0.0" `
    -Description "Updated version with bug fixes"

Write-Host "Package updated successfully"
```

### Remove Plugin Package

```powershell
# Find and remove by unique name
$package = Get-DataversePluginPackage -Connection $connection -UniqueName "MyOldPackage"
Remove-DataversePluginPackage -Connection $connection -Id $package.Id -Confirm:$false

# Safe removal with IfExists flag
Remove-DataversePluginPackage -Connection $connection -Id $someId -IfExists -Confirm:$false
```

### Compare Package Versions

```powershell
# Get all versions of a package (if multiple exist)
$packages = Get-DataversePluginPackage -Connection $connection |
    Where-Object { $_.uniquename -like "*MyCompany*" } |
    Sort-Object version -Descending

Write-Host "Found $($packages.Count) matching packages:"
$packages | Format-Table uniquename, version, @{N='Modified';E={$_.modifiedon}}
```

## Advanced Scenarios

### Disable a Plugin Step

```powershell
$step = Get-DataversePluginStep -Connection $connection -Name "My Step"

Set-DataversePluginStep `
    -Connection $connection `
    -Id $step.Id `
    -Name $step.name `
    -PluginTypeId $step.plugintypeid.Id `
    -SdkMessageId $step.sdkmessageid.Id `
    -Stage $step.stage `
    -Mode $step.mode `
    -StateCode 1 `
    -StatusCode 2

Write-Host "Step disabled"
```

### Change Step Execution Order

```powershell
$step = Get-DataversePluginStep -Connection $connection -Name "My Step"

Set-DataversePluginStep `
    -Connection $connection `
    -Id $step.Id `
    -Name $step.name `
    -PluginTypeId $step.plugintypeid.Id `
    -SdkMessageId $step.sdkmessageid.Id `
    -Stage $step.stage `
    -Mode $step.mode `
    -Rank 10

Write-Host "Step rank changed to 10"
```

## WhatIf and Confirm Support

All Set and Remove cmdlets support `-WhatIf` and `-Confirm`:

```powershell
# Preview what would be removed
Remove-DataversePluginStep -Connection $connection -Id $stepId -WhatIf

# Prompt for confirmation
Remove-DataversePluginStep -Connection $connection -Id $stepId

# Skip confirmation
Remove-DataversePluginStep -Connection $connection -Id $stepId -Confirm:$false
```

## Error Handling

```powershell
try {
    $assembly = Set-DataversePluginAssembly `
        -Connection $connection `
        -Name "MyPlugin" `
        -FilePath "C:\Plugins\MyPlugin.dll" `
        -PassThru
    
    Write-Host "Assembly uploaded successfully: $($assembly.Id)"
}
catch {
    Write-Error "Failed to upload assembly: $($_.Exception.Message)"
}

# Use IfExists for safe removal
Remove-DataversePluginAssembly -Connection $connection -Id $someId -IfExists -Confirm:$false
```

## Notes

- **Isolation Mode**: 0=None, 1=Sandbox, 2=External (recommended for new plugins)
- **Stage**: 10=PreValidation, 20=PreOperation, 40=PostOperation
- **Mode**: 0=Synchronous, 1=Asynchronous
- **Image Type**: 0=PreImage, 1=PostImage, 2=Both
- **PassThru**: Returns the created/updated object for further processing
- **IfExists**: Prevents errors when deleting non-existent items

## Additional Resources

- [Microsoft Dataverse Plugin Development](https://docs.microsoft.com/powerapps/developer/data-platform/plug-ins)
- [SDK Message Reference](https://docs.microsoft.com/powerapps/developer/data-platform/reference/about-entity-reference)
