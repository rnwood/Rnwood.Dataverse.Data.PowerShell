# Get-DataverseAttributeChangeHistory

## SYNOPSIS
Executes RetrieveAttributeChangeHistoryRequest SDK message.

## SYNTAX

```
Get-DataverseAttributeChangeHistory -Connection <ServiceClient> [-Target <object>] [-AttributeLogicalName <String>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `RetrieveAttributeChangeHistoryRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes RetrieveAttributeChangeHistoryRequest SDK message.

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
### -Target
Parameter for the RetrieveAttributeChangeHistoryRequest operation.

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
### -AttributeLogicalName
Parameter for the RetrieveAttributeChangeHistoryRequest operation.

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

### RetrieveAttributeChangeHistoryResponse

Returns the response from the `RetrieveAttributeChangeHistoryRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
