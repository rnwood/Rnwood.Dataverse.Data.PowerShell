---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseDistributeCampaignActivity

## SYNOPSIS
Contains the data that is needed to create a bulk operation that distributes a campaign activity. The appropriate activities, such as a phone call or fax, are created for the members of the list that is associated with the specified campaign activity.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DistributeCampaignActivityRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.DistributeCampaignActivityRequest)

## SYNTAX

```
Invoke-DataverseDistributeCampaignActivity [-CampaignActivityId <Guid>] [-Propagate <Boolean>]
 [-Activity <PSObject>] [-ActivityTableName <String>] [-ActivityIgnoreProperties <String[]>]
 [-ActivityLookupColumns <Hashtable>] [-TemplateId <Guid>] [-OwnershipOptions <PropagationOwnershipOptions>]
 [-Owner <PSObject>] [-SendEmail <Boolean>] [-QueueId <Guid>] [-PostWorkflowEvent <Boolean>]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Contains the data that is needed to create a bulk operation that distributes a campaign activity. The appropriate activities, such as a phone call or fax, are created for the members of the list that is associated with the specified campaign activity.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseDistributeCampaignActivity -Connection <ServiceClient> -CampaignActivityId <Guid> -Propagate <Boolean> -Activity <PSObject> -ActivityTableName <String> -ActivityIgnoreProperties <String[]> -ActivityLookupColumns <Hashtable> -TemplateId <Guid> -OwnershipOptions <PropagationOwnershipOptions> -Owner <PSObject> -SendEmail <Boolean> -QueueId <Guid> -PostWorkflowEvent <Boolean>
```

## PARAMETERS

### -Activity
Gets or sets the activity to be distributed. Required. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type.

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

### -ActivityIgnoreProperties
Gets or sets the activity to be distributed. Required. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Properties to ignore when converting Activity PSObject to Entity.

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

### -ActivityLookupColumns
Gets or sets the activity to be distributed. Required. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Hashtable specifying lookup columns for entity reference conversions in Activity.

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

### -ActivityTableName
Gets or sets the activity to be distributed. Required. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. The logical name of the table/entity type for the Activity parameter.

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

### -CampaignActivityId
Gets or sets the ID of the campaign activity. Required.

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

### -Owner
Gets or sets the owner for the activity. Required. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

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

### -OwnershipOptions
Gets or sets the propagation ownership options. Required.

```yaml
Type: PropagationOwnershipOptions
Parameter Sets: (All)
Aliases:
Accepted values: None, Caller, ListMemberOwner

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PostWorkflowEvent
Gets or sets a value that indicates whether an asynchronous job is used to distribute an activity, such as an email, fax, or letter, to the members of a list. Required.

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

### -Propagate
Gets or sets a value that indicates whether the activity is both created and executed. Required.

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

### -QueueId
Gets or sets the ID of the queue. Required

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

### -SendEmail
Gets or sets a value that indicates whether to send an email about the new activity. Required.

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
Sets the ID of the template (email template) that is used for the email notification.

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

### None
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
