# Get-DataverseNextAutoNumberValue1

## SYNOPSIS
Executes GetNextAutoNumberValue1Request SDK message.

## SYNTAX

```
Get-DataverseNextAutoNumberValue1 -Connection <ServiceClient> [-EntityName <String>] [-AttributeName <String>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `GetNextAutoNumberValue1Request` SDK message. It executes the operation through the Dataverse Organization Service.

Executes GetNextAutoNumberValue1Request SDK message.

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
Parameter for the GetNextAutoNumberValue1Request operation.

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
### -AttributeName
Parameter for the GetNextAutoNumberValue1Request operation.

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

### GetNextAutoNumberValue1Response

Returns the response from the `GetNextAutoNumberValue1Request` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
