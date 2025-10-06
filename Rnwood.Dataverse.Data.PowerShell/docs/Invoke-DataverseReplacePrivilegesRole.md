---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseReplacePrivilegesRole

## SYNOPSIS
Contains the data that is needed to replace the privilege set of an existing role.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ReplacePrivilegesRoleRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ReplacePrivilegesRoleRequest)

## SYNTAX

```
Invoke-DataverseReplacePrivilegesRole -Connection <ServiceClient> -RoleId <Guid> -Privileges <RolePrivilege[]>
```

## DESCRIPTION
Contains the data that is needed to replace the privilege set of an existing role.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseReplacePrivilegesRole -Connection <ServiceClient> -RoleId <Guid> -Privileges <RolePrivilege[]>
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

### -RoleId
Gets or sets the ID of the role for which the privileges are to be replaced.

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

### -Privileges
Use this to retrieve entity information plus privileges for the entity. Value = 4.

```yaml
Type: RolePrivilege[]
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

### Microsoft.Crm.Sdk.Messages.ReplacePrivilegesRoleResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ReplacePrivilegesRoleResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ReplacePrivilegesRoleResponse)
## NOTES

## RELATED LINKS
