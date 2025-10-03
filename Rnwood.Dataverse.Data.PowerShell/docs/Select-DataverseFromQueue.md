# Select-DataverseFromQueue

## SYNOPSIS
Executes PickFromQueueRequest SDK message.

## SYNTAX

```
Select-DataverseFromQueue -Connection <ServiceClient> [-QueueItemId <Guid>] [-WorkerId <Guid>] [-RemoveQueueItem <Boolean>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `PickFromQueueRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes PickFromQueueRequest SDK message.

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
### -QueueItemId
Parameter for the PickFromQueueRequest operation.

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
### -WorkerId
Parameter for the PickFromQueueRequest operation.

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
### -RemoveQueueItem
Parameter for the PickFromQueueRequest operation.

```yaml
Type: Boolean
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

### PickFromQueueResponse

Returns the response from the `PickFromQueueRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
