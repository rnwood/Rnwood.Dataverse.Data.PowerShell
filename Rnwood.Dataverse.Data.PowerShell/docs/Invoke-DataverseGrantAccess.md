---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseGrantAccess

## SYNOPSIS
Contains the data that is needed to grant a security principal (user, team, or organization) access to the specified record.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GrantAccessRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.GrantAccessRequest)

## SYNTAX

```
Invoke-DataverseGrantAccess -Connection <ServiceClient> -Target <PSObject> -PrincipalAccess <PrincipalAccess>
```

## DESCRIPTION
Contains the data that is needed to grant a security principal (user, team, or organization) access to the specified record.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseGrantAccess -Connection <ServiceClient> -Target <PSObject> -PrincipalAccess <PrincipalAccess>
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
Gets or sets the entity that is the target of the request to grant access. Required. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

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

### -PrincipalAccess
Gets or sets the team or user that is granted access to the specified record. Required.

```yaml
Type: PrincipalAccess
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

### Microsoft.Crm.Sdk.Messages.GrantAccessResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GrantAccessResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.GrantAccessResponse)
## NOTES

## RELATED LINKS
