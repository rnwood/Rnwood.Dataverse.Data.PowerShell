---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRemoveUserFromRecordTeam

## SYNOPSIS
Contains the data that is needed to remove a user from the auto created access team for the specified record.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RemoveUserFromRecordTeamRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RemoveUserFromRecordTeamRequest)

## SYNTAX

```
Invoke-DataverseRemoveUserFromRecordTeam -Connection <ServiceClient> -Record <PSObject> -TeamTemplateId <Guid> -SystemUserId <Guid>
```

## DESCRIPTION
Contains the data that is needed to remove a user from the auto created access team for the specified record.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRemoveUserFromRecordTeam -Connection <ServiceClient> -Record <PSObject> -TeamTemplateId <Guid> -SystemUserId <Guid>
```

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

### -Record
Gets or sets the record for which the access team is auto created. Required. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

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

### -TeamTemplateId
Gets or sets the ID of team template which is used to create the access team. Required.

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

### -SystemUserId
Gets or sets the ID of system user (user) to add to the auto created access team. Required.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.RemoveUserFromRecordTeamResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RemoveUserFromRecordTeamResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RemoveUserFromRecordTeamResponse)
## NOTES

## RELATED LINKS
