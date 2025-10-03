# Reset-DataverseUserFilters

## SYNOPSIS
Executes ResetUserFiltersRequest SDK message.

## SYNTAX

```
Reset-DataverseUserFilters -Connection <ServiceClient> [-QueryType <Int32>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `ResetUserFiltersRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes ResetUserFiltersRequest SDK message.

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
### -QueryType
Parameter for the ResetUserFiltersRequest operation.

```yaml
Type: Int32
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

### ResetUserFiltersResponse

Returns the response from the `ResetUserFiltersRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
