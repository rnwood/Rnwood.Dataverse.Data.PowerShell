---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseCommitAnnotationBlocksUpload

## SYNOPSIS
Contains the data needed to commit the uploaded data blocks to the annotation store.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CommitAnnotationBlocksUploadRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.CommitAnnotationBlocksUploadRequest)

## SYNTAX

```
Invoke-DataverseCommitAnnotationBlocksUpload -Connection <ServiceClient> -Target <PSObject> -TargetTableName <String> -TargetIgnoreProperties <String[]> -TargetLookupColumns <Hashtable> -BlockList <String[]> -FileContinuationToken <String>
```

## DESCRIPTION
Contains the data needed to commit the uploaded data blocks to the annotation store.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseCommitAnnotationBlocksUpload -Connection <ServiceClient> -Target <PSObject> -TargetTableName <String> -TargetIgnoreProperties <String[]> -TargetLookupColumns <Hashtable> -BlockList <String[]> -FileContinuationToken <String>
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
Gets or sets the target entity. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type.

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

### -TargetTableName
Gets or sets the target entity. The logical name of the table/entity type for the Target parameter.

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

### -TargetIgnoreProperties
Gets or sets the target entity. Properties to ignore when converting Target PSObject to Entity.

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

### -TargetLookupColumns
Gets or sets the target entity. Hashtable specifying lookup columns for entity reference conversions in Target.

```yaml
Type: Hashtable
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

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

### Microsoft.Crm.Sdk.Messages.CommitAnnotationBlocksUploadResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CommitAnnotationBlocksUploadResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.CommitAnnotationBlocksUploadResponse)
## NOTES

## RELATED LINKS
