# Get-DataverseRecord

## SYNOPSIS
Executes the Get-DataverseRecord operation.

## SYNTAX

```
Get-DataverseRecord -Connection <ServiceClient> [-VerboseRecordCount <SwitchParameter>] [-RecordCount <SwitchParameter>] [-FetchXml <string>] [-Criteria <FilterExpression>] [-ExcludeFilterOr <SwitchParameter>] [-ActiveOnly <SwitchParameter>] [-LookupValuesReturnName <SwitchParameter>] [-IncludeSystemColumns <SwitchParameter>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `RetrieveMultipleRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes the Get-DataverseRecord operation.

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
### -VerboseRecordCount
Parameter for the RetrieveMultipleRequest operation

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```
### -RecordCount
Parameter for the RetrieveMultipleRequest operation

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```
### -FetchXml
Parameter for the RetrieveMultipleRequest operation

```yaml
Type: string
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```
### -Criteria
Parameter for the RetrieveMultipleRequest operation

```yaml
Type: FilterExpression
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```
### -ExcludeFilterOr
Parameter for the RetrieveMultipleRequest operation

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```
### -ActiveOnly
Parameter for the RetrieveMultipleRequest operation

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```
### -LookupValuesReturnName
Parameter for the RetrieveMultipleRequest operation

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```
### -IncludeSystemColumns
Parameter for the RetrieveMultipleRequest operation

```yaml
Type: SwitchParameter
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

### OrganizationResponse

Returns the response from the `RetrieveMultipleRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
