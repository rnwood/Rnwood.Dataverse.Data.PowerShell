# Get-DataverseDistinctValuesImportFile

## SYNOPSIS
Executes GetDistinctValuesImportFileRequest SDK message.

## SYNTAX

```
Get-DataverseDistinctValuesImportFile -Connection <ServiceClient> [-ImportFileId <Guid>] [-columnNumber <Int32>] [-pageNumber <Int32>] [-recordsPerPage <Int32>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `GetDistinctValuesImportFileRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes GetDistinctValuesImportFileRequest SDK message.

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
### -ImportFileId
Parameter for the GetDistinctValuesImportFileRequest operation.

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
### -columnNumber
Parameter for the GetDistinctValuesImportFileRequest operation.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```
### -pageNumber
Parameter for the GetDistinctValuesImportFileRequest operation.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```
### -recordsPerPage
Parameter for the GetDistinctValuesImportFileRequest operation.

```yaml
Type: Int32
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

### GetDistinctValuesImportFileResponse

Returns the response from the `GetDistinctValuesImportFileRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
