---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRevokeAccess

## SYNOPSIS
Contains the data that is needed to replace the access rights on the target record for the specified security principal (user, team, or organization).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RevokeAccessRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RevokeAccessRequest)

## SYNTAX

```
Invoke-DataverseRevokeAccess -Connection <ServiceClient> -Target <PSObject> -Revokee <PSObject>
```

## DESCRIPTION
Contains the data that is needed to replace the access rights on the target record for the specified security principal (user, team, or organization).

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRevokeAccess -Connection <ServiceClient> -Target <PSObject> -Revokee <PSObject>
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
Gets or sets the target, which is a recurring appointment master record to which the appointment is converted. Required. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

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

### -Revokee
Gets or sets a security principal (user, team, or organization) whose access you want to revoke. Required. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

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

### Microsoft.Crm.Sdk.Messages.RevokeAccessResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RevokeAccessResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RevokeAccessResponse)
## NOTES

## RELATED LINKS
