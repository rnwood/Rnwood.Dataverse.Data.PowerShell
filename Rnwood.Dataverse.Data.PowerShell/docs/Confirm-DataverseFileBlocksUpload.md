# Confirm-DataverseFileBlocksUpload

## SYNOPSIS
Executes CommitFileBlocksUploadRequest SDK message.

## SYNTAX

```
Confirm-DataverseFileBlocksUpload -Connection <ServiceClient> [-FileName <String>] [-MimeType <String>] [-FileContinuationToken <String>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `CommitFileBlocksUploadRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes CommitFileBlocksUploadRequest SDK message.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet.

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
### -FileName
Parameter for the CommitFileBlocksUploadRequest operation.

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
Parameter for the CommitFileBlocksUploadRequest operation.

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
### -FileContinuationToken
Parameter for the CommitFileBlocksUploadRequest operation.

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

## INPUTS

### None

## OUTPUTS

### CommitFileBlocksUploadResponse

Returns the response from the `CommitFileBlocksUploadRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
