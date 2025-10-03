# Stop-DataverseOpportunity

## SYNOPSIS
Executes LoseOpportunityRequest SDK message.

## SYNTAX

```
Stop-DataverseOpportunity -Connection <ServiceClient> [-OpportunityClose <PSObject>] [-Status <object>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `LoseOpportunityRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes LoseOpportunityRequest SDK message.

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
### -OpportunityClose
Parameter for the LoseOpportunityRequest operation.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```
### -Status
Parameter for the LoseOpportunityRequest operation.

```yaml
Type: object
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

### LoseOpportunityResponse

Returns the response from the `LoseOpportunityRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
