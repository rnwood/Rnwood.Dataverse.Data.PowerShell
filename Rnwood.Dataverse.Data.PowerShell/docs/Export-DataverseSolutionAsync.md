# Export-DataverseSolutionAsync

## SYNOPSIS
Executes ExportSolutionAsyncRequest SDK message.

## SYNTAX

```
Export-DataverseSolutionAsync -Connection <ServiceClient> [-SolutionName <String>] [-Managed <Boolean>] [-TargetVersion <String>] [-ExportAutoNumberingSettings <Boolean>] [-ExportCalendarSettings <Boolean>] [-ExportCustomizationSettings <Boolean>] [-ExportEmailTrackingSettings <Boolean>] [-ExportGeneralSettings <Boolean>] [-ExportMarketingSettings <Boolean>] [-ExportOutlookSynchronizationSettings <Boolean>] [-ExportRelationshipRoles <Boolean>] [-ExportIsvConfig <Boolean>] [-ExportSales <Boolean>] [-ExportExternalApplications <Boolean>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `ExportSolutionAsyncRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes ExportSolutionAsyncRequest SDK message.

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
### -SolutionName
Parameter for the ExportSolutionAsyncRequest operation.

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
### -Managed
Parameter for the ExportSolutionAsyncRequest operation.

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
### -TargetVersion
Parameter for the ExportSolutionAsyncRequest operation.

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
### -ExportAutoNumberingSettings
Parameter for the ExportSolutionAsyncRequest operation.

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
### -ExportCalendarSettings
Parameter for the ExportSolutionAsyncRequest operation.

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
### -ExportCustomizationSettings
Parameter for the ExportSolutionAsyncRequest operation.

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
### -ExportEmailTrackingSettings
Parameter for the ExportSolutionAsyncRequest operation.

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
### -ExportGeneralSettings
Parameter for the ExportSolutionAsyncRequest operation.

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
### -ExportMarketingSettings
Parameter for the ExportSolutionAsyncRequest operation.

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
### -ExportOutlookSynchronizationSettings
Parameter for the ExportSolutionAsyncRequest operation.

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
### -ExportRelationshipRoles
Parameter for the ExportSolutionAsyncRequest operation.

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
### -ExportIsvConfig
Parameter for the ExportSolutionAsyncRequest operation.

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
### -ExportSales
Parameter for the ExportSolutionAsyncRequest operation.

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
### -ExportExternalApplications
Parameter for the ExportSolutionAsyncRequest operation.

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

### ExportSolutionAsyncResponse

Returns the response from the `ExportSolutionAsyncRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
