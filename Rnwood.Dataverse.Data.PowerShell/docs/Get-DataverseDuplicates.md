# Get-DataverseDuplicates

## SYNOPSIS
Executes RetrieveDuplicatesRequest SDK message.

## SYNTAX

```
Get-DataverseDuplicates -Connection <ServiceClient> [-BusinessEntity <PSObject>] [-MatchingEntityName <String>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `RetrieveDuplicatesRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes RetrieveDuplicatesRequest SDK message.

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
### -BusinessEntity
Parameter for the RetrieveDuplicatesRequest operation.

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
### -MatchingEntityName
Parameter for the RetrieveDuplicatesRequest operation.

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

### RetrieveDuplicatesResponse

Returns the response from the `RetrieveDuplicatesRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
