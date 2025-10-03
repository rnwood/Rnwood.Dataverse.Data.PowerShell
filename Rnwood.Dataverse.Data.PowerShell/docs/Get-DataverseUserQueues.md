# Get-DataverseUserQueues

## SYNOPSIS
Executes RetrieveUserQueuesRequest SDK message.

## SYNTAX

```
Get-DataverseUserQueues -Connection <ServiceClient> [-UserId <Guid>] [-IncludePublic <Boolean>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `RetrieveUserQueuesRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes RetrieveUserQueuesRequest SDK message.

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
### -UserId
Parameter for the RetrieveUserQueuesRequest operation.

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
### -IncludePublic
Parameter for the RetrieveUserQueuesRequest operation.

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

### RetrieveUserQueuesResponse

Returns the response from the `RetrieveUserQueuesRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
