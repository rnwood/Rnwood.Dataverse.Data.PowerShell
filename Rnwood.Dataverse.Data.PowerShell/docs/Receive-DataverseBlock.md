# Receive-DataverseBlock

## SYNOPSIS
Executes DownloadBlockRequest SDK message.

## SYNTAX

```
Receive-DataverseBlock -Connection <ServiceClient> [-Offset <Int64>] [-BlockLength <Int64>] [-FileContinuationToken <String>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `DownloadBlockRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes DownloadBlockRequest SDK message.

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
### -Offset
Parameter for the DownloadBlockRequest operation.

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
Parameter for the DownloadBlockRequest operation.

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
Parameter for the DownloadBlockRequest operation.

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

### DownloadBlockResponse

Returns the response from the `DownloadBlockRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
