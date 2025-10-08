---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseFetchXmlToQueryExpression

## SYNOPSIS
Contains the data that is needed to convert a query in FetchXML to a QueryExpression.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.FetchXmlToQueryExpressionRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.FetchXmlToQueryExpressionRequest)

## SYNTAX

```
Invoke-DataverseFetchXmlToQueryExpression -Connection <ServiceClient> -FetchXml <String>
```

## DESCRIPTION
Contains the data that is needed to convert a query in FetchXML to a QueryExpression.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseFetchXmlToQueryExpression -Connection <ServiceClient> -FetchXml <String>
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
Gets or sets the query to convert.

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

### Microsoft.Crm.Sdk.Messages.FetchXmlToQueryExpressionResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.FetchXmlToQueryExpressionResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.FetchXmlToQueryExpressionResponse)
## NOTES

## RELATED LINKS
