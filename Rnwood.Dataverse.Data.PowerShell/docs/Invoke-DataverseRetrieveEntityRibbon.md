---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrieveEntityRibbon

## SYNOPSIS
Contains the data that is needed to retrieve ribbon definitions for an entity.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveEntityRibbonRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveEntityRibbonRequest)

## SYNTAX

```
Invoke-DataverseRetrieveEntityRibbon -Connection <ServiceClient> -EntityName <String> -RibbonLocationFilter <RibbonLocationFilters>
```

## DESCRIPTION
Contains the data that is needed to retrieve ribbon definitions for an entity.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrieveEntityRibbon -Connection <ServiceClient> -EntityName <String> -RibbonLocationFilter <RibbonLocationFilters>
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

### -EntityName
Gets or sets the logical name of the entity.

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

### -RibbonLocationFilter
Gets or sets a filter to retrieve a specific set of ribbon definitions for an entity. Required.

```yaml
Type: RibbonLocationFilters
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

### Microsoft.Crm.Sdk.Messages.RetrieveEntityRibbonResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveEntityRibbonResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveEntityRibbonResponse)
## NOTES

## RELATED LINKS
