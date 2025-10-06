---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseTransformImport

## SYNOPSIS
Contains the data that is needed to submit an asynchronous job that transforms the parsed data.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.TransformImportRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.TransformImportRequest)

## SYNTAX

```
Invoke-DataverseTransformImport -Connection <ServiceClient> -ImportId <Guid>
```

## DESCRIPTION
Contains the data that is needed to submit an asynchronous job that transforms the parsed data.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseTransformImport -Connection <ServiceClient> -ImportId <Guid>
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

### -ImportId
Gets or sets the ID of the import (data import) that is associated with the asynchronous job that transforms the imported data. Required.

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

### Microsoft.Crm.Sdk.Messages.TransformImportResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.TransformImportResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.TransformImportResponse)
## NOTES

## RELATED LINKS
