---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseGetAutoNumberSeed

## SYNOPSIS
Executes a GetAutoNumberSeedRequest against the Dataverse organization service.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetAutoNumberSeedRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.GetAutoNumberSeedRequest)

## SYNTAX

```
Invoke-DataverseGetAutoNumberSeed -Connection <ServiceClient> -EntityName <String> -AttributeName <String>
```

## DESCRIPTION
Executes a GetAutoNumberSeedRequest against the Dataverse organization service.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseGetAutoNumberSeed -Connection <ServiceClient> -EntityName <String> -AttributeName <String>
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

### -AttributeName
Gets or sets the AttributeName for the request.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.GetAutoNumberSeedResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetAutoNumberSeedResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.GetAutoNumberSeedResponse)
## NOTES

## RELATED LINKS
