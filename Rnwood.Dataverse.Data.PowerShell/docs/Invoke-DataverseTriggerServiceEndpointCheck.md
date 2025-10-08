---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseTriggerServiceEndpointCheck

## SYNOPSIS
Contains the data that is needed to validate the configuration of a Microsoft Azure Service Bus solution's service endpoint.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.TriggerServiceEndpointCheckRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.TriggerServiceEndpointCheckRequest)

## SYNTAX

```
Invoke-DataverseTriggerServiceEndpointCheck -Connection <ServiceClient> -Entity <PSObject>
```

## DESCRIPTION
Contains the data that is needed to validate the configuration of a Microsoft Azure Service Bus solution's service endpoint.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseTriggerServiceEndpointCheck -Connection <ServiceClient> -Entity <PSObject>
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
Gets or sets the ServiceEndpoint record that contains the configuration. Required. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.TriggerServiceEndpointCheckResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.TriggerServiceEndpointCheckResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.TriggerServiceEndpointCheckResponse)
## NOTES

## RELATED LINKS
