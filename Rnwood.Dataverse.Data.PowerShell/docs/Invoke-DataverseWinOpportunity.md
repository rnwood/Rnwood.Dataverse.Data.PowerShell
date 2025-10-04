---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseWinOpportunity

## SYNOPSIS
Contains the data that is needed to set the state of an opportunity to Won.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.WinOpportunityRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.WinOpportunityRequest)

## SYNTAX

```
Invoke-DataverseWinOpportunity -Connection <ServiceClient> [-OpportunityClose <PSObject>]
 [-OpportunityCloseTableName <String>] [-OpportunityCloseIgnoreProperties <String[]>]
 [-OpportunityCloseLookupColumns <Hashtable>] [-Status <OptionSetValue>] [-ProgressAction <ActionPreference>]
 [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Contains the data that is needed to set the state of an opportunity to Won.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseWinOpportunity -Connection <ServiceClient> -OpportunityClose <PSObject> -OpportunityCloseTableName <String> -OpportunityCloseIgnoreProperties <String[]> -OpportunityCloseLookupColumns <Hashtable> -Status <OptionSetValue>
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

### -OpportunityClose
Gets or sets the opportunity close activity associated with this state change. Required. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type.

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

### -OpportunityCloseIgnoreProperties
Gets or sets the opportunity close activity associated with this state change. Required. Properties to ignore when converting OpportunityClose PSObject to Entity.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OpportunityCloseLookupColumns
Gets or sets the opportunity close activity associated with this state change. Required. Hashtable specifying lookup columns for entity reference conversions in OpportunityClose.

```yaml
Type: Hashtable
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OpportunityCloseTableName
Gets or sets the opportunity close activity associated with this state change. Required. The logical name of the table/entity type for the OpportunityClose parameter.

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

### -Status
Gets or sets a new status of the opportunity. Required.

```yaml
Type: OptionSetValue
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
