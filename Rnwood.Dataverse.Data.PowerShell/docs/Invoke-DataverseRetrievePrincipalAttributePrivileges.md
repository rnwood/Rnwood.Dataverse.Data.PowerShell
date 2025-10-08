---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrievePrincipalAttributePrivileges

## SYNOPSIS
Contains the data that is needed to retrieves all the secured attribute privileges a user or team has through direct or indirect (through team membership) associations with the entity.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrievePrincipalAttributePrivilegesRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrievePrincipalAttributePrivilegesRequest)

## SYNTAX

```
Invoke-DataverseRetrievePrincipalAttributePrivileges -Connection <ServiceClient> -Principal <PSObject>
```

## DESCRIPTION
Contains the data that is needed to retrieves all the secured attribute privileges a user or team has through direct or indirect (through team membership) associations with the entity.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrievePrincipalAttributePrivileges -Connection <ServiceClient> -Principal <PSObject>
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

### -Principal
Gets or sets the security principal (user, team, or organization) for which to retrieve attribute privileges. Required. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

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

### Microsoft.Crm.Sdk.Messages.RetrievePrincipalAttributePrivilegesResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrievePrincipalAttributePrivilegesResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrievePrincipalAttributePrivilegesResponse)
## NOTES

## RELATED LINKS
