# Import-DataverseSolutionAsync

## SYNOPSIS
Executes ImportSolutionAsyncRequest SDK message.

## SYNTAX

```
Import-DataverseSolutionAsync -Connection <ServiceClient> [-OverwriteUnmanagedCustomizations <Boolean>] [-PublishWorkflows <Boolean>] [-ImportJobId <Guid>] [-ConvertToManaged <Boolean>] [-SkipProductUpdateDependencies <Boolean>] [-HoldingSolution <Boolean>] [-SkipQueueRibbonJob <Boolean>] [-AsyncRibbonProcessing <Boolean>] [-IsTemplateMode <Boolean>] [-TemplateSuffix <String>] [-TemplateDisplayNamePrefix <String>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `ImportSolutionAsyncRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes ImportSolutionAsyncRequest SDK message.

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
### -OverwriteUnmanagedCustomizations
Parameter for the ImportSolutionAsyncRequest operation.

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
### -PublishWorkflows
Parameter for the ImportSolutionAsyncRequest operation.

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
### -ImportJobId
Parameter for the ImportSolutionAsyncRequest operation.

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
### -ConvertToManaged
Parameter for the ImportSolutionAsyncRequest operation.

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
### -SkipProductUpdateDependencies
Parameter for the ImportSolutionAsyncRequest operation.

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
### -HoldingSolution
Parameter for the ImportSolutionAsyncRequest operation.

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
### -SkipQueueRibbonJob
Parameter for the ImportSolutionAsyncRequest operation.

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
### -AsyncRibbonProcessing
Parameter for the ImportSolutionAsyncRequest operation.

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
### -IsTemplateMode
Parameter for the ImportSolutionAsyncRequest operation.

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
### -TemplateSuffix
Parameter for the ImportSolutionAsyncRequest operation.

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
### -TemplateDisplayNamePrefix
Parameter for the ImportSolutionAsyncRequest operation.

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

### ImportSolutionAsyncResponse

Returns the response from the `ImportSolutionAsyncRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
