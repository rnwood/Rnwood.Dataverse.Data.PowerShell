---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseCommitFileBlocksUpload

## SYNOPSIS
Contains the data needed to commit the uploaded data blocks to the file store.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CommitFileBlocksUploadRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.CommitFileBlocksUploadRequest)

## SYNTAX

```
Invoke-DataverseCommitFileBlocksUpload [-FileName <String>] [-MimeType <String>] [-BlockList <String[]>]
 [-FileContinuationToken <String>] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf]
 [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Contains the data needed to commit the uploaded data blocks to the file store.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseCommitFileBlocksUpload -Connection <ServiceClient> -FileName <String> -MimeType <String> -BlockList <String[]> -FileContinuationToken <String>
```

## PARAMETERS

### -BlockList
Gets or sets the IDs of the uploaded data blocks, in the correct sequence, that will result in the final annotation when the data blocks are combined.

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

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet

```yaml
Type: ServiceClient
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

### -FileName
Gets or sets a filename to associate with the binary data file.

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

### -MimeType
Gets or sets the MIME type of the uploaded file.

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

### -WhatIf
Shows what would happen if the cmdlet runs. The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
{{ Fill ProgressAction Description }}

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
