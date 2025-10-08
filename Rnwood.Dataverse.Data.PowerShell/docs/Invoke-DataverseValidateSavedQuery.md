---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseValidateSavedQuery

## SYNOPSIS
Contains the data that is needed to validate a saved query (view).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ValidateSavedQueryRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ValidateSavedQueryRequest)

## SYNTAX

```
Invoke-DataverseValidateSavedQuery -Connection <ServiceClient> -FetchXml <String> -QueryType <Int32>
```

## DESCRIPTION
Contains the data that is needed to validate a saved query (view).

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseValidateSavedQuery -Connection <ServiceClient> -FetchXml <String> -QueryType <Int32>
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

### -FetchXml
Gets or sets the FetchXML query string to be validated.

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

### -QueryType
Gets or sets the type of the query.

```yaml
Type: Int32
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

### Microsoft.Crm.Sdk.Messages.ValidateSavedQueryResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ValidateSavedQueryResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ValidateSavedQueryResponse)
## NOTES

## RELATED LINKS
