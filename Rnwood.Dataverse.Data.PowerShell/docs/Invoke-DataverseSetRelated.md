---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseSetRelated

## SYNOPSIS
Contains the data needed to create a relationship between a set of records that participate in specific relationships.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SetRelatedRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.SetRelatedRequest)

## SYNTAX

```
Invoke-DataverseSetRelated -Connection <ServiceClient> -Target <EntityReference[]>
```

## DESCRIPTION
Contains the data needed to create a relationship between a set of records that participate in specific relationships.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseSetRelated -Connection <ServiceClient> -Target <EntityReference[]>
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
Gets or sets the target records of the set related action. Required.

```yaml
Type: EntityReference[]
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.SetRelatedResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SetRelatedResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.SetRelatedResponse)
## NOTES

## RELATED LINKS
