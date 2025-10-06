---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseExecuteMultiple

## SYNOPSIS
Contains the data that is needed to execute one or more message requests as a single batch operation, and optionally return a collection of results.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.ExecuteMultipleRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.ExecuteMultipleRequest)

## SYNTAX

```
Invoke-DataverseExecuteMultiple [-Requests <OrganizationRequestCollection>]
 [-Settings <ExecuteMultipleSettings>] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Contains the data that is needed to execute one or more message requests as a single batch operation, and optionally return a collection of results.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseExecuteMultiple -Connection <ServiceClient> -Requests <OrganizationRequestCollection> -Settings <ExecuteMultipleSettings>
```

## PARAMETERS

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: False
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

### -Requests
Gets or sets the collection of requests to execute.

```yaml
Type: OrganizationRequestCollection
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Settings
Gets or sets the settings that define whether execution should continue if an error occurs and if responses for each message request processed are to be returned.

```yaml
Type: ExecuteMultipleSettings
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
Default value: False
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
