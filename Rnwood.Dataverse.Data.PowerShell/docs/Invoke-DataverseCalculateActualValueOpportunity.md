---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseCalculateActualValueOpportunity

## SYNOPSIS
Contains the data that is needed to calculate the value of an opportunity that is in the "Won" state.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CalculateActualValueOpportunityRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.CalculateActualValueOpportunityRequest)

## SYNTAX

```
Invoke-DataverseCalculateActualValueOpportunity -Connection <ServiceClient> -OpportunityId <Guid>
```

## DESCRIPTION
Contains the data that is needed to calculate the value of an opportunity that is in the "Won" state.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseCalculateActualValueOpportunity -Connection <ServiceClient> -OpportunityId <Guid>
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

### -OpportunityId
Gets or sets the ID of the opportunity. Required.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.CalculateActualValueOpportunityResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CalculateActualValueOpportunityResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.CalculateActualValueOpportunityResponse)
## NOTES

## RELATED LINKS
