---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrieveChannelAccessProfilePrivileges

## SYNOPSIS
For internal use only.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveChannelAccessProfilePrivilegesRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveChannelAccessProfilePrivilegesRequest?view=dataverse-sdk-latest)

## SYNTAX

```
Invoke-DataverseRetrieveChannelAccessProfilePrivileges [-ChannelAccessProfileId <Guid>]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
For internal use only.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrieveChannelAccessProfilePrivileges -Connection <ServiceClient> -ChannelAccessProfileId <Guid>
```

## PARAMETERS

### -ChannelAccessProfileId
For internal use only.

```yaml
Type: Guid
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

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet

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

### -WhatIf
Shows what would happen if the cmdlet runs. The cmdlet is not run.

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

