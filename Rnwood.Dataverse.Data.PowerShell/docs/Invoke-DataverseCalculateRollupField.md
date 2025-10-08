---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseCalculateRollupField

## SYNOPSIS
Contains the data that is needed to calculate the value of a rollup column.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CalculateRollupFieldRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.CalculateRollupFieldRequest)

## SYNTAX

```
Invoke-DataverseCalculateRollupField -Connection <ServiceClient> -Target <PSObject> -FieldName <String>
```

## DESCRIPTION
Contains the data that is needed to calculate the value of a rollup column.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseCalculateRollupField -Connection <ServiceClient> -Target <PSObject> -FieldName <String>
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

### -Target
Gets or sets a reference to the record containing the rollup column to calculate. Required. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True
Accept wildcard characters: False
```

### -FieldName
Gets or sets the logical name of the column to calculate. Required.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.CalculateRollupFieldResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CalculateRollupFieldResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.CalculateRollupFieldResponse)
## NOTES

## RELATED LINKS
