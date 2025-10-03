# Get-DataverseDependentComponents

## SYNOPSIS
Executes RetrieveDependentComponentsRequest SDK message.

## SYNTAX

```
Get-DataverseDependentComponents -Connection <ServiceClient> [-ObjectId <Guid>] [-ComponentType <Int32>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `RetrieveDependentComponentsRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes RetrieveDependentComponentsRequest SDK message.

### Type Conversion

This cmdlet follows the standard type conversion patterns:

- **EntityReference parameters**: Accept EntityReference objects, PSObjects with Id/TableName properties, or Guid values (with corresponding TableName parameter). Conversion handled by DataverseTypeConverter.ToEntityReference().

- **Entity parameters**: Accept PSObjects representing records. Properties map to attribute logical names. Lookup fields accept Guid/EntityReference/PSObject. Choice fields accept numeric values or string labels. Conversion handled by DataverseEntityConverter.

- **OptionSetValue parameters**: Accept numeric option codes or string labels. Conversion handled by DataverseTypeConverter.ToOptionSetValue().

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
### -ObjectId
Parameter for the RetrieveDependentComponentsRequest operation

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
### -ComponentType
Parameter for the RetrieveDependentComponentsRequest operation

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

### RetrieveDependentComponentsResponse

Returns the response from the `RetrieveDependentComponentsRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
