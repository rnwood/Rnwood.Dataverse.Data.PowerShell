# Table Icons

This guide explains how to set table vector icons in Dataverse using the icon set cmdlets.

## Overview

Modern Dataverse UI uses SVG vector icons for tables, providing scalable, resolution-independent display. The module provides cmdlets to easily download and set table icons from popular online icon sets without manually creating web resources.

## Supported Icon Sets

The module supports three major icon sets:

- **FluentUI System Icons** (default): Microsoft's comprehensive icon library with 2000+ icons
  - Source: https://github.com/microsoft/fluentui-system-icons
  - Modern, professional design aligned with Microsoft 365
  
- **Tabler Icons**: Open-source icon library with 5000+ icons
  - Source: https://github.com/tabler/tabler-icons
  - Clean, consistent outline style
  
- **Iconoir**: Modern, open-source SVG icon library with 1000+ icons
  - Source: https://iconoir.com
  - Minimalist design with comprehensive coverage

## Quick Start

### Browse Available Icons

Use `Get-DataverseIconSetIcon` to browse and search for icons:

```powershell
# List all icons from the default FluentUI icon set
Get-DataverseIconSetIcon

# Search for person-related icons
Get-DataverseIconSetIcon -Name "person*"

# Browse icons from Tabler icon set
Get-DataverseIconSetIcon -IconSet Tabler -Name "*user*"

# Interactive selection with Out-GridView
$icon = Get-DataverseIconSetIcon -Name "*settings*" | Out-GridView -OutputMode Single
```

### Set a Table Icon

Use `Set-DataverseTableIconFromSet` to download an icon and set it on a table:

```powershell
# Set icon from FluentUI (default) with publisher prefix
Set-DataverseTableIconFromSet -EntityName "contact" -IconName "person" -PublisherPrefix "contoso" -Publish

# Set icon from Tabler icon set
Set-DataverseTableIconFromSet -EntityName "account" -IconName "building" -IconSet Tabler -PublisherPrefix "contoso" -Publish

# Set icon from Iconoir
Set-DataverseTableIconFromSet -EntityName "new_project" -IconName "user" -IconSet Iconoir -PublisherPrefix "contoso" -Publish
```

## How It Works

The `Set-DataverseTableIconFromSet` cmdlet automates the entire process:

1. **Downloads** the SVG icon from the online icon set
2. **Creates or updates** a web resource with the icon content
   - Web resource naming pattern: `{prefix}_/icons/{iconset}/{iconname}.svg`
   - Requires you to specify the publisher prefix with `-PublisherPrefix`
3. **Updates** the table's `IconVectorName` metadata property
4. **Publishes** changes if the `-Publish` switch is specified

## Advanced Usage

### Publisher Prefix

The `-PublisherPrefix` parameter is required and specifies the prefix for the web resource:

```powershell
Set-DataverseTableIconFromSet -EntityName "new_customtable" `
    -IconName "settings" `
    -IconSet "FluentUI" `
    -PublisherPrefix "contoso" `
    -Publish
```

This creates a web resource named `contoso_/icons/FluentUI/settings.svg`.

### Preview Changes

Use `-WhatIf` to see what changes would be made:

```powershell
Set-DataverseTableIconFromSet -EntityName "contact" -IconName "user" -PublisherPrefix "contoso" -WhatIf
```

### Return Updated Metadata

Use `-PassThru` to return the updated entity metadata:

```powershell
$result = Set-DataverseTableIconFromSet -EntityName "account" `
    -IconName "building" `
    -PublisherPrefix "contoso" `
    -PassThru
$result.IconVectorName
```

### Set Icons for Multiple Tables

```powershell
@("contact", "account", "lead") | ForEach-Object {
    Set-DataverseTableIconFromSet -EntityName $_ -IconName "person" -PublisherPrefix "contoso" -Publish
}
```

## Icon Set Details

### FluentUI System Icons

- **Default icon set** - Used when no `-IconSet` parameter is specified
- **2000+ icons** covering common UI needs
- **Naming**: Lowercase with hyphens (e.g., "person", "building", "mail")
- **Style**: Modern Microsoft design language
- Uses 24px regular variant optimized for table icons

### Tabler Icons

- **5000+ icons** - Largest collection
- **Naming**: Lowercase with hyphens (e.g., "user", "mail", "settings")
- **Style**: Clean outline design
- Consistent 2px stroke width across all icons

### Iconoir

- **1000+ icons** with minimalist design
- **Naming**: Lowercase with hyphens (e.g., "user", "settings")
- **Style**: Simple, clean lines
- Lightweight SVG files

## Integration with Entity Metadata

These cmdlets work seamlessly with the existing metadata cmdlets:

```powershell
# Create entity with icon in one step
Set-DataverseEntityMetadata -EntityName new_product `
    -SchemaName new_Product `
    -DisplayName "Product" `
    -DisplayCollectionName "Products" `
    -OwnershipType UserOwned `
    -PrimaryAttributeSchemaName new_name `
    -PrimaryAttributeDisplayName "Product Name"

# Then set the icon
Set-DataverseTableIconFromSet -EntityName new_product `
    -IconName "shopping-cart" `
    -IconSet Tabler `
    -PublisherPrefix "new" `
    -Publish
```

Or manually set the icon using `Set-DataverseEntityMetadata`:

```powershell
# After using Set-DataverseTableIconFromSet, the web resource name follows the pattern:
# {prefix}_/icons/{iconset}/{iconname}.svg

Set-DataverseEntityMetadata -EntityName new_product `
    -IconVectorName "new_/icons/Tabler/shopping-cart.svg"
```

## Troubleshooting

### Icon Not Found

If you get an error that an icon wasn't found:

```powershell
# Use Get-DataverseIconSetIcon to verify the icon name exists
Get-DataverseIconSetIcon -Name "*search-term*"
```

Icon names are case-sensitive and must exactly match the icon set's naming convention.

### Internet Access Required

Both cmdlets require internet access to:
- Query the GitHub API for icon lists
- Download icon files from GitHub raw content

If you're behind a firewall, ensure access to:
- `api.github.com`
- `raw.githubusercontent.com`

### Publisher Prefix

You must always specify the publisher prefix with the `-PublisherPrefix` parameter. This determines the naming of the web resource created for the icon. Use your organization's customization prefix (e.g., "contoso", "new", etc.).

## Related Cmdlets

- `Get-DataverseIconSetIcon` - Browse and search available icons
- `Set-DataverseTableIconFromSet` - Download and set table icon
- `Set-DataverseEntityMetadata` - Create/update entity metadata (including IconVectorName)
- `Get-DataverseEntityMetadata` - Retrieve entity metadata
- `Set-DataverseWebResource` - Manually create/update web resources

## See Also

- [Metadata Concepts](metadata.md)
- [Web Resources](web-resources.md)
- [FluentUI System Icons](https://github.com/microsoft/fluentui-system-icons)
- [Tabler Icons](https://github.com/tabler/tabler-icons)
- [Iconoir Icons](https://iconoir.com)
