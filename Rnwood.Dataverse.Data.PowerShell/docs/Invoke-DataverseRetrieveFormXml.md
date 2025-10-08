---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrieveFormXml

## SYNOPSIS
For internal use only.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveFormXmlRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveFormXmlRequest)

## SYNTAX

```
Invoke-DataverseRetrieveFormXml -Connection <ServiceClient> -EntityName <String>
```

## DESCRIPTION
For internal use only.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrieveFormXml -Connection <ServiceClient> -EntityName <String>
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.RetrieveFormXmlResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveFormXmlResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveFormXmlResponse)
## NOTES

## RELATED LINKS
