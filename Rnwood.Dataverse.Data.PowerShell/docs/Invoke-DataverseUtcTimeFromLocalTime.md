# Invoke-DataverseUtcTimeFromLocalTime

## SYNOPSIS
Executes UtcTimeFromLocalTimeRequest SDK message.

## SYNTAX

```
Invoke-DataverseUtcTimeFromLocalTime -Connection <ServiceClient> [-TimeZoneCode <Int32>] [-LocalTime <DateTime>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `UtcTimeFromLocalTimeRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes UtcTimeFromLocalTimeRequest SDK message.

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
### -TimeZoneCode
Parameter for the UtcTimeFromLocalTimeRequest operation.

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
### -LocalTime
Parameter for the UtcTimeFromLocalTimeRequest operation.

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

### UtcTimeFromLocalTimeResponse

Returns the response from the `UtcTimeFromLocalTimeRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
