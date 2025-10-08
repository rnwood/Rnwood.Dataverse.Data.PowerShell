---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseInitializeFrom

## SYNOPSIS
Contains the data that is needed to initialize a new record from an existing record.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.InitializeFromRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.InitializeFromRequest)

## SYNTAX

```
Invoke-DataverseInitializeFrom -Connection <ServiceClient> -EntityMoniker <PSObject> -TargetEntityName <String> -TargetFieldType <TargetFieldType> -SkipParentalRelationshipMapping <Boolean>
```

## DESCRIPTION
Contains the data that is needed to initialize a new record from an existing record.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseInitializeFrom -Connection <ServiceClient> -EntityMoniker <PSObject> -TargetEntityName <String> -TargetFieldType <TargetFieldType> -SkipParentalRelationshipMapping <Boolean>
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

### -EntityMoniker
Gets or sets the record that is the source for initializing. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

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

### -TargetEntityName
Gets or sets the logical name of the target entity.

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

### -TargetFieldType
Gets or sets which attributes are to be initialized in the initialized instance.

```yaml
Type: TargetFieldType
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SkipParentalRelationshipMapping
Gets or sets the SkipParentalRelationshipMapping for the request.

```yaml
Type: Boolean
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

### Microsoft.Crm.Sdk.Messages.InitializeFromResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.InitializeFromResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.InitializeFromResponse)
## NOTES

## RELATED LINKS
