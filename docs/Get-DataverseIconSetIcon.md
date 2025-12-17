# Get-DataverseIconSetIcon

## SYNOPSIS
Retrieves available icons from supported online icon sets.

## SYNTAX

```powershell
Get-DataverseIconSetIcon [[-IconSet] <String>] [[-Name] <String>] [<CommonParameters>]
```

## DESCRIPTION
The `Get-DataverseIconSetIcon` cmdlet retrieves a list of available icons from supported online icon sets. It can list all icons or filter them by name using wildcards. This cmdlet is useful for browsing available icons before setting them on tables using `Set-DataverseTableIcon`.

## PARAMETERS

### -IconSet
Icon set to retrieve icons from. Currently supported icon sets:
- **FluentUI**: Microsoft's Fluent UI System Icons - comprehensive icon library with 2000+ icons from https://github.com/microsoft/fluentui-system-icons
- **Iconoir**: Modern, open-source SVG icon library from https://iconoir.com

Default value: `FluentUI`

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: FluentUI
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name
Filter pattern to match icon names. Supports wildcards like `user*` or `*settings`. If not specified, all icons are returned.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: True
```

## INPUTS

### None
This cmdlet does not accept pipeline input.

## OUTPUTS

### System.Management.Automation.PSObject
Returns PSObject with the following properties:
- **IconSet**: Name of the icon set (e.g., "Iconoir")
- **Name**: Icon name (e.g., "user", "settings")
- **FileName**: Full filename including extension (e.g., "user.svg")
- **DownloadUrl**: Direct download URL for the icon
- **Size**: File size in bytes

## NOTES
This cmdlet requires internet access to retrieve the icon list from the online repository. The icon list is retrieved fresh each time the cmdlet is run.

## EXAMPLES

### Example 1: List all available icons
```powershell
Get-DataverseIconSetIcon
```

Lists all available icons from the default FluentUI icon set.

### Example 1b: List icons from a specific icon set
```powershell
Get-DataverseIconSetIcon -IconSet Iconoir
```

Lists all available icons from the Iconoir icon set.

### Example 2: Search for user-related icons
```powershell
Get-DataverseIconSetIcon -Name "user*"
```

Lists all icons whose names start with "user".

### Example 3: Search for settings icons
```powershell
Get-DataverseIconSetIcon -Name "*setting*"
```

Lists all icons whose names contain "setting".

### Example 4: Get a specific icon
```powershell
Get-DataverseIconSetIcon -Name "user"
```

Gets information about the "user" icon.

### Example 5: Count available icons
```powershell
(Get-DataverseIconSetIcon).Count
```

Returns the total number of available icons.

### Example 6: Browse and select an icon interactively
```powershell
$icon = Get-DataverseIconSetIcon -Name "user*" | Out-GridView -OutputMode Single
```

Displays a filterable grid of user-related icons and allows selection of one.

## RELATED LINKS

- [Set-DataverseTableIcon](Set-DataverseTableIcon.md)
- [Set-DataverseEntityMetadata](Set-DataverseEntityMetadata.md)
- [FluentUI System Icons](https://github.com/microsoft/fluentui-system-icons)
- [Iconoir Icons](https://iconoir.com)
