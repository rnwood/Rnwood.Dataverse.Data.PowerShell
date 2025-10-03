# Convert-DataverseQuoteToSalesOrder

## SYNOPSIS
Executes ConvertQuoteToSalesOrderRequest SDK message.

## SYNTAX

```
Convert-DataverseQuoteToSalesOrder -Connection <ServiceClient> [-QuoteId <Guid>] [-QuoteCloseDate <DateTime>] [-QuoteCloseStatus <object>] [-QuoteCloseSubject <String>] [-QuoteCloseDescription <String>] [-ProcessInstanceId <object>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `ConvertQuoteToSalesOrderRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes ConvertQuoteToSalesOrderRequest SDK message.

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
### -QuoteId
Parameter for the ConvertQuoteToSalesOrderRequest operation

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
### -QuoteCloseDate
Parameter for the ConvertQuoteToSalesOrderRequest operation

```yaml
Type: DateTime
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```
### -QuoteCloseStatus
Reference to a Dataverse record. Can be:
- **EntityReference** object from the SDK
- **PSObject** with Id and TableName properties (e.g., from Get-DataverseRecord)
- **Guid** value (requires corresponding TableName parameter)

The cmdlet uses DataverseTypeConverter to handle the conversion automatically.

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
### -QuoteCloseSubject
Parameter for the ConvertQuoteToSalesOrderRequest operation

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
### -QuoteCloseDescription
Parameter for the ConvertQuoteToSalesOrderRequest operation

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
### -ProcessInstanceId
Reference to a Dataverse record. Can be:
- **EntityReference** object from the SDK
- **PSObject** with Id and TableName properties (e.g., from Get-DataverseRecord)
- **Guid** value (requires corresponding TableName parameter)

The cmdlet uses DataverseTypeConverter to handle the conversion automatically.

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
### -WhatIf
Shows what would happen if the cmdlet runs. The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

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

### ConvertQuoteToSalesOrderResponse

Returns the response from the `ConvertQuoteToSalesOrderRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
