# Initialize-DataverseFrom

## SYNOPSIS
Executes InitializeFromRequest SDK message.

## SYNTAX

```
Initialize-DataverseFrom -Connection <ServiceClient> [-EntityMoniker <object>] [-TargetEntityName <String>] [-SkipParentalRelationshipMapping <Boolean>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `InitializeFromRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes InitializeFromRequest SDK message.

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
### -EntityMoniker
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
### -TargetEntityName
Logical name of the Dataverse table (entity). Required when providing Guid values for record references instead of EntityReference or PSObject.

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
### -SkipParentalRelationshipMapping
Parameter for the InitializeFromRequest operation

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

### InitializeFromResponse

Returns the response from the `InitializeFromRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
