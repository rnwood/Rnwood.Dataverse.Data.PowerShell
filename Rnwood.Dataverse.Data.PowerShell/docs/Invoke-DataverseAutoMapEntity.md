---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseAutoMapEntity

## SYNOPSIS
Contains the data that is needed to generate a new set of attribute mappings based on the metadata.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AutoMapEntityRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.AutoMapEntityRequest)

## SYNTAX

```
Invoke-DataverseAutoMapEntity -Connection <ServiceClient> -EntityMapId <Guid>
```

## DESCRIPTION
Contains the data that is needed to generate a new set of attribute mappings based on the metadata.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseAutoMapEntity -Connection <ServiceClient> -EntityMapId <Guid>
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

### -EntityMapId
Gets or sets the ID of the entity map to overwrite when the automated mapping is performed. Required.

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

### Microsoft.Crm.Sdk.Messages.AutoMapEntityResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AutoMapEntityResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.AutoMapEntityResponse)
## NOTES

## RELATED LINKS
