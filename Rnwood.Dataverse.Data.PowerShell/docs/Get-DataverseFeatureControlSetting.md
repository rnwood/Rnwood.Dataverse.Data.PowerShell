# Get-DataverseFeatureControlSetting

## SYNOPSIS
Executes RetrieveFeatureControlSettingRequest SDK message.

## SYNTAX

```
Get-DataverseFeatureControlSetting -Connection <ServiceClient> [-NamespaceValue <String>] [-FeatureControlName <String>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `RetrieveFeatureControlSettingRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes RetrieveFeatureControlSettingRequest SDK message.

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
### -NamespaceValue
Parameter for the RetrieveFeatureControlSettingRequest operation.

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
### -FeatureControlName
Parameter for the RetrieveFeatureControlSettingRequest operation.

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

### RetrieveFeatureControlSettingResponse

Returns the response from the `RetrieveFeatureControlSettingRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
