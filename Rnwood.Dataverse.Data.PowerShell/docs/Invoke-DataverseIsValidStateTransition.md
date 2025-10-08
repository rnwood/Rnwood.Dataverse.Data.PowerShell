---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseIsValidStateTransition

## SYNOPSIS
Contains the data that is needed to validate the state transition.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.IsValidStateTransitionRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.IsValidStateTransitionRequest)

## SYNTAX

```
Invoke-DataverseIsValidStateTransition -Connection <ServiceClient> -Entity <PSObject> -NewState <String> -NewStatus <Int32>
```

## DESCRIPTION
Contains the data that is needed to validate the state transition.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseIsValidStateTransition -Connection <ServiceClient> -Entity <PSObject> -NewState <String> -NewStatus <Int32>
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

### -Entity
Gets or sets the entity reference for the record whose transition state is validated. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

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

### -NewState
Gets or sets the proposed new state for the record.

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

### -NewStatus
Gets or sets the proposed new status for the record.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.IsValidStateTransitionResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.IsValidStateTransitionResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.IsValidStateTransitionResponse)
## NOTES

## RELATED LINKS
