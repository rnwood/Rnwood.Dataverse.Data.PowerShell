---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Wait-DataversePublish

## SYNOPSIS
Waits for a Dataverse publish operation to complete.

## SYNTAX

```
Wait-DataversePublish [-MaxWaitSeconds <Int32>] [-PollIntervalSeconds <Int32>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Waits for an in-progress publish operation to complete before continuing. This is useful after
making metadata changes or publishing customizations to ensure they are fully applied before
proceeding with subsequent operations.

The cmdlet polls the Dataverse environment at regular intervals to check the publish status.
If the publish operation does not complete within the maximum wait time, an error is thrown.

## EXAMPLES

### Example 1: Wait for publish to complete
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Publish-DataverseCustomizations
PS C:\> Wait-DataversePublish
```

Publishes customizations and waits for the publish operation to complete using default settings (5 minute timeout, 2 second poll interval).

### Example 2: Wait with custom timeout
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Publish-DataverseCustomizations
PS C:\> Wait-DataversePublish -MaxWaitSeconds 600 -PollIntervalSeconds 5
```

Waits up to 10 minutes for the publish to complete, checking every 5 seconds.

### Example 3: Wait after metadata changes
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAttributeMetadata -EntityName contact -Attribute @{ schemaname = "new_customfield"; displayname = "Custom Field"; attributetype = "String" }
PS C:\> Publish-DataverseCustomizations
PS C:\> Wait-DataversePublish
PS C:\> # Metadata is now published and available
```

Creates a new attribute, publishes it, and waits for publish to complete before continuing.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet.
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

### -MaxWaitSeconds
Maximum time to wait in seconds.
Default is 300 seconds (5 minutes).

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PollIntervalSeconds
Interval between polls in seconds.
Default is 2 seconds.

```yaml
Type: Int32
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
