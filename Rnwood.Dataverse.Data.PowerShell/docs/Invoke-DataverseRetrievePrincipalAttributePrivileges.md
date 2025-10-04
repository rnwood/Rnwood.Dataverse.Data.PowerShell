---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrievePrincipalAttributePrivileges

## SYNOPSIS
Contains the data that is needed to retrieves all the secured attribute privileges a user or team has through direct or indirect (through team membership) associations with the entity.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrievePrincipalAttributePrivilegesRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrievePrincipalAttributePrivilegesRequest)

## SYNTAX

```
Invoke-DataverseRetrievePrincipalAttributePrivileges -Connection <ServiceClient> [-Principal <PSObject>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Contains the data that is needed to retrieves all the secured attribute privileges a user or team has through direct or indirect (through team membership) associations with the entity.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrievePrincipalAttributePrivileges -Connection <ServiceClient> -Principal <PSObject>
```

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet

```yaml
Type: ServiceClient
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Principal
Gets or sets the principal to add to the queue. Required. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

```yaml
Type: PSObject
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
Default value: False
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
