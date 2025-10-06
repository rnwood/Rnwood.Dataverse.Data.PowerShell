---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseSetParentBusinessUnit

## SYNOPSIS
Contains the data that is needed to set the parent business unit for a business unit.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SetParentBusinessUnitRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.SetParentBusinessUnitRequest)

## SYNTAX

```
Invoke-DataverseSetParentBusinessUnit -Connection <ServiceClient> -BusinessUnitId <Guid> -ParentId <Guid>
```

## DESCRIPTION
Contains the data that is needed to set the parent business unit for a business unit.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseSetParentBusinessUnit -Connection <ServiceClient> -BusinessUnitId <Guid> -ParentId <Guid>
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

### -BusinessUnitId
Gets the GUIDGUID of the business unit that the user making the request, also known as the calling user, belongs to.

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

### -ParentId
For internal use only.

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

### Microsoft.Crm.Sdk.Messages.SetParentBusinessUnitResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SetParentBusinessUnitResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.SetParentBusinessUnitResponse)
## NOTES

## RELATED LINKS
