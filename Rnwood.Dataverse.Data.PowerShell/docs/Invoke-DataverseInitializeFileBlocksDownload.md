---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseInitializeFileBlocksDownload

## SYNOPSIS
Contains the data needed to initialize the download of one or more binary data blocks.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.InitializeFileBlocksDownloadRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.InitializeFileBlocksDownloadRequest)

## SYNTAX

```
Invoke-DataverseInitializeFileBlocksDownload -Connection <ServiceClient> -Target <PSObject> -FileAttributeName <String> -DataSource <String>
```

## DESCRIPTION
Contains the data needed to initialize the download of one or more binary data blocks.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseInitializeFileBlocksDownload -Connection <ServiceClient> -Target <PSObject> -FileAttributeName <String> -DataSource <String>
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

### -Target
Gets or sets the target, which is a recurring appointment master record to which the appointment is converted. Required. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True
Accept wildcard characters: False
```

### -FileAttributeName
Gets or sets the name of the file or image column with the data.

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

### -DataSource
Get or set the data source to use.

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

### Microsoft.Crm.Sdk.Messages.InitializeFileBlocksDownloadResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.InitializeFileBlocksDownloadResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.InitializeFileBlocksDownloadResponse)
## NOTES

## RELATED LINKS
