# Get-DataverseLocLabels

## SYNOPSIS
Executes RetrieveLocLabelsRequest SDK message.

## SYNTAX

```
Get-DataverseLocLabels -Connection <ServiceClient> [-EntityMoniker <object>] [-AttributeName <String>] [-IncludeUnpublished <Boolean>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `RetrieveLocLabelsRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes RetrieveLocLabelsRequest SDK message.

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
### -EntityMoniker
Parameter for the RetrieveLocLabelsRequest operation.

```yaml
Type: object
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```
### -AttributeName
Parameter for the RetrieveLocLabelsRequest operation.

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
### -IncludeUnpublished
Parameter for the RetrieveLocLabelsRequest operation.

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

### RetrieveLocLabelsResponse

Returns the response from the `RetrieveLocLabelsRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
