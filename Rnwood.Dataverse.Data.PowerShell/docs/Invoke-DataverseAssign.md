---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseAssign

## SYNOPSIS
Contains the data that is needed to assign the specified record to a new owner (user or team) by changing the OwnerId attribute of the record.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AssignRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.AssignRequest)

## SYNTAX

```
Invoke-DataverseAssign -Connection <ServiceClient> -Target <PSObject> -Assignee <PSObject>
```

## DESCRIPTION
Contains the data that is needed to assign the specified record to a new owner (user or team) by changing the OwnerId attribute of the record.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseAssign -Connection <ServiceClient> -Target <PSObject> -Assignee <PSObject>
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

### -Target
Gets or sets the target record to assign to another user or team. Required. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True
Accept wildcard characters: False
```

### -Assignee
Gets or sets the user or team for which you want to assign a record. Required. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.AssignResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AssignResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.AssignResponse)
## NOTES

## RELATED LINKS
