# Group-DataverseDetectDuplicates

## SYNOPSIS
Executes BulkDetectDuplicatesRequest SDK message.

## SYNTAX

```
Group-DataverseDetectDuplicates -Connection <ServiceClient> [-JobName <String>] [-SendEmailNotification <Boolean>] [-TemplateId <Guid>] [-RecurrencePattern <String>] [-RecurrenceStartTime <DateTime>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `BulkDetectDuplicatesRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes BulkDetectDuplicatesRequest SDK message.

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
### -JobName
Parameter for the BulkDetectDuplicatesRequest operation.

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
### -SendEmailNotification
Parameter for the BulkDetectDuplicatesRequest operation.

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
### -TemplateId
Parameter for the BulkDetectDuplicatesRequest operation.

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
### -RecurrencePattern
Parameter for the BulkDetectDuplicatesRequest operation.

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
### -RecurrenceStartTime
Parameter for the BulkDetectDuplicatesRequest operation.

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
### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### BulkDetectDuplicatesResponse

Returns the response from the `BulkDetectDuplicatesRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
