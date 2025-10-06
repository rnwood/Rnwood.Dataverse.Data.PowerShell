---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrieveTotalRecordCount

## SYNOPSIS
Contains the data to retrieve the total entity record count from within the last 24 hours.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveTotalRecordCountRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveTotalRecordCountRequest)

## SYNTAX

```
Invoke-DataverseRetrieveTotalRecordCount -Connection <ServiceClient> -EntityNames <String[]>
```

## DESCRIPTION
Contains the data to retrieve the total entity record count from within the last 24 hours.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrieveTotalRecordCount -Connection <ServiceClient> -EntityNames <String[]>
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

### -EntityNames
Gets an array of table logical names that can have many-to-many entity relationships.

```yaml
Type: String[]
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

### Microsoft.Crm.Sdk.Messages.RetrieveTotalRecordCountResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveTotalRecordCountResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveTotalRecordCountResponse)
## NOTES

## RELATED LINKS
