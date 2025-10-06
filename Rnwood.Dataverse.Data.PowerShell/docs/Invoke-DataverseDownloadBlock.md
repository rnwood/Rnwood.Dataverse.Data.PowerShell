---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseDownloadBlock

## SYNOPSIS
Contains the data needed to download a data block.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DownloadBlockRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.DownloadBlockRequest)

## SYNTAX

```
Invoke-DataverseDownloadBlock -Connection <ServiceClient> -Offset <Int64> -BlockLength <Int64> -FileContinuationToken <String>
```

## DESCRIPTION
Contains the data needed to download a data block.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseDownloadBlock -Connection <ServiceClient> -Offset <Int64> -BlockLength <Int64> -FileContinuationToken <String>
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

### -Offset
The offset (in bytes) from the beginning of the block to the first byte of data in the block.

```yaml
Type: Int64
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BlockLength
The size of the block in bytes.

```yaml
Type: Int64
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FileContinuationToken
Gets or sets a token that uniquely identifies a sequence of related data uploads.

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

### Microsoft.Crm.Sdk.Messages.DownloadBlockResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DownloadBlockResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.DownloadBlockResponse)
## NOTES

## RELATED LINKS
