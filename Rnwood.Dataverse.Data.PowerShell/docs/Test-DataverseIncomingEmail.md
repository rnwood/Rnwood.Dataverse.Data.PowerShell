# Test-DataverseIncomingEmail

## SYNOPSIS
Executes CheckIncomingEmailRequest SDK message.

## SYNTAX

```
Test-DataverseIncomingEmail -Connection <ServiceClient> [-MessageId <String>] [-Subject <String>] [-From <String>] [-To <String>] [-Cc <String>] [-Bcc <String>] [-ExtraProperties <PSObject>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `CheckIncomingEmailRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes CheckIncomingEmailRequest SDK message.

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
### -MessageId
Parameter for the CheckIncomingEmailRequest operation

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
### -Subject
Parameter for the CheckIncomingEmailRequest operation

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
### -From
Parameter for the CheckIncomingEmailRequest operation

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
### -To
Parameter for the CheckIncomingEmailRequest operation

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
### -Cc
Parameter for the CheckIncomingEmailRequest operation

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
### -Bcc
Parameter for the CheckIncomingEmailRequest operation

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
### -ExtraProperties
Parameter for the CheckIncomingEmailRequest operation

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
### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### CheckIncomingEmailResponse

Returns the response from the `CheckIncomingEmailRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
