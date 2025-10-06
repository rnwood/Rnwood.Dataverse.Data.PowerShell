---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataversePublishXml

## SYNOPSIS
Contains the data that is needed to publish specified solution components.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.PublishXmlRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.PublishXmlRequest)

## SYNTAX

```
Invoke-DataversePublishXml -Connection <ServiceClient> -ParameterXml <String>
```

## DESCRIPTION
Contains the data that is needed to publish specified solution components.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataversePublishXml -Connection <ServiceClient> -ParameterXml <String>
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

### -ParameterXml
Gets or sets the XML that defines which solution components to publish in this request. Required.

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

### Microsoft.Crm.Sdk.Messages.PublishXmlResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.PublishXmlResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.PublishXmlResponse)
## NOTES

## RELATED LINKS
