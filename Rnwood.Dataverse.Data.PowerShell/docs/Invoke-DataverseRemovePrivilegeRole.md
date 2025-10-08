---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRemovePrivilegeRole

## SYNOPSIS
Contains the data that is needed to remove a privilege from an existing role.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RemovePrivilegeRoleRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RemovePrivilegeRoleRequest)

## SYNTAX

```
Invoke-DataverseRemovePrivilegeRole -Connection <ServiceClient> -RoleId <Guid> -PrivilegeId <Guid>
```

## DESCRIPTION
Contains the data that is needed to remove a privilege from an existing role.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRemovePrivilegeRole -Connection <ServiceClient> -RoleId <Guid> -PrivilegeId <Guid>
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
Gets or sets the ID of the role from which the privilege is to be removed.

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

### -PrivilegeId
Gets or sets the ID of the privilege that is to be removed from the existing role.

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

### Microsoft.Crm.Sdk.Messages.RemovePrivilegeRoleResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RemovePrivilegeRoleResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RemovePrivilegeRoleResponse)
## NOTES

## RELATED LINKS
