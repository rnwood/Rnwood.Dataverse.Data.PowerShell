---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseRecordOwner

## SYNOPSIS
Assigns a record to a different user or team.

## SYNTAX

```
Set-DataverseRecordOwner -Connection <ServiceClient> -Target <Object> [-TableName <String>] 
 -Assignee <Object> [-AssigneeTableName <String>] [-WhatIf] [-Confirm] 
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet assigns ownership of a Dataverse record to a different user or team using the AssignRequest message.

Assignment is commonly used to:
- Route records to different users or teams
- Transfer ownership when users leave or change roles
- Implement custom assignment logic based on business rules

The target record and assignee can be specified using:
- EntityReference objects
- PSObjects with Id and TableName properties (e.g., from Get-DataverseRecord)
- Guid values (requires TableName/AssigneeTableName parameters)

## EXAMPLES

### Example 1
```powershell
PS C:\> Set-DataverseRecordOwner -Connection $c -Target $recordId -TableName "account" -Assignee $userId -AssigneeTableName "systemuser"
```

Assigns an account record to a specific user using Guid values.

### Example 2
```powershell
PS C:\> $record = Get-DataverseRecord -Connection $c -TableName account -Id $accountId
PS C:\> $user = Get-DataverseRecord -Connection $c -TableName systemuser -Id $userId
PS C:\> Set-DataverseRecordOwner -Connection $c -Target $record -Assignee $user
```

Assigns an account to a user using PSObjects from Get-DataverseRecord.

### Example 3
```powershell
PS C:\> Get-DataverseRecord -Connection $c -TableName account -Filter @{ownerid=$oldUserId} | 
         Set-DataverseRecordOwner -Connection $c -Assignee $newUserId -AssigneeTableName "systemuser"
```

Reassigns all accounts owned by one user to another user via pipeline.

## PARAMETERS

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

### -Target
Reference to the record to assign. Can be an EntityReference, a PSObject with Id and TableName properties, or a Guid (requires TableName parameter).

```yaml
Type: Object
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -TableName
Logical name of the table when Target is specified as a Guid

```yaml
Type: String
Parameter Sets: (All)
Aliases: EntityName, LogicalName

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Assignee
Reference to the user or team to assign the record to. Can be an EntityReference, a PSObject with Id and TableName properties, or a Guid (requires AssigneeTableName parameter).

```yaml
Type: Object
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -AssigneeTableName
Logical name of the assignee table (systemuser or team) when Assignee is specified as a Guid

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

### System.Object

Can accept record references from the pipeline via the Target parameter.

## OUTPUTS

### Microsoft.Crm.Sdk.Messages.AssignResponse

Returns an AssignResponse object indicating the operation completed successfully.

## NOTES

See https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.assignrequest?view=dataverse-sdk-latest

## RELATED LINKS
