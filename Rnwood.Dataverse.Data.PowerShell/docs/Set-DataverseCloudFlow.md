---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseCloudFlow

## SYNOPSIS
Updates properties of a cloud flow in Dataverse or changes its state.

## SYNTAX

### ById
```
Set-DataverseCloudFlow [-Id] <Guid> [-NewName <String>] [-Description <String>] [-Activate] [-Deactivate]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### ByName
```
Set-DataverseCloudFlow [-Name] <String> [-NewName <String>] [-Description <String>] [-Activate] [-Deactivate]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Updates a cloud flow's properties (name, description) or changes its state (activate/deactivate). Supports WhatIf and Confirm for safe execution.

## EXAMPLES

### Example 1 - Rename a flow
```powershell
PS C:\> Set-DataverseCloudFlow -Name "My Flow" -NewName "My Renamed Flow"
```

Renames the cloud flow from "My Flow" to "My Renamed Flow".

### Example 2 - Activate a flow
```powershell
PS C:\> Set-DataverseCloudFlow -Id "00000000-0000-0000-0000-000000000000" -Activate
```

Activates (turns on) the cloud flow with the specified ID.

### Example 3 - Deactivate a flow
```powershell
PS C:\> Set-DataverseCloudFlow -Name "My Flow" -Deactivate
```

Deactivates (turns off) the cloud flow named "My Flow".

### Example 4 - Update description
```powershell
PS C:\> Set-DataverseCloudFlow -Name "My Flow" -Description "Updated description"
```

Updates the description of the cloud flow.

### Example 5 - Use WhatIf to preview changes
```powershell
PS C:\> Set-DataverseCloudFlow -Name "My Flow" -Activate -WhatIf
```

Shows what would happen if the flow were activated without actually activating it.

## PARAMETERS

### -Activate
Activate the cloud flow.

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

### -Deactivate
Deactivate the cloud flow (set to draft).

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

### -Description
The new description for the cloud flow.

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
The ID of the cloud flow to update.

```yaml
Type: Guid
Parameter Sets: ById
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Name
The name of the cloud flow to update.

```yaml
Type: String
Parameter Sets: ByName
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NewName
The new display name for the cloud flow.

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

### System.Guid
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
