---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseUploadBlock

## SYNOPSIS
Contains the data needed to upload a block of data to storage.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.UploadBlockRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.UploadBlockRequest)

## SYNTAX

```
Invoke-DataverseUploadBlock -Connection <ServiceClient> -BlockId <String> -BlockData <Byte[]> -InFile <String> -FileContinuationToken <String>
```

## DESCRIPTION
Contains the data needed to upload a block of data to storage.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseUploadBlock -Connection <ServiceClient> -BlockId <String> -BlockData <Byte[]> -InFile <String> -FileContinuationToken <String>
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

### -BlockId
A unique identifier for the block of data.

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

### -BlockData
The data block to upload.

```yaml
Type: Byte[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InFile
Gets or sets the path to a file containing the data to upload.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
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

### Microsoft.Crm.Sdk.Messages.UploadBlockResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.UploadBlockResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.UploadBlockResponse)
## NOTES

## RELATED LINKS
