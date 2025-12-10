---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseWebResource

## SYNOPSIS
{{ Fill in the Synopsis }}

## SYNTAX

### InputObject
```
Set-DataverseWebResource -InputObject <PSObject> [-Id <Guid>] [-Name <String>] [-Publish] [-PassThru]
 [-NoUpdate] [-NoCreate] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf]
 [-Confirm] [<CommonParameters>]
```

### File
```
Set-DataverseWebResource [-Id <Guid>] -Name <String> -Path <String> [-DisplayName <String>]
 [-Description <String>] [-WebResourceType <WebResourceType>] [-Publish] [-PassThru] [-NoUpdate] [-NoCreate]
 [-PublisherPrefix <String>] [-IfNewer] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [-WhatIf] [-Confirm] [<CommonParameters>]
```

### Folder
```
Set-DataverseWebResource -Folder <String> [-DisplayName <String>] [-Publish] [-PassThru] [-NoUpdate]
 [-NoCreate] [-FileFilter <String>] [-PublisherPrefix <String>] [-IfNewer] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
{{ Fill in the Description }}

## EXAMPLES

### Example 1
```powershell
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

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
Description for the web resource

```yaml
Type: String
Parameter Sets: File
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DisplayName
Display name for the web resource

```yaml
Type: String
Parameter Sets: File, Folder
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FileFilter
File filter pattern for folder operations (e.g., '*.js', '*.html').
Default is '*.*'

```yaml
Type: String
Parameter Sets: Folder
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Folder
Folder path containing multiple web resource files.
File names are used as web resource names.

```yaml
Type: String
Parameter Sets: Folder
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Id
ID of the web resource to update.
If not specified, creates a new web resource or matches by name.

```yaml
Type: Guid
Parameter Sets: InputObject, File
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IfNewer
If set, only updates if the file modification time is newer than the web resource modified time

```yaml
Type: SwitchParameter
Parameter Sets: File, Folder
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject
Object containing web resource properties (name, displayname, webresourcetype, content, etc.)

```yaml
Type: PSObject
Parameter Sets: InputObject
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Name
Name of the web resource.
Used to match existing resources if Id is not provided.

```yaml
Type: String
Parameter Sets: InputObject
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

```yaml
Type: String
Parameter Sets: File
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoCreate
If set, skips creating new web resources (only updates existing ones)

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

### -NoUpdate
If set, skips updating existing web resources (only creates new ones)

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

### -PassThru
If set, returns the created/updated web resource object

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

### -Path
File path containing the web resource content

```yaml
Type: String
Parameter Sets: File
Aliases:

Required: True
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

### -Publish
If set, publishes the web resource after creation/update

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

### -PublisherPrefix
Publisher prefix for new web resources (e.g., 'new_').
Required for new web resources if Name doesn't contain a prefix.

```yaml
Type: String
Parameter Sets: File, Folder
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WebResourceType
Web resource type: 1=HTML, 2=CSS, 3=JS, 4=XML, 5=PNG, 6=JPG, 7=GIF, 8=XAP, 9=XSL, 10=ICO, 11=SVG, 12=RESX.
Auto-detected from file extension if not specified.

```yaml
Type: WebResourceType
Parameter Sets: File
Aliases:
Accepted values: HTML, CSS, JavaScript, XML, PNG, JPG, GIF, XAP, XSL, ICO, SVG, RESX

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

### System.Management.Automation.PSObject
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES

## RELATED LINKS
