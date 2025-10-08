---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseValidate

## SYNOPSIS
Contains the data that is needed to verify that an appointment or service appointment (service activity) has valid available resources for the activity, duration, and site, as appropriate.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ValidateRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ValidateRequest)

## SYNTAX

```
Invoke-DataverseValidate -Connection <ServiceClient> -Activities <EntityCollection>
```

## DESCRIPTION
Contains the data that is needed to verify that an appointment or service appointment (service activity) has valid available resources for the activity, duration, and site, as appropriate.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseValidate -Connection <ServiceClient> -Activities <EntityCollection>
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

### -Activities
Gets or sets the activities to validate.

```yaml
Type: EntityCollection
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

### Microsoft.Crm.Sdk.Messages.ValidateResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ValidateResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ValidateResponse)
## NOTES

## RELATED LINKS
