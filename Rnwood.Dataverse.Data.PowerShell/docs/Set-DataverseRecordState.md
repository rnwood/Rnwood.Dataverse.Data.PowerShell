---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseRecordState

## SYNOPSIS
Changes the state and status of a Dataverse record.

## SYNTAX

```
Set-DataverseRecordState -Connection <ServiceClient> -Target <Object> [-TableName <String>] 
 -State <Object> -Status <Object> [-WhatIf] [-Confirm] [-ProgressAction <ActionPreference>] 
 [<CommonParameters>]
```

## DESCRIPTION

This cmdlet changes the state and status of a Dataverse record using the SetStateRequest message.

State and status control the lifecycle of records in Dataverse:
- State represents major phases (e.g., Active = 0, Inactive = 1)
- Status provides finer-grained control within each state
- Valid combinations depend on the table's state/status metadata

Common uses include:
- Activating or deactivating records
- Setting custom status reasons
- Managing workflow-controlled entities

## EXAMPLES

### Example 1
```powershell
PS C:\> Set-DataverseRecordState -Connection $c -Target $accountId -TableName "account" -State 1 -Status 2
```

Sets an account to Inactive state with status code 2.

### Example 2
```powershell
PS C:\> Get-DataverseRecord -Connection $c -TableName account -Id $accountId | 
         Set-DataverseRecordState -Connection $c -State 0 -Status 1
```

Activates an account via pipeline.

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
Reference to the record to update. Can be an EntityReference, a PSObject with Id and TableName properties, or a Guid (requires TableName parameter).

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

### -State
State code value to set. Can be an integer or an OptionSetValue.

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

### -Status
Status code value to set. Can be an integer or an OptionSetValue.

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

### -WhatIf / -Confirm
Standard ShouldProcess parameters.

### -ProgressAction
See standard PS docs.

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Object

Can accept record references from the pipeline via the Target parameter.

## OUTPUTS

### Microsoft.Crm.Sdk.Messages.SetStateResponse

Returns a SetStateResponse object indicating the operation completed successfully.

## NOTES

See https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.setstaterequest?view=dataverse-sdk-latest

## RELATED LINKS
