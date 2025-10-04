---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseRecord

## SYNOPSIS
Deletes an existing Dataverse record.

## SYNTAX

```
Remove-DataverseRecord -Connection <ServiceClient> [-InputObject <PSObject>] -TableName <String> -Id <Guid>
 [-BatchSize <UInt32>] [-IfExists] [-BypassBusinessLogicExecution <BusinessLogicTypes[]>]
 [-BypassBusinessLogicExecutionStepIds <Guid[]>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

## DESCRIPTION
The `TableName` and `Id` can be read from the pipeline when piping in a record obtained from `get-dataverserecord` instead of being specified separately. This allows you to delete a stream of records from the pipeline.

## EXAMPLES

### Example 1
```powershell
PS C:\> get-dataverserecord -connection $c -tablename contact | remove-dataverserecord -connection $c
```

Deletes all contact records.

### Example 2
```powershell
PS C:\> remove-dataverserecord -connection $c -tablename contact -id 4CE66D51-C605-4429-8565-8C7AFA4B9550
```

Deletes the single contact with the specified ID.

## PARAMETERS

### -BatchSize
Controls the maximum number of requests sent to Dataverse in one batch (where possible) to improve throughput. Specify 1 to disable.

When value is 1, requests are sent to Dataverse one request at a time. When > 1, batching is used. 

Note that the batch will continue on error and any errors will be returned once the batch has completed. The error contains the input record to allow correlation.

```yaml
Type: UInt32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BypassBusinessLogicExecution
Specifies the types of business logic (for example plugins) to bypass

```yaml
Type: BusinessLogicTypes[]
Parameter Sets: (All)
Aliases:
Accepted values: CustomSync, CustomAsync

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BypassBusinessLogicExecutionStepIds
Specifies the IDs of plugin steps to bypass

```yaml
Type: Guid[]
Parameter Sets: (All)
Aliases:

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

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet

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

### -Id
Id of record to process

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -IfExists
If specified, the cmdlet will not raise an error if the record does not exist.

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

### -InputObject
Record from pipeline. This allows piping in record to delete.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -TableName
Name of entity

```yaml
Type: String
Parameter Sets: (All)
Aliases: EntityName

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -WhatIf
Shows what would happen if the cmdlet runs.
The cmdlet is not run.

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

### -ProgressAction
See standard PS docs.

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Management.Automation.PSObject
### System.String
### System.Guid
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
