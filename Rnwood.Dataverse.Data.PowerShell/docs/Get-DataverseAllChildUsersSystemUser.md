# Get-DataverseAllChildUsersSystemUser

## SYNOPSIS
Executes RetrieveAllChildUsersSystemUserRequest SDK message.

## SYNTAX

```
Get-DataverseAllChildUsersSystemUser -Connection <ServiceClient> [-EntityId <Guid>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `RetrieveAllChildUsersSystemUserRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes RetrieveAllChildUsersSystemUserRequest SDK message.

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
### -EntityId
Parameter for the RetrieveAllChildUsersSystemUserRequest operation.

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
### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### RetrieveAllChildUsersSystemUserResponse

Returns the response from the `RetrieveAllChildUsersSystemUserRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
