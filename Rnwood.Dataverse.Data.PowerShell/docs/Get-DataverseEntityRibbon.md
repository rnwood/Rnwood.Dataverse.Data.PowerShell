# Get-DataverseEntityRibbon

## SYNOPSIS
Executes RetrieveEntityRibbonRequest SDK message.

## SYNTAX

```
Get-DataverseEntityRibbon -Connection <ServiceClient> [-EntityName <String>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `RetrieveEntityRibbonRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes RetrieveEntityRibbonRequest SDK message.

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
### -EntityName
Parameter for the RetrieveEntityRibbonRequest operation.

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

### RetrieveEntityRibbonResponse

Returns the response from the `RetrieveEntityRibbonRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
