# Get-DataverseMembersBulkOperation

## SYNOPSIS
Executes RetrieveMembersBulkOperationRequest SDK message.

## SYNTAX

```
Get-DataverseMembersBulkOperation -Connection <ServiceClient> [-BulkOperationId <Guid>] [-BulkOperationSource <Int32>] [-EntitySource <Int32>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `RetrieveMembersBulkOperationRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes RetrieveMembersBulkOperationRequest SDK message.

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
### -BulkOperationId
Parameter for the RetrieveMembersBulkOperationRequest operation.

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
### -BulkOperationSource
Parameter for the RetrieveMembersBulkOperationRequest operation.

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
### -EntitySource
Parameter for the RetrieveMembersBulkOperationRequest operation.

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

### RetrieveMembersBulkOperationResponse

Returns the response from the `RetrieveMembersBulkOperationRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
