# Complete-DataverseOpportunity

## SYNOPSIS
Executes WinOpportunityRequest SDK message.

## SYNTAX

```
Complete-DataverseOpportunity -Connection <ServiceClient> [-OpportunityClose <PSObject>] [-Status <object>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `WinOpportunityRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes WinOpportunityRequest SDK message.

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
Parameter for the WinOpportunityRequest operation.

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
Parameter for the WinOpportunityRequest operation.

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

### WinOpportunityResponse

Returns the response from the `WinOpportunityRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
