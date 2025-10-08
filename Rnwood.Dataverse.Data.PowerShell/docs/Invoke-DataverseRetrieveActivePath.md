---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrieveActivePath

## SYNOPSIS
Contains the data to retrieve a collection of stages currently in the active path for a business process flow instance.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveActivePathRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveActivePathRequest)

## SYNTAX

```
Invoke-DataverseRetrieveActivePath -Connection <ServiceClient> -ProcessInstanceId <Guid>
```

## DESCRIPTION
Contains the data to retrieve a collection of stages currently in the active path for a business process flow instance.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrieveActivePath -Connection <ServiceClient> -ProcessInstanceId <Guid>
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

### -ProcessInstanceId
Gets or sets the ID of the business process flow instance record to retrieve a collection of stages currently in the active path.

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

### Microsoft.Crm.Sdk.Messages.RetrieveActivePathResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveActivePathResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveActivePathResponse)
## NOTES

## RELATED LINKS
