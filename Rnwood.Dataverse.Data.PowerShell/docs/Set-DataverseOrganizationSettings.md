---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseOrganizationSettings

## SYNOPSIS
Updates organization settings in the single organization record in a Dataverse environment.

## SYNTAX

```
Set-DataverseOrganizationSettings -InputObject <PSObject> [-PassThru] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Set-DataverseOrganizationSettings cmdlet updates organization settings in the single organization record that exists in every Dataverse environment. The record is automatically discovered and the specified columns from the InputObject are updated.

To update settings within the OrgDbOrgSettings XML column, provide an OrgDbOrgSettingsUpdate property on the InputObject containing a hashtable or PSObject with the setting names and values to update.

This cmdlet supports -WhatIf and -Confirm parameters for safe operation.

## EXAMPLES

### Example 1: Update organization name
```powershell
PS C:\> $connection = Get-DataverseConnection -Url "https://contoso.crm.dynamics.com" -Interactive
PS C:\> $updates = [PSCustomObject]@{ name = "Contoso Corporation" }
PS C:\> Set-DataverseOrganizationSettings -Connection $connection -InputObject $updates -Confirm:$false
```

Updates the organization name.

### Example 2: Update OrgDbOrgSettings XML
```powershell
PS C:\> $updates = [PSCustomObject]@{
    OrgDbOrgSettingsUpdate = [PSCustomObject]@{
        MaxUploadFileSize = 10485760
        EnableBingMapsIntegration = $true
    }
}
PS C:\> Set-DataverseOrganizationSettings -Connection $connection -InputObject $updates -Confirm:$false
```

Updates specific settings within the OrgDbOrgSettings XML column.

### Example 3: Update with PassThru
```powershell
PS C:\> $updates = [PSCustomObject]@{ name = "New Name" }
PS C:\> $result = Set-DataverseOrganizationSettings -Connection $connection -InputObject $updates -PassThru -Confirm:$false
PS C:\> $result.name
New Name
```

Updates the organization name and returns the updated record.

### Example 4: Use WhatIf to preview changes
```powershell
PS C:\> $updates = [PSCustomObject]@{ name = "New Name" }
PS C:\> Set-DataverseOrganizationSettings -Connection $connection -InputObject $updates -WhatIf
What if: Performing the operation "Update organization settings" on target "Organization record ..."
```

Previews what would be updated without making actual changes.

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

### -InputObject
Object containing values to update.
Property names must match organization table column names.
To update OrgDbOrgSettings, use OrgDbOrgSettingsUpdate property with a hashtable of setting names and values.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -PassThru
If specified, returns the updated organization record

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
This cmdlet has a high confirm impact and will prompt for confirmation by default. Use -Confirm:$false to suppress the prompt.

## RELATED LINKS

[Get-DataverseOrganizationSettings](Get-DataverseOrganizationSettings.md)
[Get-DataverseConnection](Get-DataverseConnection.md)
