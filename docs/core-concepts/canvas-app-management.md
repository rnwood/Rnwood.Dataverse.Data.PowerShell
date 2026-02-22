# Canvas App Management

This guide covers managing Canvas apps in Dataverse using PowerShell cmdlets with a local-first approach for optimal performance.

## Overview

Canvas apps are custom applications built using Microsoft Power Apps. This module provides comprehensive cmdlets for:

- **Canvas app lifecycle** - Create, retrieve, update, and delete Canvas apps
- **Local .msapp file operations** - Work with Canvas app packages (.msapp files) locally
- **Screen management** - Add, update, and remove screens using YAML definitions
- **Component management** - Manage reusable components using YAML definitions
- **Upsert operations** - Automatically create or update apps based on ID or Name

## Architecture

Canvas apps are stored in Dataverse as `.msapp` files, which are ZIP archives containing:
- **YAML files** - Screen and component definitions in Power Apps YAML format
- **Header.json** - App metadata and version information
- **Properties.json** - App properties and configuration
- **References/** - Data sources, resources, and themes

## Workflow

The recommended workflow for Canvas app management:

1. **Download or create** a .msapp file locally
2. **Modify** screens and components using the MsApp cmdlets
3. **Upload** the modified .msapp file to Dataverse using Set-DataverseCanvasApp

This approach is **significantly faster** than import/export for each individual change.

## Setting Up Default Connection

Before working with Canvas apps, establish a default connection:

```powershell
# Connect to Dataverse and set as default
Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive -SetAsDefault

# Verify connection
Get-DataverseWhoAmI
```

All Canvas app cmdlets will use this default connection unless you explicitly provide a different one via the `-Connection` parameter.

## Basic Canvas App Operations

### Retrieving Canvas Apps

Get Canvas apps by ID, name pattern, or other criteria:

```powershell
# Get all Canvas apps
Get-DataverseCanvasApp

# Get a specific Canvas app by ID
Get-DataverseCanvasApp -Id "12345678-1234-1234-1234-123456789012"

# Get Canvas apps by name pattern (supports wildcards)
Get-DataverseCanvasApp -Name "MyApp*"

# Get Canvas apps by display name pattern
Get-DataverseCanvasApp -DisplayName "Customer*"

# Get only unmanaged Canvas apps
Get-DataverseCanvasApp -Unmanaged

# Get Canvas app with document content (.msapp file)
$app = Get-DataverseCanvasApp -Name "MyApp" -IncludeDocument
# Save the .msapp file to disk
[System.IO.File]::WriteAllBytes("myapp.msapp", [System.Convert]::FromBase64String($app.document))
```

### Creating or Updating Canvas Apps (Upsert)

The `Set-DataverseCanvasApp` cmdlet automatically determines whether to create or update:

```powershell
# Create new Canvas app (or update if exists) by Name
Set-DataverseCanvasApp -Name "new_myapp" -DisplayName "My App" -MsAppPath "C:\apps\myapp.msapp"

# Update existing Canvas app by ID (or create if doesn't exist)
Set-DataverseCanvasApp -Id "12345678-1234-1234-1234-123456789012" -MsAppPath "C:\apps\updated.msapp"

# Create with additional metadata
Set-DataverseCanvasApp -Name "new_myapp" `
    -DisplayName "My Custom App" `
    -Description "This is my custom Canvas app" `
    -MsAppPath "C:\apps\myapp.msapp" `
    -PassThru  # Returns the app ID
```

**How Upsert Works:**
- If you provide an **ID**: Checks if app with that ID exists → Update if yes, Create with that ID if no
- If you provide a **Name**: Checks if app with that name exists → Update if yes, Create if no
- No need to know beforehand whether you're creating or updating

### Deleting Canvas Apps

Remove Canvas apps with optional error handling:

```powershell
# Delete by ID
Remove-DataverseCanvasApp -Id "12345678-1234-1234-1234-123456789012"

# Delete by Name
Remove-DataverseCanvasApp -Name "new_myapp"

# Delete with IfExists flag (no error if app doesn't exist)
Remove-DataverseCanvasApp -Name "new_maybeexists" -IfExists
```

## Working with .msapp Files Locally

> **Note:** The `Set-DataverseMsAppScreen`, `Set-DataverseMsAppComponent`, and `Set-DataverseMsAppProperties` cmdlets use Power Apps YAML format to modify .msapp files. **This functionality is experimental.** The Power Apps YAML format may change between releases and the results may need to be validated in Power Apps Studio. A warning is emitted at runtime for each of these cmdlets as a reminder. To suppress the warning, use `-WarningAction SilentlyContinue`.

### Understanding .msapp Structure

A .msapp file is a ZIP archive with this typical structure:

```
myapp.msapp/
├── Header.json                    # App version and metadata
├── Properties.json                # App properties
├── Src/
│   ├── App.pa.yaml               # App-level configuration
│   ├── Screen1.pa.yaml           # Screen definitions
│   ├── Screen2.pa.yaml
│   └── MyComponent.pa.yaml       # Component definitions
└── References/
    ├── DataSources.json          # Data connections
    ├── Resources.json            # Images, files, etc.
    └── Themes.json               # Theme definitions
```

### Screen Management

Screens are the main user interface pages in a Canvas app:

```powershell
# Get all screens from a .msapp file
$screens = Get-DataverseMsAppScreen -MsAppPath "myapp.msapp"
$screens | Format-Table ScreenName, Size

# Get screens matching a pattern
$screens = Get-DataverseMsAppScreen -MsAppPath "myapp.msapp" -ScreenName "Main*"

# View screen YAML content
$screen = Get-DataverseMsAppScreen -MsAppPath "myapp.msapp" -ScreenName "Screen1"
$screen.YamlContent

# Add or update a screen
$yamlContent = @"
Screens:
  NewScreen:
    Properties:
      LoadingSpinnerColor: =RGBA(56, 96, 178, 1)
      Fill: =RGBA(255, 255, 255, 1)
"@

Set-DataverseMsAppScreen -MsAppPath "myapp.msapp" -ScreenName "NewScreen" -YamlContent $yamlContent

# Add screen from a YAML file
Set-DataverseMsAppScreen -MsAppPath "myapp.msapp" -ScreenName "ImportedScreen" -YamlFilePath "C:\screens\myscreen.pa.yaml"

# Remove a screen
Remove-DataverseMsAppScreen -MsAppPath "myapp.msapp" -ScreenName "OldScreen"

# Remove with IfExists flag
Remove-DataverseMsAppScreen -MsAppPath "myapp.msapp" -ScreenName "MaybeExists" -IfExists
```

### Component Management

Components are reusable custom controls that can be used across multiple screens:

```powershell
# Get all components from a .msapp file
$components = Get-DataverseMsAppComponent -MsAppPath "myapp.msapp"
$components | Format-Table ComponentName, Size

# Get components matching a pattern
$components = Get-DataverseMsAppComponent -MsAppPath "myapp.msapp" -ComponentName "Custom*"

# Add or update a component
$componentYaml = @"
Component:
  CustomButton:
    Properties:
      Width: 100
      Height: 50
      Fill: =RGBA(0, 120, 212, 1)
      Color: =RGBA(255, 255, 255, 1)
"@

Set-DataverseMsAppComponent -MsAppPath "myapp.msapp" -ComponentName "CustomButton" -YamlContent $componentYaml

# Add component from a YAML file
Set-DataverseMsAppComponent -MsAppPath "myapp.msapp" -ComponentName "HeaderComponent" -YamlFilePath "C:\components\header.pa.yaml"

# Remove a component
Remove-DataverseMsAppComponent -MsAppPath "myapp.msapp" -ComponentName "OldComponent"

# Remove with IfExists flag
Remove-DataverseMsAppComponent -MsAppPath "myapp.msapp" -ComponentName "MaybeExists" -IfExists
```

## Complete Workflows

### Workflow 1: Create a New Canvas App from Scratch

```powershell
# 1. Create a minimal .msapp file structure
# (You can start with an existing template or create from Power Apps Studio)

# 2. Modify screens and components locally
Set-DataverseMsAppScreen -MsAppPath "newapp.msapp" -ScreenName "HomeScreen" -YamlContent $homeScreenYaml
Set-DataverseMsAppScreen -MsAppPath "newapp.msapp" -ScreenName "DetailsScreen" -YamlContent $detailsYaml
Set-DataverseMsAppComponent -MsAppPath "newapp.msapp" -ComponentName "NavBar" -YamlContent $navBarYaml

# 3. Upload to Dataverse
$appId = Set-DataverseCanvasApp -Name "new_customapp" -DisplayName "Custom App" -MsAppPath "newapp.msapp" -PassThru
Write-Host "Created Canvas app with ID: $appId"
```

### Workflow 2: Modify an Existing Canvas App

```powershell
# 1. Download existing Canvas app
$app = Get-DataverseCanvasApp -Name "existing_app" -IncludeDocument
[System.IO.File]::WriteAllBytes("existing_app.msapp", [System.Convert]::FromBase64String($app.document))

# 2. Modify locally
# Add a new screen
Set-DataverseMsAppScreen -MsAppPath "existing_app.msapp" -ScreenName "NewFeatureScreen" -YamlContent $newFeatureYaml

# Update existing screen
$existingScreen = Get-DataverseMsAppScreen -MsAppPath "existing_app.msapp" -ScreenName "HomeScreen"
$updatedYaml = $existingScreen.YamlContent -replace "RGBA\(0, 0, 0, 1\)", "RGBA(255, 255, 255, 1)"
Set-DataverseMsAppScreen -MsAppPath "existing_app.msapp" -ScreenName "HomeScreen" -YamlContent $updatedYaml

# Remove unused screen
Remove-DataverseMsAppScreen -MsAppPath "existing_app.msapp" -ScreenName "DeprecatedScreen"

# 3. Upload modified app back to Dataverse
Set-DataverseCanvasApp -Id $app.Id -MsAppPath "existing_app.msapp"
```

### Workflow 3: Batch Update Multiple Apps

```powershell
# Get all Canvas apps matching a pattern
$apps = Get-DataverseCanvasApp -Name "dept_*" -IncludeDocument

foreach ($app in $apps) {
    $filename = "$($app.Name).msapp"
    
    # Download
    [System.IO.File]::WriteAllBytes($filename, [System.Convert]::FromBase64String($app.document))
    
    # Add standardized component to each app
    Set-DataverseMsAppComponent -MsAppPath $filename -ComponentName "StandardHeader" -YamlFilePath "StandardHeader.pa.yaml"
    
    # Upload back
    Set-DataverseCanvasApp -Id $app.Id -MsAppPath $filename
    
    Write-Host "Updated $($app.DisplayName)"
}
```

## Power Apps YAML Format

The YAML files in .msapp archives follow the Power Apps YAML format. Here are some common patterns:

### Screen YAML Structure

```yaml
Screens:
  ScreenName:
    Properties:
      LoadingSpinnerColor: =RGBA(56, 96, 178, 1)
      Fill: =RGBA(255, 255, 255, 1)
      Width: =Parent.Width
      Height: =Parent.Height
    Controls:
      - Label1:
          Type: Label
          Properties:
            Text: ="Hello World"
            X: 40
            Y: 40
            Width: 200
            Height: 40
```

### Component YAML Structure

```yaml
Component:
  ComponentName:
    Properties:
      Width: 100
      Height: 50
      Fill: =RGBA(0, 120, 212, 1)
    Controls:
      - Icon1:
          Type: Icon
          Properties:
            Icon: Icon.Add
            Color: =RGBA(255, 255, 255, 1)
```

## Best Practices

### 1. Use Version Control

Store .msapp files in version control (Git) to track changes:

```powershell
# After modifications
git add myapp.msapp
git commit -m "Added new feature screen"
git push
```

### 2. Use Descriptive Names

Use consistent naming conventions for screens and components:

```powershell
# Good naming
Set-DataverseMsAppScreen -MsAppPath "app.msapp" -ScreenName "CustomerListScreen" -YamlContent $yaml
Set-DataverseMsAppComponent -MsAppPath "app.msapp" -ComponentName "CustomerCardComponent" -YamlContent $yaml

# Avoid generic names like "Screen1", "Component1"
```

### 3. Test Locally Before Uploading

Make multiple local changes and test them before uploading to Dataverse:

```powershell
# Make multiple changes
Set-DataverseMsAppScreen -MsAppPath "app.msapp" -ScreenName "Screen1" -YamlContent $yaml1
Set-DataverseMsAppScreen -MsAppPath "app.msapp" -ScreenName "Screen2" -YamlContent $yaml2
Set-DataverseMsAppComponent -MsAppPath "app.msapp" -ComponentName "Component1" -YamlContent $yaml3

# Test locally (extract and review)
$screens = Get-DataverseMsAppScreen -MsAppPath "app.msapp"
$screens | Format-Table

# Upload once when satisfied
Set-DataverseCanvasApp -Name "new_myapp" -MsAppPath "app.msapp"
```

### 4. Use WhatIf for Destructive Operations

Use `-WhatIf` to preview changes before executing:

```powershell
# Preview deletion
Remove-DataverseCanvasApp -Name "new_testapp" -WhatIf

# Preview screen removal
Remove-DataverseMsAppScreen -MsAppPath "app.msapp" -ScreenName "OldScreen" -WhatIf
```

### 5. Handle Errors Gracefully

Use `-IfExists` flag to avoid errors when resources may not exist:

```powershell
# Won't throw error if app doesn't exist
Remove-DataverseCanvasApp -Name "new_maybeexists" -IfExists

# Won't throw error if screen doesn't exist
Remove-DataverseMsAppScreen -MsAppPath "app.msapp" -ScreenName "MaybeExists" -IfExists
```

## Troubleshooting

### Issue: "Canvas app not found"

**Solution:** Verify the app name or ID:
```powershell
# List all Canvas apps
Get-DataverseCanvasApp | Format-Table Name, DisplayName, Id
```

### Issue: "Screen not found in .msapp file"

**Solution:** List available screens:
```powershell
Get-DataverseMsAppScreen -MsAppPath "app.msapp" | Format-Table ScreenName
```

### Issue: "Invalid YAML format"

**Solution:** Validate your YAML syntax. Ensure proper indentation and structure:
```powershell
# View existing screen YAML as template
$screen = Get-DataverseMsAppScreen -MsAppPath "app.msapp" -ScreenName "Screen1"
$screen.YamlContent
```

### Issue: "No default connection set"

**Solution:** Establish a default connection:
```powershell
Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive -SetAsDefault
```

## Related Cmdlets

- **Get-DataverseCanvasApp** - Retrieve Canvas apps from Dataverse
- **Set-DataverseCanvasApp** - Create or update Canvas apps (upsert)
- **Remove-DataverseCanvasApp** - Delete Canvas apps
- **Get-DataverseMsAppScreen** - Extract screens from .msapp files
- **Set-DataverseMsAppScreen** - Add/update screens in .msapp files
- **Remove-DataverseMsAppScreen** - Remove screens from .msapp files
- **Get-DataverseMsAppComponent** - Extract components from .msapp files
- **Set-DataverseMsAppComponent** - Add/update components in .msapp files
- **Remove-DataverseMsAppComponent** - Remove components from .msapp files

## See Also

- [Creating and Updating Records](creating-updating.md)
- [Solution Management](solution-management.md)
- [Connections](connections.md)
- [Error Handling](error-handling.md)
