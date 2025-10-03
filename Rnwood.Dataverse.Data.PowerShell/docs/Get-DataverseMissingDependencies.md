# Get-DataverseMissingDependencies

## SYNOPSIS
Executes RetrieveMissingDependenciesRequest SDK message.

## SYNTAX

```
Get-DataverseMissingDependencies -Connection <ServiceClient> [-SolutionUniqueName <String>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `RetrieveMissingDependenciesRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes RetrieveMissingDependenciesRequest SDK message.

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
### -SolutionUniqueName
Parameter for the RetrieveMissingDependenciesRequest operation.

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

### RetrieveMissingDependenciesResponse

Returns the response from the `RetrieveMissingDependenciesRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
