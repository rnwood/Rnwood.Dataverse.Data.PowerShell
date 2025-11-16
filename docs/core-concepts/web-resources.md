# Managing Web Resources

Web resources in Dataverse allow you to store and serve web files like JavaScript, CSS, HTML, images, and other content. This guide covers the cmdlets for managing web resources with file system integration.

## Table of Contents

- [Overview](#overview)
- [Web Resource Types](#web-resource-types)
- [Retrieving Web Resources](#retrieving-web-resources)
- [Creating and Updating Web Resources](#creating-and-updating-web-resources)
- [Deleting Web Resources](#deleting-web-resources)
- [Batch Operations](#batch-operations)
- [Best Practices](#best-practices)

## Overview

The web resource cmdlets provide:
- **File system integration** - Upload from and download to files/folders
- **Auto-type detection** - Automatically detect web resource type from file extension
- **Wildcard filtering** - Use wildcards in name and display name filters
- **Conditional updates** - Only update when files are newer
- **Batch operations** - Process multiple files in a folder
- **Pipeline support** - Chain operations together

## Web Resource Types

Web resources are classified by type using the `WebResourceType` enum:

| Type | Value | Extension | Description |
|------|-------|-----------|-------------|
| HTML | 1 | .htm, .html | HTML web pages |
| CSS | 2 | .css | Cascading Style Sheets |
| JavaScript | 3 | .js | JavaScript code |
| XML | 4 | .xml | XML data files |
| PNG | 5 | .png | PNG images |
| JPG | 6 | .jpg, .jpeg | JPEG images |
| GIF | 7 | .gif | GIF images |
| XAP | 8 | .xap | Silverlight applications |
| XSL | 9 | .xsl, .xslt | XSL stylesheets |
| ICO | 10 | .ico | Icon files |
| SVG | 11 | .svg | SVG images |
| RESX | 12 | .resx | Resource files |

## Retrieving Web Resources

### Get by Name

```powershell
# Get a specific web resource
Get-DataverseWebResource -Connection $conn -Name "new_myscript"

# Use wildcards
Get-DataverseWebResource -Connection $conn -Name "new_*"
```

### Get by ID

```powershell
$webResourceId = "d5e8a4b2-1234-5678-90ab-cdef12345678"
Get-DataverseWebResource -Connection $conn -Id $webResourceId
```

### Filter by Type

```powershell
# Get all JavaScript files
Get-DataverseWebResource -Connection $conn -WebResourceType JavaScript

# Get all CSS files with wildcard
Get-DataverseWebResource -Connection $conn -Name "prefix_*" -WebResourceType CSS
```

### Filter by Display Name

```powershell
# Exact match
Get-DataverseWebResource -Connection $conn -DisplayName "My Script"

# Wildcard match
Get-DataverseWebResource -Connection $conn -DisplayName "My *"
```

### Filter Unmanaged Only

```powershell
# Get only unmanaged web resources
Get-DataverseWebResource -Connection $conn -Unmanaged
```

### Download to File

```powershell
# Download single web resource
Get-DataverseWebResource -Connection $conn -Name "new_myscript" -Path "./local-script.js"

# Download by ID
Get-DataverseWebResource -Connection $conn -Id $webResourceId -Path "./downloaded.js"
```

### Download to Folder

```powershell
# Download all JavaScript files to a folder
Get-DataverseWebResource -Connection $conn -WebResourceType JavaScript -Folder "./js-files"

# Download with wildcard filter
Get-DataverseWebResource -Connection $conn -Name "new_*" -Folder "./webresources"
```

### Decode Content

```powershell
# Get content as byte array instead of base64 string
$webResource = Get-DataverseWebResource -Connection $conn -Name "new_myscript" -DecodeContent
$contentBytes = $webResource.content  # byte[]
```

## Creating and Updating Web Resources

### Create from File

```powershell
# Create with auto-detected type
Set-DataverseWebResource -Connection $conn -Name "new_script" -Path "./app.js" -PublisherPrefix "new"

# Specify type explicitly
Set-DataverseWebResource -Connection $conn -Name "new_styles" -Path "./main.css" `
    -WebResourceType CSS -DisplayName "Main Styles"
```

### Update Existing

```powershell
# Update by name
Set-DataverseWebResource -Connection $conn -Name "new_script" -Path "./app.js"

# Update by ID
Set-DataverseWebResource -Connection $conn -Id $webResourceId -Path "./app.js"
```

### Conditional Update with IfNewer

```powershell
# Only update if file is newer than web resource
Set-DataverseWebResource -Connection $conn -Name "new_script" -Path "./app.js" -IfNewer
```

### Create from InputObject

```powershell
# Create with custom properties
$webResource = @{
    name = "new_customscript"
    displayname = "Custom Script"
    webresourcetype = [Rnwood.Dataverse.Data.PowerShell.Commands.WebResourceType]::JavaScript
    content = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes("console.log('test');"))
}
Set-DataverseWebResource -Connection $conn -InputObject $webResource
```

### Publish After Create/Update

```powershell
# Automatically publish the web resource
Set-DataverseWebResource -Connection $conn -Name "new_script" -Path "./app.js" -Publish
```

### Control Create/Update Behavior

```powershell
# Only create, skip if exists
Set-DataverseWebResource -Connection $conn -Name "new_script" -Path "./app.js" -NoUpdate

# Only update, skip if doesn't exist
Set-DataverseWebResource -Connection $conn -Name "new_script" -Path "./app.js" -NoCreate
```

### Get Result with PassThru

```powershell
# Return the created/updated web resource
$result = Set-DataverseWebResource -Connection $conn -Name "new_script" -Path "./app.js" -PassThru
Write-Host "Web resource ID: $($result.Id)"
```

## Deleting Web Resources

### Delete by Name

```powershell
Remove-DataverseWebResource -Connection $conn -Name "new_script"
```

### Delete by ID

```powershell
Remove-DataverseWebResource -Connection $conn -Id $webResourceId
```

### Silent Deletion

```powershell
# Don't throw error if doesn't exist
Remove-DataverseWebResource -Connection $conn -Name "new_script" -IfExists
```

### Delete from Pipeline

```powershell
# Delete multiple web resources
Get-DataverseWebResource -Connection $conn -Name "old_*" | 
    Remove-DataverseWebResource -Connection $conn
```

### Confirmation Control

```powershell
# Skip confirmation prompt
Remove-DataverseWebResource -Connection $conn -Name "new_script" -Confirm:$false

# Test what would be deleted (WhatIf)
Remove-DataverseWebResource -Connection $conn -Name "new_script" -WhatIf
```

## Batch Operations

### Upload Folder

```powershell
# Upload all files from folder
Set-DataverseWebResource -Connection $conn -Folder "./webresources" -PublisherPrefix "new"

# Upload with file filter
Set-DataverseWebResource -Connection $conn -Folder "./webresources" -FileFilter "*.js" -PublisherPrefix "new"

# Upload with IfNewer check
Set-DataverseWebResource -Connection $conn -Folder "./webresources" -IfNewer -PublisherPrefix "new"

# Upload and publish
Set-DataverseWebResource -Connection $conn -Folder "./webresources" -PublisherPrefix "new" -Publish
```

### Download Folder

```powershell
# Download all JavaScript files
Get-DataverseWebResource -Connection $conn -WebResourceType JavaScript -Folder "./downloaded"

# Download with name filter
Get-DataverseWebResource -Connection $conn -Name "new_*" -Folder "./downloaded"

# Download unmanaged only
Get-DataverseWebResource -Connection $conn -Unmanaged -Folder "./downloaded"
```

### Batch Delete

```powershell
# Delete all matching web resources
Get-DataverseWebResource -Connection $conn -Name "temp_*" | 
    Remove-DataverseWebResource -Connection $conn -Confirm:$false
```

## Best Practices

### Naming Conventions

1. **Use publisher prefix**: Always include your publisher prefix (e.g., `new_scriptname`)
2. **Descriptive names**: Use clear, descriptive names that indicate purpose
3. **Avoid spaces**: Use underscores or camelCase instead of spaces
4. **Include type hint**: Consider including file type in name (e.g., `new_loginScript`)

```powershell
# Good
Set-DataverseWebResource -Name "contoso_loginValidation" -Path "./login.js" -PublisherPrefix "contoso"

# Avoid
Set-DataverseWebResource -Name "script1" -Path "./login.js" -PublisherPrefix "new"
```

### Version Control Integration

Use `-IfNewer` for syncing from source control:

```powershell
# Only upload files that changed
Set-DataverseWebResource -Connection $conn -Folder "./src/webresources" -IfNewer -PublisherPrefix "contoso"
```

### Development Workflow

```powershell
# 1. Download current web resources for backup
Get-DataverseWebResource -Connection $conn -Name "contoso_*" -Folder "./backup"

# 2. Make changes to local files
# ... edit files ...

# 3. Upload changed files only
Set-DataverseWebResource -Connection $conn -Folder "./src" -IfNewer -PublisherPrefix "contoso"

# 4. Publish all at once (for production)
Publish-DataverseCustomizations -Connection $conn
```

### Error Handling

```powershell
try {
    Set-DataverseWebResource -Connection $conn -Name "contoso_script" -Path "./app.js" -PublisherPrefix "contoso"
} catch {
    Write-Error "Failed to upload web resource: $_"
    # Handle error (retry, log, etc.)
}
```

### Using with Solution Management

```powershell
# Export solution with web resources
Export-DataverseSolution -Connection $conn -SolutionName "MySolution" -Path "./solution.zip"

# Import solution (includes web resources)
Import-DataverseSolution -Connection $conn -Path "./solution.zip"
```

### Testing

```powershell
# Test upload with WhatIf
Set-DataverseWebResource -Connection $conn -Folder "./webresources" -PublisherPrefix "contoso" -WhatIf

# Verify upload
$uploaded = Get-DataverseWebResource -Connection $conn -Name "contoso_script"
if ($uploaded) {
    Write-Host "Upload successful: $($uploaded.displayname)"
}
```

## See Also

- [Get-DataverseWebResource](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseWebResource.md)
- [Set-DataverseWebResource](../../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseWebResource.md)
- [Remove-DataverseWebResource](../../Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseWebResource.md)
- [Publish-DataverseCustomizations](../../Rnwood.Dataverse.Data.PowerShell/docs/Publish-DataverseCustomizations.md)
- [Solution Management](../advanced/solution-management.md)
