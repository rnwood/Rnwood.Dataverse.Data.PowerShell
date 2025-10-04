---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseBulkDelete

## SYNOPSIS
Contains the data that's needed to submit a bulk delete job that deletes selected records in bulk. This job runs asynchronously in the background without blocking other activities.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.BulkDeleteRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.BulkDeleteRequest)

## SYNTAX

```
Invoke-DataverseBulkDelete -Connection <ServiceClient> [-QuerySet <QueryExpression[]>] [-JobName <String>]
 [-SendEmailNotification <Boolean>] [-ToRecipients <Guid[]>] [-CCRecipients <Guid[]>]
 [-RecurrencePattern <String>] [-StartDateTime <DateTime>] [-SourceImportId <Guid>] [-RunNow <Boolean>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Contains the data that's needed to submit a bulk delete job that deletes selected records in bulk. This job runs asynchronously in the background without blocking other activities.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseBulkDelete -Connection <ServiceClient> -QuerySet <QueryExpression[]> -JobName <String> -SendEmailNotification <Boolean> -ToRecipients <Guid> -CCRecipients <Guid> -RecurrencePattern <String> -StartDateTime <DateTime> -SourceImportId <Guid> -RunNow <Boolean>
```

## PARAMETERS

### -CCRecipients
Gets or sets an array of IDs for the system users (users) who are listed in the Cc box of the email notification. Required.

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

### -JobName
Gets or sets the name of an asynchronous bulk delete job. Required.

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

### -QuerySet
Gets or sets an array of queries for a bulk delete job. Required.

```yaml
Type: QueryExpression[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RecurrencePattern
Gets or sets the recurrence pattern for the bulk delete job. Optional.

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

### -RunNow
For internal use only.

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

### -SendEmailNotification
Gets or sets a value that indicates whether an email notification is sent after the bulk delete job has finished running. Required.

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

### -SourceImportId
Gets or sets the ID of the data import job. Optional.

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

### -StartDateTime
Gets or sets the start date and time to run a bulk delete job. Optional.

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

### -ToRecipients
Gets or sets an array of IDs for the system users (users) who are listed in the To box of an email notification. Required.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
