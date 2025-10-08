---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseReassignObjectsOwner

## SYNOPSIS
Contains the data that is needed to reassign all records that are owned by the security principal (user, team, or organization) to another security principal (user, team, or organization).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ReassignObjectsOwnerRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ReassignObjectsOwnerRequest)

## SYNTAX

```
Invoke-DataverseReassignObjectsOwner -Connection <ServiceClient> -FromPrincipal <PSObject> -ToPrincipal <PSObject>
```

## DESCRIPTION
Contains the data that is needed to reassign all records that are owned by the security principal (user, team, or organization) to another security principal (user, team, or organization).

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseReassignObjectsOwner -Connection <ServiceClient> -FromPrincipal <PSObject> -ToPrincipal <PSObject>
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

### -FromPrincipal
Gets or sets the security principal (user, team, or organization) for which to reassign all records. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

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

### -ToPrincipal
Gets or sets the security principal (user, team, or organization) that will be the new owner for the records. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

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

### Microsoft.Crm.Sdk.Messages.ReassignObjectsOwnerResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ReassignObjectsOwnerResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ReassignObjectsOwnerResponse)
## NOTES

## RELATED LINKS
