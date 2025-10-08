---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseCloneMobileOfflineProfile

## SYNOPSIS
For internal use only.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CloneMobileOfflineProfileRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.CloneMobileOfflineProfileRequest)

## SYNTAX

```
Invoke-DataverseCloneMobileOfflineProfile -Connection <ServiceClient> -Source <PSObject>
```

## DESCRIPTION
For internal use only.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseCloneMobileOfflineProfile -Connection <ServiceClient> -Source <PSObject>
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

### -Source
For internal use only. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

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

### Microsoft.Crm.Sdk.Messages.CloneMobileOfflineProfileResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CloneMobileOfflineProfileResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.CloneMobileOfflineProfileResponse)
## NOTES

## RELATED LINKS
