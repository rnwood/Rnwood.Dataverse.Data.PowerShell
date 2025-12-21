---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseCanvasApp

## SYNOPSIS
Creates or updates a Canvas app in a Dataverse environment.

## SYNTAX

```
Set-DataverseCanvasApp [[-Id] <Guid>] [[-Name] <String>] [-DisplayName <String>] [-Description <String>]
 -MsAppPath <String> [-PublisherPrefix <String>] [-PassThru] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Creates a new Canvas app or updates an existing one by generating a solution file and importing it. For creation, a minimal .msapp file is generated unless you provide one via -MsAppPath. For updates, the existing app is exported, modified, and re-imported.

## EXAMPLES

### Example 1: Create a new Canvas app with minimal properties
```powershell
PS C:\> Set-DataverseCanvasApp -Name "new_myapp" -DisplayName "My App" -PublisherPrefix "new" -PassThru
```

Creates a new Canvas app with the specified name and display name.

### Example 2: Create a Canvas app with a custom .msapp file
```powershell
PS C:\> Set-DataverseCanvasApp -Name "new_customapp" -DisplayName "Custom App" -PublisherPrefix "new" -MsAppPath "C:\apps\myapp.msapp"
```

Creates a new Canvas app using a specific .msapp file as the document content.

### Example 3: Update an existing Canvas app's display name
```powershell
PS C:\> Set-DataverseCanvasApp -Id "12345678-1234-1234-1234-123456789012" -DisplayName "Updated App Name"
```

Updates the display name of an existing Canvas app.

### Example 4: Update a Canvas app's .msapp file
```powershell
PS C:\> Set-DataverseCanvasApp -Id "12345678-1234-1234-1234-123456789012" -MsAppPath "C:\apps\updated.msapp"
```

Updates the Canvas app's document content with a new .msapp file.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet, or string specifying Dataverse organization URL (e.g.
http://server.com/MyOrg/).
If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

```yaml
Type: ServiceClient
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Description
Description for the Canvas app

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DisplayName
Display name for the Canvas app

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Id
ID of the Canvas app to update

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -MsAppPath
Path to an .msapp file to use as the document content

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name
Name for the Canvas app (logical name)

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
If set, returns the Canvas app ID

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
{{ Fill ProgressAction Description }}

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PublisherPrefix
Publisher prefix for the new Canvas app (e.g., 'new')

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WhatIf
Shows what would happen if the cmdlet runs.
The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Nullable`1[[System.Guid, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
## OUTPUTS

### System.Guid
## NOTES

## RELATED LINKS
