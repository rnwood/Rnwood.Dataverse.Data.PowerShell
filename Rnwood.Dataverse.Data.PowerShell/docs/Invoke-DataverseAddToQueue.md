---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseAddToQueue

## SYNOPSIS
Contains the data that is needed to move an entity record from a source queue to a destination queue.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AddToQueueRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.AddToQueueRequest?view=dataverse-sdk-latest)

## SYNTAX

```
Invoke-DataverseAddToQueue -Target <PSObject> [-SourceQueueId <Guid>] [-DestinationQueueId <Guid>]
 [-QueueItemProperties <PSObject>] [-QueueItemPropertiesTableName <String>]
 [-QueueItemPropertiesIgnoreProperties <String[]>] [-QueueItemPropertiesLookupColumns <Hashtable>]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Contains the data that is needed to move an entity record from a source queue to a destination queue.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseAddToQueue -Connection <ServiceClient> -Target <PSObject> -SourceQueueId <Guid> -DestinationQueueId <Guid> -QueueItemProperties <PSObject> -QueueItemPropertiesTableName <String> -QueueItemPropertiesIgnoreProperties <String[]> -QueueItemPropertiesLookupColumns <Hashtable>
```

## PARAMETERS

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

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DestinationQueueId
Gets or sets the ID of the destination queue. Required.

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

### -QueueItemProperties
Gets or sets the properties that are needed to create a queue item in the destination queue. Optional. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type.

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

### -QueueItemPropertiesIgnoreProperties
Gets or sets the properties that are needed to create a queue item in the destination queue. Optional. Properties to ignore when converting QueueItemProperties PSObject to Entity.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -QueueItemPropertiesLookupColumns
Gets or sets the properties that are needed to create a queue item in the destination queue. Optional. Hashtable specifying lookup columns for entity reference conversions in QueueItemProperties.

```yaml
Type: Hashtable
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -QueueItemPropertiesTableName
Gets or sets the properties that are needed to create a queue item in the destination queue. Optional. The logical name of the table/entity type for the QueueItemProperties parameter.

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

### -SourceQueueId
Gets or sets the ID of the source queue. Optional.

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

### -Target
Gets or sets the target, which is a recurring appointment master record to which the appointment is converted. Required. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
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

### -ProgressAction
{{ Fill ProgressAction Description }}

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
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
